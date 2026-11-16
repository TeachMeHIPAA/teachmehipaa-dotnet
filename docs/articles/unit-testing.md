---
title: Unit test code that uses the TeachMeHIPAA client
description: Test against a fake HttpMessageHandler, control time with FakeTimeProvider, sign webhook Deliveries yourself and keep Sandbox tests out of the default run.
ms.topic: how-to
ms.date: 11/16/2026
---

# Unit test code that uses the TeachMeHIPAA client

`TeachMeHipaaClient` is a sealed class with no interface, by design: you
test through its transport. Point it at a fake `HttpMessageHandler` and
your tests exercise the real client, from serialization to exception
mapping, without leaving the process.

## Test with a fake transport

Give the client a `StaticTokenProvider` and an `OrganizationId` so it
skips the token and Organization requests, and set `MaxRetries = 0` so a
failure surfaces on the first attempt:

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

Fake responses use the API's camelCase JSON field names, as in the
[OpenAPI document](https://github.com/TeachMeHIPAA/teachmehipaa-dotnet/blob/main/openapi/openapi.developer.json);
the models map them to PascalCase properties. Errors are Problem Details
with a `code`, so the 404 branch above makes the client throw
`NotFoundException` with `Code` set to `not_found`. The client never
disposes a handler or `HttpClient` that you supply.

## Control time in tests

Token expiry, retry backoff and `WaitAsync` polling all read the client's
`TimeProvider`. Set it to a `FakeTimeProvider` from the
`Microsoft.Extensions.TimeProvider.Testing` package and advance the clock
in the test, so those paths run without real delays.

## Sign a Delivery yourself

`WebhookUtility.GenerateTestHeaders` signs a payload the way TeachMeHIPAA
signs a Delivery. Pass it the body, a test Signing secret and the path,
and send the headers it returns with the body:

<!-- sample: delivery-test-headers -->
```csharp
const string payload = """
    {"eventId":"evt_01JD8M","eventType":"learner.certified","orgId":"org_acme",
     "occurredAt":"2026-11-15T12:00:00.000Z",
     "data":{"learnerId":"sub_01JD80AB2C3D4E5F6G7H8J9K0M","email":"ana@example.com","externalId":"hr-123",
             "courseId":"hipaa-2026","status":"certified","occurredAt":"2026-11-15T12:00:00.000Z"}}
    """;
var headers = WebhookUtility.GenerateTestHeaders(payload, "test-signing-secret", "/webhooks/tmh");

var delivery = WebhookUtility.ConstructEvent(payload, headers, "test-signing-secret", "/webhooks/tmh");
if (delivery.Event is EventLearnerCertified certified)
{
    Console.WriteLine(certified.EventType);
}
```

`GenerateTestHeaders` signs with the current time unless you pass a
`timestamp`, which lets a test cover the 5-minute tolerance.

## Test a webhook endpoint

Post signed Deliveries to your endpoint in memory with ASP.NET Core's
`TestServer`, from the `Microsoft.AspNetCore.TestHost` package, or with
`WebApplicationFactory` for the whole app. Map the endpoint in an
extension method so the app and the test share it:

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

## Run end-to-end tests against the Sandbox

The Sandbox runs the real API on separate data with separate credentials,
so tests can create Learners and Enrollments there. Point a client at it
with `Environment = TeachMeHipaaEnvironment.Sandbox` and Sandbox
credentials. To check a deployed webhook endpoint, call
`WebhookSubscriptions.TestAsync`; see
[Send a Test delivery](receive-webhooks.md#send-a-test-delivery).

Sandbox tests need credentials and the network, so keep them out of the
default run. With xUnit, tag each one with
`[Trait("Category", "Sandbox")]` and filter on the trait:

```sh
dotnet test --filter "Category!=Sandbox"
TMH_CLIENT_ID=cli_... TMH_CLIENT_SECRET=tmh_cs_... dotnet test --filter "Category=Sandbox"
```

## Next steps

- [Configure the client](configure.md): `HttpMessageHandler`,
  `HttpClient` and `TimeProvider`.
- [Receive webhooks in ASP.NET Core](receive-webhooks.md)
