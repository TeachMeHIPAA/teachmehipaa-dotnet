using System.Text;
using System.Text.Json.Nodes;
using TeachMeHipaa.Generator;
using TeachMeHipaa.Tests.Support;

namespace TeachMeHipaa.Tests;

/// <summary>
/// <c>WebhookUtility.ConstructEvent</c> and <c>GenerateTestHeaders</c>, tested
/// with the shared cross-SDK vector (<c>_shared/webhook-vector.json</c>).
/// </summary>
/// <remarks>
/// The vector was signed by the platform's own signer, not this SDK:
/// <c>bun platform-sign.ts</c> run in the monorepo calls packages/auth-s2s
/// <c>signRequest</c> on packages/webhooks <c>canonicalizeEventBody(envelope)</c>,
/// and a node:crypto-only replica of auth-s2s canonical.ts produced the same hex.
/// </remarks>
public class WebhookTests
{
    private const string Secret = "4l28+FYKZo2O57actVGINwR/Xyhzt3OtqCipV81/Zd0=";
    private const string KeyId = "WHSEC-7KQ2-M4XD-9RTB-H3VN";
    private const string Path = "/hooks/tmh?source=teachmehipaa";
    private const string EventTimestamp = "2026-11-15T16:31:32.123Z";
    private const string EventBody =
        """{"data":{"courseId":"hipaa-basics-2026","email":"zoë.müller@example.com","externalId":"hr-123","learnerId":"lrn_4h7j2k","occurredAt":"2026-11-15T16:31:30.000Z","status":"certified"},"eventId":"evt_01JCZ8Y2K5V3M7Q9R4T6W8X0AB","eventType":"learner.certified","occurredAt":"2026-11-15T16:31:30.000Z","orgId":"org_9f3k2m"}""";
    private const string EventBodyHash = "0a849105d2fea53433e3284d0dc6dee83ebccdf6e64088bf34bf3b537b6e640b";
    private const string EventSignature = "c2a0fd14bca6c361c44e84a5b3bf0378b00c49a533636a229c2b6972073b618a";

    // What a signer that wrongly base64-decodes the Signing secret produces for
    // the same Delivery (vector/replica.mjs, `Buffer.from(secret, "base64")`).
    private const string SignatureWithDecodedKey = "26c88e3993dc44d1f52949f6bc1a9061c5e3e9c0b12caf04934e2e4ea3814f2d";

    // The vector's Test delivery, signed the same way: the body the platform's
    // `:test` route builds, canonicalized.
    private const string TestDeliveryTimestamp = "2026-11-15T16:40:05.000Z";
    private const string TestDeliveryBody = """{"subscriptionId":"whs_2b8f4c1d","test":true}""";
    private const string TestDeliveryBodyHash = "42879a05e13a5dfba51eee07c2fef4814ec6009091f1389a8db4403661ce6a66";
    private const string TestDeliverySignature = "b7baa3ae58a738d025adf4e27ce7af833ac6085f5cb1cc5fc8ff1addb191c3b9";

    private static readonly DateTimeOffset AtEventTime = At("2026-11-15T16:31:40.000Z");

    private static DateTimeOffset At(string timestamp) =>
        DateTimeOffset.Parse(timestamp, System.Globalization.CultureInfo.InvariantCulture);

    private static Dictionary<string, string> EventHeaders() => new()
    {
        ["x-tmh-key-id"] = KeyId,
        ["x-tmh-timestamp"] = EventTimestamp,
        ["x-tmh-signature"] = EventSignature,
        ["x-tmh-body-hash"] = EventBodyHash,
        ["x-tmh-event-type"] = "learner.certified",
        ["x-tmh-event-id"] = "evt_01JCZ8Y2K5V3M7Q9R4T6W8X0AB",
        ["content-type"] = "application/json",
    };

    private static Dictionary<string, string> EventHeadersWithout(string name)
    {
        var headers = EventHeaders();
        headers.Remove(name);
        return headers;
    }

    private static Dictionary<string, string> EventHeadersWith(string name, string value)
    {
        var headers = EventHeaders();
        headers[name] = value;
        return headers;
    }

    private static WebhookSignatureException Rejection(
        string payload,
        Dictionary<string, string> headers,
        string secret = Secret,
        string path = Path,
        TimeSpan? tolerance = null,
        DateTimeOffset? receivedAt = null)
    {
        var error = Assert.Throws<WebhookSignatureException>(
            () => WebhookUtility.ConstructEvent(payload, headers, secret, path, tolerance, receivedAt ?? AtEventTime));
        Assert.IsAssignableFrom<TeachMeHipaaException>(error);
        // ADR-0007: the body may hold PHI, so a rejection never carries it.
        Assert.Null(error.RawBody);
        Assert.Empty(error.Headers);
        Assert.Null(error.StatusCode);
        Assert.DoesNotContain("hr-12", error.Message, StringComparison.Ordinal);
        return error;
    }

    private static readonly string TamperedEventBody = EventBody.Replace("hr-123", "hr-124", StringComparison.Ordinal);

    [Fact]
    public void VerifiesADeliverySignedByThePlatformAndReturnsItsTypedEvent()
    {
        var delivery = WebhookUtility.ConstructEvent(EventBody, EventHeaders(), Secret, Path, receivedAt: AtEventTime);

        Assert.False(delivery.IsTestDelivery);
        Assert.Null(delivery.TestDelivery);
        var certified = Assert.IsType<EventLearnerCertified>(delivery.Event);
        Assert.Equal("evt_01JCZ8Y2K5V3M7Q9R4T6W8X0AB", certified.EventId);
        Assert.Equal("learner.certified", certified.EventType);
        Assert.Equal("org_9f3k2m", certified.OrgId);
        Assert.Equal(new DateTimeOffset(2026, 11, 15, 16, 31, 30, TimeSpan.Zero), certified.OccurredAt);
        Assert.Equal("lrn_4h7j2k", certified.Data.LearnerId);
        Assert.Equal("zoë.müller@example.com", certified.Data.Email);
        Assert.Equal("hr-123", certified.Data.ExternalId);
        Assert.Equal("hipaa-basics-2026", certified.Data.CourseId);
        Assert.Equal("certified", certified.Data.Status.Value);
    }

    [Fact]
    public void RejectsATamperedBody()
    {
        var error = Rejection(TamperedEventBody, EventHeaders());

        Assert.Equal(WebhookSignatureException.BodyHashMismatch, error.Code);
        Assert.Contains("raw request body", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsATamperedBodyWhenNoBodyHashHeaderCameWithIt()
    {
        var error = Rejection(TamperedEventBody, EventHeadersWithout("x-tmh-body-hash"));

        Assert.Equal(WebhookSignatureException.SignatureMismatch, error.Code);
    }

    [Fact]
    public void RejectsADeliveryVerifiedAgainstTheWrongPath()
    {
        var error = Rejection(EventBody, EventHeaders(), path: "/hooks/tmh");

        Assert.Equal("signature_mismatch", error.Code);
        Assert.Contains("exact request path", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsADeliveryVerifiedWithTheWrongSigningSecret()
    {
        var error = Rejection(EventBody, EventHeaders(), secret: "Zm9vYmFyYmF6cXV4cXV1eGNvcmdlZ3JhdWx0Z2FycGx5");

        Assert.Equal("signature_mismatch", error.Code);
    }

    [Fact]
    public void RejectsASignatureMadeWithTheBase64DecodedSigningSecret()
    {
        var error = Rejection(EventBody, EventHeadersWith("x-tmh-signature", SignatureWithDecodedKey));

        Assert.Equal("signature_mismatch", error.Code);
    }

    [Fact]
    public void RejectsAnUppercaseSignature()
    {
        var error = Rejection(EventBody, EventHeadersWith("x-tmh-signature", EventSignature.ToUpperInvariant()));

        Assert.Equal("signature_mismatch", error.Code);
    }

    [Theory]
    [InlineData("x-tmh-signature")]
    [InlineData("x-tmh-timestamp")]
    public void RejectsADeliveryWithoutARequiredHeader(string name)
    {
        var error = Rejection(EventBody, EventHeadersWithout(name));

        Assert.Equal(WebhookSignatureException.MissingHeader, error.Code);
        Assert.Contains(name, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TreatsAnEmptySignatureHeaderAsMissing()
    {
        var error = Rejection(EventBody, EventHeadersWith("x-tmh-signature", ""));

        Assert.Equal("missing_header", error.Code);
    }

    [Fact]
    public void RejectsADeliveryWhoseTimestampIsMoreThanFiveMinutesOld()
    {
        var error = Rejection(EventBody, EventHeaders(), receivedAt: At("2026-11-15T16:36:32.124Z"));

        Assert.Equal(WebhookSignatureException.TimestampOutsideTolerance, error.Code);
    }

    [Fact]
    public void RejectsADeliveryWhoseTimestampIsMoreThanFiveMinutesInTheFuture()
    {
        var error = Rejection(EventBody, EventHeaders(), receivedAt: At("2026-11-15T16:26:32.122Z"));

        Assert.Equal("timestamp_outside_tolerance", error.Code);
    }

    [Theory]
    [InlineData("2026-11-15T16:36:32.123Z")]
    [InlineData("2026-11-15T16:26:32.123Z")]
    public void AcceptsADeliveryExactlyFiveMinutesOff(string receivedAt)
    {
        var delivery = WebhookUtility.ConstructEvent(EventBody, EventHeaders(), Secret, Path, receivedAt: At(receivedAt));

        Assert.IsType<EventLearnerCertified>(delivery.Event);
    }

    [Fact]
    public void HonorsACustomTolerance()
    {
        var error = Rejection(EventBody, EventHeaders(), tolerance: TimeSpan.FromSeconds(5));

        Assert.Equal("timestamp_outside_tolerance", error.Code);
    }

    [Fact]
    public void ChecksTheSignatureBeforeTheTimestampSoAForgeryNeverLooksMerelyLate()
    {
        var error = Rejection(
            TamperedEventBody,
            EventHeadersWithout("x-tmh-body-hash"),
            receivedAt: At("2027-01-01T00:00:00.000Z"));

        Assert.Equal("signature_mismatch", error.Code);
    }

    [Theory]
    [InlineData("1763224292")]
    [InlineData("2026-11-15T16:31:32.123")]
    [InlineData("2026-11-15 16:31:32Z")]
    [InlineData("2026-13-15T16:31:32.123Z")]
    public void RejectsATimestampThatIsNotAnIso8601DateTimeWithAnOffset(string timestamp)
    {
        var error = Rejection(EventBody, EventHeadersWith("x-tmh-timestamp", timestamp));

        Assert.Equal(WebhookSignatureException.InvalidTimestamp, error.Code);
    }

    [Fact]
    public void RefusesAnEmptySigningSecretAsAProgrammingError()
    {
        var error = Assert.ThrowsAny<ArgumentException>(
            () => WebhookUtility.ConstructEvent(EventBody, EventHeaders(), "", Path, receivedAt: AtEventTime));

        Assert.Contains("Signing secret", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RefusesANonPositiveToleranceAsAProgrammingError(int seconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => WebhookUtility.ConstructEvent(EventBody, EventHeaders(), Secret, Path, TimeSpan.FromSeconds(seconds)));
    }

    [Theory]
    [InlineData("hooks/tmh")]
    [InlineData("")]
    [InlineData("ftp://partner.example.com/hooks/tmh")]
    public void RefusesAPathThatIsNeitherARequestPathNorAUrlAsAProgrammingError(string path)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => WebhookUtility.ConstructEvent(EventBody, EventHeaders(), Secret, path, receivedAt: AtEventTime));
    }

    [Fact]
    public void ReturnsATestDeliveryAsItsOwnType()
    {
        var headers = new Dictionary<string, string>
        {
            ["x-tmh-key-id"] = KeyId,
            ["x-tmh-timestamp"] = TestDeliveryTimestamp,
            ["x-tmh-signature"] = TestDeliverySignature,
            ["x-tmh-body-hash"] = TestDeliveryBodyHash,
            ["x-tmh-event-type"] = "learner.certified",
            ["x-tmh-event-id"] = "3f1c9a52-7d4e-4b8a-9c21-5e6f7a8b9c0d",
            ["content-type"] = "application/json",
        };

        var delivery = WebhookUtility.ConstructEvent(
            TestDeliveryBody, headers, Secret, Path, receivedAt: At("2026-11-15T16:40:06.000Z"));

        Assert.True(delivery.IsTestDelivery);
        Assert.Null(delivery.Event);
        Assert.Equal("whs_2b8f4c1d", delivery.TestDelivery.SubscriptionId);
        Assert.True(delivery.TestDelivery.Test);
    }

    [Fact]
    public void ReturnsAnEventOfATypeNotInTheCatalogAsAnUnknownEvent()
    {
        const string body =
            """{"data":{"badge":"gold","learnerId":"lrn_4h7j2k"},"eventId":"evt_01JCZ9000000000000000000AB","eventType":"learner.promoted","occurredAt":"2026-11-15T16:31:30.000Z","orgId":"org_9f3k2m"}""";
        var headers = WebhookUtility.GenerateTestHeaders(body, Secret, Path, timestamp: At(EventTimestamp));

        var delivery = WebhookUtility.ConstructEvent(body, headers, Secret, Path, receivedAt: AtEventTime);

        var unknown = Assert.IsType<UnknownEvent>(delivery.Event);
        Assert.Equal("learner.promoted", unknown.EventType);
        Assert.Equal("evt_01JCZ9000000000000000000AB", unknown.EventId);
        Assert.Equal("gold", unknown.Data["badge"].GetString());
    }

    [Fact]
    public void GeneratesTheHeadersThePlatformSendsForTheSameDelivery()
    {
        var headers = WebhookUtility.GenerateTestHeaders(
            EventBody,
            Secret,
            Path,
            timestamp: At(EventTimestamp),
            eventType: "learner.certified",
            eventId: "evt_01JCZ8Y2K5V3M7Q9R4T6W8X0AB",
            keyId: KeyId);

        Assert.Equal(EventHeaders(), headers);
    }

    [Fact]
    public void GeneratesHeadersThatConstructEventAcceptsNow()
    {
        var headers = WebhookUtility.GenerateTestHeaders(TestDeliveryBody, Secret, Path);

        var delivery = WebhookUtility.ConstructEvent(TestDeliveryBody, headers, Secret, Path);

        Assert.True(delivery.IsTestDelivery);
        Assert.Equal("whs_2b8f4c1d", delivery.TestDelivery.SubscriptionId);
    }

    [Fact]
    public void LeavesOutTheIdentifyingHeadersItWasNotGiven()
    {
        var headers = WebhookUtility.GenerateTestHeaders(
            TestDeliveryBody, Secret, Path, timestamp: new DateTimeOffset(2026, 11, 15, 11, 40, 5, TimeSpan.FromHours(-5)));

        Assert.Equal(
            new Dictionary<string, string>
            {
                ["x-tmh-timestamp"] = TestDeliveryTimestamp,
                ["x-tmh-signature"] = TestDeliverySignature,
                ["x-tmh-body-hash"] = TestDeliveryBodyHash,
                ["content-type"] = "application/json",
            },
            headers);
    }

    [Fact]
    public void GeneratesHeadersLookedUpWithoutRegardToCase()
    {
        var headers = WebhookUtility.GenerateTestHeaders(TestDeliveryBody, Secret, Path, timestamp: At(TestDeliveryTimestamp));

        Assert.Equal(TestDeliverySignature, headers["X-TMH-Signature"]);
    }

    [Fact]
    public void AcceptsTheRawBodyAsBytesAndHeadersInAnyCase()
    {
        var headers = new Dictionary<string, string>
        {
            ["X-TMH-Timestamp"] = EventTimestamp,
            ["X-Tmh-Signature"] = EventSignature,
        };

        var delivery = WebhookUtility.ConstructEvent(
            Encoding.UTF8.GetBytes(EventBody), headers, Secret, Path, receivedAt: AtEventTime);

        Assert.IsType<EventLearnerCertified>(delivery.Event);
    }

    [Fact]
    public void AcceptsMultiValuedHeadersSuchAsHttpHeaders()
    {
        using var request = new HttpRequestMessage();
        request.Headers.TryAddWithoutValidation("X-TMH-Timestamp", EventTimestamp);
        request.Headers.TryAddWithoutValidation("X-TMH-Signature", EventSignature);
        request.Headers.TryAddWithoutValidation("X-TMH-Body-Hash", EventBodyHash);

        var delivery = WebhookUtility.ConstructEvent(EventBody, request.Headers, Secret, Path, receivedAt: AtEventTime);

        Assert.IsType<EventLearnerCertified>(delivery.Event);
    }

    [Fact]
    public void AcceptsAHeaderLookupSuchAsAspNetCoresRequestHeaders()
    {
        var requestHeaders = new Dictionary<string, string>(EventHeaders(), StringComparer.OrdinalIgnoreCase);

        var delivery = WebhookUtility.ConstructEvent(
            Encoding.UTF8.GetBytes(EventBody),
            name => requestHeaders.TryGetValue(name, out var value) ? value : null,
            Secret,
            Path,
            receivedAt: AtEventTime);

        Assert.IsType<EventLearnerCertified>(delivery.Event);
    }

    [Fact]
    public void AcceptsTheFullRequestUrlInPlaceOfThePath()
    {
        var delivery = WebhookUtility.ConstructEvent(
            EventBody,
            EventHeaders(),
            Secret,
            "https://partner.example.com/hooks/tmh?source=teachmehipaa",
            receivedAt: AtEventTime);

        Assert.IsType<EventLearnerCertified>(delivery.Event);
    }

    [Fact]
    public void GeneratesHeadersForARawBodyGivenAsBytes()
    {
        var headers = WebhookUtility.GenerateTestHeaders(
            Encoding.UTF8.GetBytes(EventBody), Secret, Path, timestamp: At(EventTimestamp));

        Assert.Equal(EventSignature, headers["x-tmh-signature"]);
        Assert.Equal(EventBodyHash, headers["x-tmh-body-hash"]);
    }

    [Fact]
    public void IsAlsoReachableFromAClientWithoutMakingARequest()
    {
        var handler = new FakeHttpMessageHandler();
        var client = Fixtures.NewClient(handler);

        var headers = client.Webhooks.GenerateTestHeaders(TestDeliveryBody, Secret, Path);
        var delivery = client.Webhooks.ConstructEvent(TestDeliveryBody, headers, Secret, Path);
        var fromBytes = client.Webhooks.ConstructEvent(
            Encoding.UTF8.GetBytes(EventBody),
            name => EventHeaders().TryGetValue(name, out var value) ? value : null,
            Secret,
            Path,
            receivedAt: AtEventTime);

        Assert.True(delivery.IsTestDelivery);
        Assert.IsType<EventLearnerCertified>(fromBytes.Event);
        Assert.Empty(handler.Requests);
    }

    // The verifier hardcodes ADR-0005's recipe. If the vendored IR ever
    // describes a different one, this fails and the verifier must change with it.
    [Fact]
    public void VerifiesTheSigningRecipeTheVendoredIrDescribes()
    {
        var ir = JsonNode.Parse(File.ReadAllText(System.IO.Path.Combine(RepoRoot.Find(), "openapi", "sdk-ir.json")))!;
        var expected = JsonNode.Parse(
            """
            {
              "algorithm": "hmacSha256Hex",
              "canonical": {
                "parts": [
                  { "kind": "literal", "value": "POST" },
                  { "kind": "pathAndQuery" },
                  { "kind": "header", "name": "x-tmh-timestamp" },
                  { "kind": "bodySha256Hex" }
                ],
                "separator": "\n"
              },
              "headers": {
                "bodyHash": "x-tmh-body-hash",
                "eventId": "x-tmh-event-id",
                "eventType": "x-tmh-event-type",
                "keyId": "x-tmh-key-id",
                "signature": "x-tmh-signature",
                "timestamp": "x-tmh-timestamp"
              },
              "key": "secretUtf8",
              "timestampFormat": "iso8601",
              "toleranceSeconds": 300
            }
            """);

        Assert.True(
            JsonNode.DeepEquals(expected, ir["events"]!["signing"]),
            $"The IR's signing recipe changed: {ir["events"]!["signing"]!.ToJsonString()}");
    }
}
