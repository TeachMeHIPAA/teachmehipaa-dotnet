using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.ReadmeTests;

/// <summary>
/// Runs every sample in the README and the guides against the fake
/// transport and checks what it printed and sent. That the samples compile
/// is checked by the build. Checks for the guides' own samples are in
/// GuideSampleChecks.cs.
/// </summary>
public partial class ReadmeTests
{
    private const string Org = $"/v1/organizations/{OrgId}";
    private const string RequestId = "req_7f3c2a";

    /// <summary><c>dev#/components/schemas/ComplianceSummary</c>.</summary>
    private const string ComplianceJson = """
        {"asOf":"2026-11-15T12:00:00.000Z","courseId":"hipaa-2026","totals":{"learners":4,"compliant":3,"expiringSoon":1,"expired":0,"inProgress":1,"notStarted":0,"inviteBounced":0},"compliantRate":0.75}
        """;

    /// <summary><c>dev#/components/schemas/Course</c>.</summary>
    private const string CourseJson = """
        {"courseId":"hipaa-2026","title":"HIPAA Essentials 2026","courseVersion":"2026.1","passingThreshold":0.8}
        """;

    /// <summary>A <c>dev#/components/schemas/Enrollment</c>.</summary>
    private const string EnrollmentJson = $$"""
        {"enrollmentId":"{{LearnerId}}__hipaa-2026","subjectId":"{{LearnerId}}","courseId":"hipaa-2026","orgId":"{{OrgId}}","status":"INVITED","effectiveStatus":"INVITED","archived":false,"cycle":1,"rev":1,"invitedAt":"2026-11-15T09:00:00.000Z","startedAt":null,"completedAt":null,"certifiedAt":null,"expiresAt":null,"certificateId":null}
        """;

    private const string OperationName = $"organizations/{OrgId}/operations/0b7d4a8e-5c1f-4e2a-9d3b";

    private static readonly IReadOnlyDictionary<string, Func<ReadmeHarness, Task>> Checks = new Dictionary<string, Func<ReadmeHarness, Task>>
    {
        // README.md
        ["quickstart"] = Quickstart,

        // docs/guides/getting-started.md
        ["first-call"] = FirstCall,
        ["learner-writes"] = LearnerWrites,
        ["dependency-injection"] = DependencyInjection,
        ["dependency-injection-endpoint"] = DependencyInjection,

        // docs/guides/authentication.md
        ["client-credentials"] = ClientCredentials,
        ["organization"] = Organization,
        ["static-token"] = StaticToken,
        ["public-lookups"] = PublicLookups,

        // docs/guides/errors-and-retries.md
        ["errors"] = Errors,
        ["request-options"] = RequestOptions,

        // docs/guides/pagination-and-events.md
        ["pagination"] = Pagination,
        ["events-list"] = EventsList,
        ["events-tail"] = EventsTail,

        // docs/guides/webhooks.md
        ["webhook-subscription-create"] = WebhookSubscriptionCreate,
        ["delivery-aspnetcore"] = DeliveryAspNetCore,
        ["delivery-controller"] = DeliveryController,
        ["webhook-test-delivery"] = WebhookTestDelivery,
        ["delivery-test-headers"] = DeliveryTestHeaders,

        // docs/guides/operations-and-files.md
        ["operations"] = Operations,
        ["certificates-export"] = CertificatesExport,
        ["csv-import"] = CsvImport,
        ["pdf-download"] = PdfDownload,

        // docs/guides/conditional-requests.md
        ["conditional-write"] = ConditionalWrite,
        ["not-modified"] = NotModified,

        // docs/guides/testing.md
        ["fake-transport"] = FakeTransport,
        ["webhook-handler-test"] = WebhookHandlerTest,

        // docs/guides/configuration.md
        ["configuration"] = Configuration,
        ["timeouts"] = Timeouts,
        ["http-handler"] = HttpHandler,
        ["logging"] = Logging,
    };

    public static TheoryData<string> SampleIds => [.. ReadmeSamples.Ids];

    [Fact]
    public void EverySampleHasACheck()
    {
        Assert.Equal(ReadmeSamples.Ids.Order(), Checks.Keys.Order());
    }

    [Theory]
    [MemberData(nameof(SampleIds))]
    public async Task SampleRuns(string id)
    {
        await using var readme = new ReadmeHarness();

        await Checks[id](readme);
    }

    private static ScriptedReply Json(string json, HttpStatusCode status = HttpStatusCode.OK, IReadOnlyDictionary<string, string>? headers = null) =>
        new() { Status = status, Json = json, Headers = headers ?? new Dictionary<string, string>() };

    private static ScriptedReply Page(string itemsJson, string? nextCursor = null) =>
        Json($$"""{"items":[{{itemsJson}}],"nextCursor":{{(nextCursor is null ? "null" : $"\"{nextCursor}\"")}}}""");

    private static ScriptedReply DoneOperation(string kind, string responseJson) =>
        Json($$"""{"name":"{{OperationName}}","done":true,"metadata":{"kind":"{{kind}}"},"response":{{responseJson}}}""");

    private static string LearnerCertifiedEvent(string eventId) => $$$"""
        {"eventId":"{{{eventId}}}","eventType":"learner.certified","orgId":"{{{OrgId}}}","occurredAt":"2026-11-15T12:00:00.000Z","data":{"learnerId":"{{{LearnerId}}}","email":"ana@example.com","externalId":"hr-123","courseId":"hipaa-2026","status":"certified","occurredAt":"2026-11-15T12:00:00.000Z"}}
        """;

    private static void AssertJson(string expected, string? actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(actual!)), actual);

    private static async Task Quickstart(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script(IntrospectionRoute, IntrospectionReply())
            .Script(LearnerRoute, LearnerReply())
            .Script($"GET {Org}/learners", Page(LearnerJson));

        await ReadmeSamples.RunAsync("quickstart");

        Assert.Equal(["Ana Example in_progress", "ana@example.com"], readme.Printed);
        Assert.Equal("status=in_progress", Assert.Single(readme.Handler.RequestsTo($"GET {Org}/learners")).Uri.Query.TrimStart('?'));
        Assert.All(readme.Handler.Requests, request => Assert.Equal("api-teachmehipaa-dev.web.app", request.Uri.Host));
    }

    private static async Task DependencyInjection(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script(IntrospectionRoute, IntrospectionReply())
            .Script($"GET {Org}/compliance", Json(ComplianceJson));

        await ReadmeSamples.RunAsync("dependency-injection");
        readme.Builder.Services
            .AddHttpClient(TeachMeHipaaServiceCollectionExtensions.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => readme.Handler);
        await ReadmeSamples.RunAsync("dependency-injection-endpoint");
        await readme.App.StartAsync();
        var body = await readme.App.GetTestClient().GetStringAsync(new Uri("/compliance", UriKind.Relative));

        AssertJson("""{"compliantRate":0.75}""", body);
        Assert.Equal(
            [TokenRoute, IntrospectionRoute, $"GET {Org}/compliance"],
            readme.Handler.Requests.Select(request => request.Route));
    }

    private static async Task Organization(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script($"GET {Org}/compliance", Json(ComplianceJson, headers: new Dictionary<string, string> { ["x-request-id"] = RequestId }));

        await ReadmeSamples.RunAsync("organization");

        Assert.Equal([$"0.75 {RequestId}"], readme.Printed);
        Assert.Empty(readme.Handler.RequestsTo(IntrospectionRoute));
    }

    private static async Task PublicLookups(ReadmeHarness readme)
    {
        readme.Handler
            .Script("GET /v1/certificate-verifications/aB3dE9", Json("""
                {"status":"valid","learnerName":"Ana Example","courseTitle":"HIPAA Essentials 2026","issuedAt":"2026-11-03T08:00:00.000Z","expiresAt":"2027-11-03T08:00:00.000Z","issuer":"TeachMeHIPAA"}
                """))
            .Script("GET /v1/courses", Page(CourseJson));

        await ReadmeSamples.RunAsync("public-lookups");

        Assert.Equal(["valid Ana Example", "HIPAA Essentials 2026"], readme.Printed);
        Assert.All(readme.Handler.Requests, request => Assert.Null(request.Header("Authorization")));
    }

    private static async Task LearnerWrites(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script($"POST {Org}/learners", Json(LearnerJson, HttpStatusCode.Created))
            .Script($"PUT {Org}/learners/by-external-id/hr-123", Json(LearnerJson));

        await ReadmeSamples.RunAsync("learner-writes");

        Assert.Equal(["invited", LearnerId], readme.Printed);
        AssertJson(
            """{"email":"ana@example.com","name":"Ana Example","courseId":"hipaa-2026"}""",
            Assert.Single(readme.Handler.RequestsTo($"POST {Org}/learners")).Body);
    }

    private static async Task Pagination(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script($"GET {Org}/learners", Page(LearnerJson, "c2"), Page(LearnerJson))
            .Script($"GET {Org}/enrollments", Page(EnrollmentJson, "e2"), Page(EnrollmentJson));

        await ReadmeSamples.RunAsync("pagination");

        Assert.Equal([LearnerId, LearnerId, "1", "1"], readme.Printed);
        Assert.Equal(
            ["?limit=100", "?limit=100&cursor=c2", "?limit=50", "?limit=50&cursor=e2"],
            readme.Handler.Requests.Skip(1).Select(request => request.Uri.Query));
    }

    private static async Task Errors(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, ProblemReply(403, """{"title":"Forbidden","status":403,"code":"insufficient_scope","detail":"Requires learners:read."}"""));

        await ReadmeSamples.RunAsync("errors");

        Assert.Equal(["Missing a Scope: Requires learners:read."], readme.Printed);
    }

    private static async Task RequestOptions(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script($"POST {Org}/enrollments", Json(EnrollmentJson, HttpStatusCode.Created, new Dictionary<string, string> { ["x-request-id"] = "hr-sync-2026-11-15-ana" }));

        await ReadmeSamples.RunAsync("request-options");

        Assert.Equal([$"{LearnerId}__hipaa-2026 hr-sync-2026-11-15-ana"], readme.Printed);
        var sent = Assert.Single(readme.Handler.RequestsTo($"POST {Org}/enrollments"));
        Assert.Equal("hr-sync-2026-11-15-ana", sent.Header("Idempotency-Key"));
        Assert.Equal("hr-sync-2026-11-15-ana", sent.Header("x-request-id"));
    }

    private static async Task ConditionalWrite(ReadmeHarness readme)
    {
        var etag = new Dictionary<string, string> { ["ETag"] = "\"7\"" };
        var patch = $"PATCH {Org}/learners/{LearnerId}";
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, LearnerReply(etag), LearnerReply(etag))
            .Script(patch, Json(LearnerJson.Replace("\"Ana Example\"", "\"Ana Q. Example\"", StringComparison.Ordinal)))
            .Script(patch, ProblemReply(412, """{"title":"Precondition Failed","status":412,"code":"precondition_failed"}"""));

        await ReadmeSamples.RunAsync("conditional-write");
        await ReadmeSamples.RunAsync("conditional-write");

        Assert.Equal(["Ana Q. Example", "Someone else changed this Learner; read it again"], readme.Printed);
        var sent = readme.Handler.RequestsTo(patch)[0];
        Assert.Equal("\"7\"", sent.Header("If-Match"));
        Assert.Equal("application/merge-patch+json", sent.Header("Content-Type"));
        AssertJson("""{"name":"Ana Q. Example","externalId":null}""", sent.Body);
    }

    private static async Task NotModified(ReadmeHarness readme)
    {
        readme.Handler.Script(
            "GET /v1/courses/hipaa-2026",
            Json(CourseJson, headers: new Dictionary<string, string> { ["ETag"] = "\"c1\"" }),
            new ScriptedReply { Status = HttpStatusCode.NotModified, Headers = new Dictionary<string, string> { ["ETag"] = "\"c1\"" } });

        await ReadmeSamples.RunAsync("not-modified");

        Assert.Equal(["still current", "HIPAA Essentials 2026"], readme.Printed);
        Assert.Equal("\"c1\"", readme.Handler.Requests[1].Header("If-None-Match"));
    }

    private static async Task Operations(ReadmeHarness readme)
    {
        var csv = "email\nana@example.com\nben@example.com\n";
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script($"POST {Org}/learners:export", DoneOperation(
                "learners.export",
                $$"""{"format":"csv","contentType":"text/csv","filename":"learners.csv","rowCount":2,"dataBase64":"{{Convert.ToBase64String(Encoding.UTF8.GetBytes(csv))}}"}"""));

        await ReadmeSamples.RunAsync("operations");

        Assert.Equal([$"learners.csv 2 {csv.Length}"], readme.Printed);
        AssertJson(
            """{"format":"csv","courseId":"hipaa-2026"}""",
            Assert.Single(readme.Handler.RequestsTo($"POST {Org}/learners:export")).Body);
    }

    private static async Task CsvImport(ReadmeHarness readme)
    {
        File.WriteAllText("roster.csv", "email,name\nana@example.com,Ana Example\n");
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script(
                $"POST {Org}/enrollments:import",
                Json("""{"totalRows":3,"validRows":2,"seatsRequired":2,"seatsAvailable":10,"rows":[]}"""),
                DoneOperation("enrollments.import", "null"));

        await ReadmeSamples.RunAsync("csv-import");

        Assert.Equal(["2 of 3 rows are valid"], readme.Printed);
        var imports = readme.Handler.RequestsTo($"POST {Org}/enrollments:import");
        Assert.Equal(2, imports.Count);
        Assert.All(imports, request => Assert.Contains("ana@example.com,Ana Example", request.Body, StringComparison.Ordinal));
    }

    private static async Task PdfDownload(ReadmeHarness readme)
    {
        var pdf = "%PDF-1.7 certificate"u8.ToArray();
        var pdfReply = new ScriptedReply
        {
            Bytes = pdf,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/pdf",
                ["Content-Disposition"] = "attachment; filename=\"certificate-cert_01JD8F.pdf\"",
            },
        };
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script($"GET {Org}/certificates/cert_01JD8F/content", pdfReply, pdfReply);

        await ReadmeSamples.RunAsync("pdf-download");

        Assert.Equal(pdf, File.ReadAllBytes("certificate-cert_01JD8F.pdf"));
        Assert.Equal(pdf, File.ReadAllBytes("certificate-copy.pdf"));
    }

    private static async Task DeliveryAspNetCore(ReadmeHarness readme)
    {
        // Signed for the path with its query string, as TeachMeHIPAA signs it.
        const string path = "/webhooks/tmh?attempt=1";
        var payload = LearnerCertifiedEvent("evt_01JD8M");

        await ReadmeSamples.RunAsync("delivery-aspnetcore");
        await readme.App.StartAsync();
        using var http = readme.App.GetTestClient();
        var signed = await PostAsync(http, path, payload, WebhookUtility.GenerateTestHeaders(payload, ReadmeHarness.WebhookSecret, path));
        var forged = await PostAsync(http, path, payload, WebhookUtility.GenerateTestHeaders(payload, "another-secret", path));

        Assert.Equal((HttpStatusCode.OK, $"\"certified {LearnerId}\""), signed);
        Assert.Equal((HttpStatusCode.BadRequest, "\"signature_mismatch\""), forged);
    }

    private static async Task<(HttpStatusCode, string)> PostAsync(HttpClient http, string path, string payload, IReadOnlyDictionary<string, string> headers)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(path, UriKind.Relative))
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json"),
        };
        foreach (var (name, value) in headers)
        {
            request.Headers.TryAddWithoutValidation(name, value);
        }

        using var response = await http.SendAsync(request);
        return (response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private static async Task DeliveryTestHeaders(ReadmeHarness readme)
    {
        await ReadmeSamples.RunAsync("delivery-test-headers");

        Assert.Equal(["learner.certified"], readme.Printed);
    }

    private static async Task EventsTail(ReadmeHarness readme)
    {
        readme.Cursors.Cursor = "evt_01JD8A";
        readme.Cursors.Expected = 2;
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script($"GET {Org}/events", Page($"{LearnerCertifiedEvent("evt_01JD8B")},{LearnerCertifiedEvent("evt_01JD8C")}"));
        using var follower = new EventLogFollower(readme.Client, readme.Cursors);

        await follower.StartAsync(CancellationToken.None);
        await readme.Cursors.EnoughSaved.WaitAsync(TimeSpan.FromSeconds(10));
        await follower.StopAsync(CancellationToken.None);

        Assert.Equal(["evt_01JD8B", "evt_01JD8C"], readme.Cursors.Saved);
        Assert.Equal(["evt_01JD8B", "evt_01JD8C"], readme.Printed);
        Assert.Equal("?after=evt_01JD8A", readme.Handler.RequestsTo($"GET {Org}/events")[0].Uri.Query);
    }

    private static async Task Configuration(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script($"GET {Org}/compliance", Json(ComplianceJson));

        await ReadmeSamples.RunAsync("configuration");

        Assert.Equal(["0.75"], readme.Printed);
        Assert.Contains("scope=learners%3Aread", readme.Handler.RequestsTo(TokenRoute)[0].Body, StringComparison.Ordinal);
        Assert.EndsWith(
            "acme-hr-sync/4.2.0 (+https://acme.example)",
            readme.Handler.RequestsTo($"GET {Org}/compliance")[0].Header("User-Agent"),
            StringComparison.Ordinal);
    }

    private static async Task Logging(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script($"GET {Org}/learners", Page(LearnerJson));

        await ReadmeSamples.RunAsync("logging");

        Assert.Equal([TokenRoute, $"GET {Org}/learners"], readme.Handler.Requests.Select(request => request.Route));
    }
}
