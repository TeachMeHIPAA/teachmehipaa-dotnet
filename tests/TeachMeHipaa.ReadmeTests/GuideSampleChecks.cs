using System.Net;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.ReadmeTests;

/// <summary>Checks for the samples that only the guides in docs/guides/ have.</summary>
public partial class ReadmeTests
{
    private const string SubscriptionId = "whs_01JD8K2M4N6P8R0T2V4X6Z8B0D";

    private static async Task FirstCall(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script(IntrospectionRoute, IntrospectionReply())
            .Script($"GET {Org}/compliance", Json(ComplianceJson))
            .Script($"GET {Org}/learners", Page(LearnerJson));

        await ReadmeSamples.RunAsync("first-call");

        Assert.Equal(["Compliant: 0.75", "ana@example.com in_progress"], readme.Printed);
        Assert.Equal("?limit=20", Assert.Single(readme.Handler.RequestsTo($"GET {Org}/learners")).Uri.Query);
        Assert.All(readme.Handler.Requests, request => Assert.Equal("sandbox.api.tme.so", request.Uri.Host));
    }

    private static async Task ClientCredentials(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script(IntrospectionRoute, IntrospectionReply());

        await ReadmeSamples.RunAsync("client-credentials");

        Assert.Equal(["live org_acme learners:read,learners:write"], readme.Printed);
        Assert.Equal([TokenRoute, IntrospectionRoute], readme.Handler.Requests.Select(request => request.Route));
        Assert.Contains("scope=learners%3Aread", readme.Handler.RequestsTo(TokenRoute)[0].Body, StringComparison.Ordinal);
    }

    private static async Task StaticToken(ReadmeHarness readme)
    {
        readme.Handler
            .Script(IntrospectionRoute, IntrospectionReply())
            .Script($"GET {Org}/compliance", Json(ComplianceJson));

        await ReadmeSamples.RunAsync("static-token");

        Assert.Equal(["0.75"], readme.Printed);
        Assert.Empty(readme.Handler.RequestsTo(TokenRoute));
        Assert.All(readme.Handler.Requests, request => Assert.Equal($"Bearer {readme.AccessToken}", request.Header("Authorization")));
    }

    private static async Task EventsList(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script($"GET {Org}/events", Page(LearnerCertifiedEvent("evt_01JD8B")));

        await ReadmeSamples.RunAsync("events-list");

        Assert.Equal(["ana@example.com 2026-11-15"], readme.Printed);
        Assert.Equal("?type=learner.certified", readme.Handler.RequestsTo($"GET {Org}/events")[0].Uri.Query);
    }

    private static async Task WebhookSubscriptionCreate(ReadmeHarness readme)
    {
        // dev#/components/schemas/WebhookSubscriptionCreated
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script($"POST {Org}/webhook-subscriptions", Json(
                $$"""
                {"subscriptionId":"{{SubscriptionId}}","endpointUrl":"https://hooks.example.com/webhooks/tmh","eventTypes":["learner.certified","learner.expiring"],"enabled":true,"description":"HR sync","seatsLowThreshold":5,"secretKeyId":"whk_01JD8K","createdAt":"2026-11-15T12:00:00.000Z","rev":1,"secret":"whsec_c2lnbmluZy1zZWNyZXQ"}
                """,
                HttpStatusCode.Created));

        await ReadmeSamples.RunAsync("webhook-subscription-create");

        Assert.Equal([$"{SubscriptionId} whk_01JD8K"], readme.Printed);
        Assert.Equal("whsec_c2lnbmluZy1zZWNyZXQ", readme.SigningSecrets.Secrets[SubscriptionId]);
        AssertJson(
            """{"endpointUrl":"https://hooks.example.com/webhooks/tmh","eventTypes":["learner.certified","learner.expiring"],"description":"HR sync"}""",
            Assert.Single(readme.Handler.RequestsTo($"POST {Org}/webhook-subscriptions")).Body);
    }

    private static async Task DeliveryController(ReadmeHarness readme)
    {
        // Signed for the path with its query string, as TeachMeHIPAA signs it.
        const string path = "/webhooks/tmh?attempt=1";
        var payload = LearnerCertifiedEvent("evt_01JD8M");
        const string test = """{"subscriptionId":"whs_test","test":true}""";
        readme.Builder.Services.AddControllers().AddApplicationPart(typeof(ReadmeTests).Assembly);
        readme.App.MapControllers();

        await ReadmeSamples.RunAsync("delivery-controller");
        await readme.App.StartAsync();
        using var http = readme.App.GetTestClient();
        var signed = await PostAsync(http, path, payload, WebhookUtility.GenerateTestHeaders(payload, ReadmeHarness.WebhookSecret, path));
        var testDelivery = await PostAsync(http, path, test, WebhookUtility.GenerateTestHeaders(test, ReadmeHarness.WebhookSecret, path));
        var forged = await PostAsync(http, path, payload, WebhookUtility.GenerateTestHeaders(payload, "another-secret", path));
        var wrongPath = await PostAsync(http, path, payload, WebhookUtility.GenerateTestHeaders(payload, ReadmeHarness.WebhookSecret, "/webhooks/tmh"));

        Assert.Equal((HttpStatusCode.OK, "received evt_01JD8M"), signed);
        Assert.Equal((HttpStatusCode.OK, "test received"), testDelivery);
        Assert.Equal((HttpStatusCode.BadRequest, "signature_mismatch"), forged);
        Assert.Equal(HttpStatusCode.BadRequest, wrongPath.Item1);
    }

    private static async Task WebhookTestDelivery(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script($"POST {Org}/webhook-subscriptions/{SubscriptionId}:test", DoneOperation("webhooks.test", """{"_tag":"sent","responseStatus":200}"""));

        await ReadmeSamples.RunAsync("webhook-test-delivery");

        Assert.Equal(["endpoint answered 200"], readme.Printed);
    }

    private static async Task CertificatesExport(ReadmeHarness readme)
    {
        var csv = "certificateId,email\r\ncert_01JD8F,ana@example.com\r\ncert_01JD8G,ben@example.com\r\n";
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script($"POST {Org}/certificates:export", DoneOperation(
                "certificates.export",
                $$"""{"format":"csv","contentType":"text/csv","filename":"certificates.csv","rowCount":2,"dataBase64":"{{Convert.ToBase64String(Encoding.UTF8.GetBytes(csv))}}"}"""));

        await ReadmeSamples.RunAsync("certificates-export");

        Assert.Equal(["2 Certificates exported"], readme.Printed);
        Assert.Equal(csv, File.ReadAllText("certificates.csv"));
        AssertJson("""{"format":"csv"}""", Assert.Single(readme.Handler.RequestsTo($"POST {Org}/certificates:export")).Body);
    }

    private static async Task FakeTransport(ReadmeHarness readme)
    {
        await ReadmeSamples.RunAsync("fake-transport");

        Assert.Equal(["0.75"], readme.Printed);
        Assert.Empty(readme.Handler.Requests);
    }

    private static async Task WebhookHandlerTest(ReadmeHarness readme)
    {
        // The sample is an xUnit test class; run its tests as xUnit would.
        await ReadmeSamples.RunAsync("webhook-handler-test");
        var tests = new TmhWebhookEndpointTests();

        await tests.AcceptsASignedTestDelivery();
        await tests.RejectsAChangedBody();

        Assert.Empty(readme.Handler.Requests);
    }

    private static async Task Timeouts(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script($"GET {Org}/learners", Page(LearnerJson));

        await ReadmeSamples.RunAsync("timeouts");

        Assert.Equal(["1"], readme.Printed);
        Assert.Equal("?limit=50", Assert.Single(readme.Handler.RequestsTo($"GET {Org}/learners")).Uri.Query);
    }

    private static async Task HttpHandler(ReadmeHarness readme)
    {
        // The sample only configures a client; creating one sends nothing.
        await ReadmeSamples.RunAsync("http-handler");

        Assert.Empty(readme.Printed);
        Assert.Empty(readme.Handler.Requests);
    }
}
