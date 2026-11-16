# Testing

Test your integration in three layers: unit tests against a fake
transport that never leave your process, webhook handler tests with
Deliveries you sign yourself, and end-to-end runs against the Sandbox.

## Unit tests with a fake transport

The client sends every request through an `HttpMessageHandler`, so a test
can set `HttpMessageHandler` to one that answers from your own code. Give
the client a `StaticTokenProvider` and an `OrganizationId` so it makes no
token or Organization lookup requests, and `MaxRetries = 0` so a failure
shows up at once:

<!-- sample: fake-transport -->
```csharp
using System.Net;
using System.Text;
using TeachMeHipaa;

var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    TokenProvider = new StaticTokenProvider("test-token"),
    OrganizationId = "org_test",
    HttpMessageHandler = new FakeTeachMeHipaa(),
    MaxRetries = 0,
});
Console.WriteLine((await client.Compliance.GetAsync()).CompliantRate);

/// <summary>Answers the client's requests from memory.</summary>
public sealed class FakeTeachMeHipaa : HttpMessageHandler
{
    private const string Compliance = """
        {"asOf":"2026-11-15T12:00:00.000Z","courseId":"hipaa-2026","compliantRate":0.75,
         "totals":{"learners":4,"compliant":3,"expiringSoon":1,"expired":0,"inProgress":1,"notStarted":0,"inviteBounced":0}}
        """;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = request.RequestUri!.AbsolutePath == "/v1/organizations/org_test/compliance"
            ? Json(HttpStatusCode.OK, Compliance, "application/json")
            : Json(HttpStatusCode.NotFound, """{"title":"Not found","status":404,"code":"not_found"}""", "application/problem+json");
        return Task.FromResult(response);
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string json, string mediaType) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, mediaType) };
}
```

Responses use the API's JSON field names (camelCase), as in the
[OpenAPI document](../../openapi/openapi.developer.json); the models turn
them into PascalCase properties. Errors are Problem Details with a `code`,
so the 404 above throws `NotFoundException` with `Code` `not_found`. The
client never disposes a handler or `HttpClient` you give it.

To test token expiry, retries and `WaitAsync` without waiting, set
`TimeProvider` to a `FakeTimeProvider` from
`Microsoft.Extensions.TimeProvider.Testing` and advance it in the test;
see [Configuration](configuration.md).

## Test your webhook handler

`WebhookUtility.GenerateTestHeaders` signs a body the way TeachMeHIPAA
signs a Delivery, so a test can post a Delivery to your endpoint in memory
with ASP.NET Core's `TestServer` (from `Microsoft.AspNetCore.TestHost`, or
`WebApplicationFactory` for your whole app). Map the endpoint in an
extension method so the app and the test share it, use a test Signing
secret, and sign for the path the test posts to:

<!-- sample: webhook-handler-test -->
```csharp
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using TeachMeHipaa;
using Xunit;

public static class TmhWebhookEndpoint
{
    public static RouteHandlerBuilder MapTmhWebhook(this IEndpointRouteBuilder endpoints, string signingSecret) =>
        endpoints.MapPost("/webhooks/tmh", async (HttpRequest request) =>
        {
            using var body = new MemoryStream();
            await request.Body.CopyToAsync(body);
            try
            {
                WebhookUtility.ConstructEvent(
                    body.ToArray(),
                    name => request.Headers[name],
                    signingSecret,
                    $"{request.PathBase}{request.Path}{request.QueryString}");
            }
            catch (WebhookSignatureException error)
            {
                return Results.BadRequest(error.Code);
            }

            return Results.Ok();
        });
}

public sealed class TmhWebhookEndpointTests
{
    private const string SigningSecret = "test-signing-secret";
    private const string Body = """{"subscriptionId":"whs_test","test":true}""";

    [Fact]
    public async Task AcceptsASignedTestDelivery()
    {
        var headers = WebhookUtility.GenerateTestHeaders(Body, SigningSecret, "/webhooks/tmh");

        var status = await PostAsync(Body, headers);

        Assert.Equal(HttpStatusCode.OK, status);
    }

    [Fact]
    public async Task RejectsAChangedBody()
    {
        var headers = WebhookUtility.GenerateTestHeaders(Body, SigningSecret, "/webhooks/tmh");

        var status = await PostAsync(Body.Replace("true", "false"), headers);

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    private static async Task<HttpStatusCode> PostAsync(string body, IReadOnlyDictionary<string, string> headers)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        app.MapTmhWebhook(SigningSecret);
        await app.StartAsync();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/tmh")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        foreach (var (name, value) in headers)
        {
            request.Headers.TryAddWithoutValidation(name, value);
        }

        using var response = await app.GetTestClient().SendAsync(request);
        return response.StatusCode;
    }
}
```

`GenerateTestHeaders` signs with the current time unless you pass a
`timestamp`. [Webhooks](webhooks.md) has a controller version of the
endpoint.

## End-to-end tests in the Sandbox

The Sandbox runs the real API against separate data and separate
credentials, and only emails allowlisted addresses, so you can create
Learners and Enrollments there freely. Point a client at it with
`Environment = TeachMeHipaaEnvironment.Sandbox` and Sandbox credentials. To
check a webhook endpoint you've deployed, run
`WebhookSubscriptions.TestAsync` against it (see
[Test deliveries](webhooks.md#test-deliveries)).

Keep Sandbox tests out of your default test run, since they need
credentials and the network. With xUnit, put them in their own test
project, or tag them with a trait and filter it out:

```sh
dotnet test --filter "Category!=Sandbox"
TMH_CLIENT_ID=cli_... TMH_CLIENT_SECRET=tmh_cs_... dotnet test --filter "Category=Sandbox"
```

where each Sandbox test carries `[Trait("Category", "Sandbox")]`.
