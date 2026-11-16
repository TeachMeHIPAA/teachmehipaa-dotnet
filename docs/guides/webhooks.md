# Webhooks

A Webhook subscription sends Events to your HTTPS endpoint as they happen.
Each Delivery is a signed POST of one Event. Your endpoint verifies the
signature, handles the Event and answers 2xx; a Delivery that fails is
retried until it succeeds or is dead-lettered.

Samples assume a `client` created as in
[Getting started](getting-started.md#3-make-your-first-call).

## Create a Webhook subscription

Subscribe an endpoint to the Event types you want. Managing subscriptions
needs the `webhooks:manage` Scope.

<!-- sample: webhook-subscription-create -->
```csharp
var subscription = await client.WebhookSubscriptions.CreateAsync(new WebhookSubscriptionCreateOptions
{
    EndpointUrl = "https://hooks.example.com/webhooks/tmh",
    EventTypes = [EventType.LearnerCertified, EventType.LearnerExpiring],
    Description = "HR sync",
});
await signingSecrets.SaveAsync(subscription.SubscriptionId, subscription.Secret);
Console.WriteLine($"{subscription.SubscriptionId} {subscription.SecretKeyId}");
```

The response carries the subscription's Signing secret in `Secret`, and
only this once: store it now, in a secret manager (`signingSecrets` stands
in for yours). Your endpoint needs it to verify Deliveries.
`WebhookSubscriptions.ListAsync`, `GetAsync`, `UpdateAsync` and
`DeleteAsync` manage subscriptions later; `UpdateAsync` needs an ETag (see
[Conditional requests](conditional-requests.md)).

## Verify a Delivery

`WebhookUtility.ConstructEvent` verifies a Delivery and returns a
`WebhookDelivery` holding its `Event`, or a `TestDelivery` when
`IsTestDelivery`. It needs three things exactly as they arrived:

1. **The raw body.** Parsed and re-serialized JSON never verifies, so read
   the body as bytes before anything binds or parses it.
2. **The headers.** A dictionary, `HttpHeaders`, or a function that looks
   one up, such as `name => request.Headers[name]`.
3. **The request path, with its query string, or the full URL.** The
   signature covers the path the Delivery was sent to, so a verifier that
   drops the path, its query string or the app's path base rejects every
   Delivery.

It also takes the Signing secret. Deliveries signed more than 5 minutes
from your clock, in either direction, are rejected. Verification throws
`WebhookSignatureException` with a `Code` saying which check failed:
`missing_header`, `invalid_timestamp`, `timestamp_outside_tolerance`,
`body_hash_mismatch` or `signature_mismatch` (constants on
`WebhookSignatureException`). Answer it with 400.

Event types this version doesn't know arrive as an `UnknownEvent`, so new
types never break your handler. The same Event can arrive more than once,
so make your handling idempotent, keyed by its `EventId`.

`WebhookUtility` needs only the Signing secret, never API credentials, so a
handler can use it without creating a client. It is also on a client as
`client.Webhooks`.

### ASP.NET Core minimal API

Read the body from `request.Body` yourself rather than binding a parameter
to it, and pass the path base, path and query string:

<!-- sample: delivery-aspnetcore -->
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TeachMeHipaa;

var signingSecret = Environment.GetEnvironmentVariable("TMH_WEBHOOK_SECRET")!;

app.MapPost("/webhooks/tmh", async (HttpRequest request) =>
{
    using var body = new MemoryStream();
    await request.Body.CopyToAsync(body);
    WebhookDelivery delivery;
    try
    {
        delivery = WebhookUtility.ConstructEvent(
            body.ToArray(),
            name => request.Headers.TryGetValue(name, out var value) ? value.ToString() : null,
            signingSecret,
            $"{request.PathBase}{request.Path}{request.QueryString}");
    }
    catch (WebhookSignatureException error)
    {
        return Results.BadRequest(error.Code);
    }

    return delivery.Event switch
    {
        null => Results.Ok("test received"),
        EventLearnerCertified certified => Results.Ok($"certified {certified.Data.LearnerId}"),
        var other => Results.Ok($"received {other.EventId}"),
    };
});
```

### ASP.NET Core controller

In an MVC or Web API controller, leave the action without a `[FromBody]`
parameter, so nothing reads the body before you do:

<!-- sample: delivery-controller -->
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TeachMeHipaa;

[ApiController]
[Route("webhooks/tmh")]
public sealed class TmhWebhookController(IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        using var body = new MemoryStream();
        await Request.Body.CopyToAsync(body, HttpContext.RequestAborted);
        WebhookDelivery delivery;
        try
        {
            delivery = WebhookUtility.ConstructEvent(
                body.ToArray(),
                name => Request.Headers[name],
                configuration["TeachMeHipaa:WebhookSecret"]!,
                $"{Request.PathBase}{Request.Path}{Request.QueryString}");
        }
        catch (WebhookSignatureException error)
        {
            return BadRequest(error.Code);
        }

        if (delivery.IsTestDelivery)
        {
            return Ok("test received");
        }

        return Ok($"received {delivery.Event.EventId}");
    }
}
```

### Behind a proxy

If a proxy or load balancer rewrites the path before your app sees it,
pass the path the Delivery was sent to, as registered in `EndpointUrl`,
rather than the one ASP.NET Core reports. `UsePathBase` and the forwarded
headers middleware change `PathBase` and `Path`, so check what your app
sees against the registered URL.

## Test deliveries

`WebhookSubscriptions.TestAsync` sends a signed Test delivery to the
subscription's endpoint, to check your verifier end to end. It carries no
Event: `ConstructEvent` returns a `WebhookDelivery` whose
`IsTestDelivery` is true and whose `TestDelivery` has the
`SubscriptionId`. Answer it with 2xx, as the handlers above do.

The call returns an Operation that finishes once your endpoint has
answered; wait for it and read the outcome with `GetDeliveryResult`:

<!-- sample: webhook-test-delivery -->
```csharp
var operation = await client.WebhookSubscriptions.TestAsync("whs_01JD8K2M4N6P8R0T2V4X6Z8B0D");
var done = await operation.WaitAsync(timeout: TimeSpan.FromMinutes(1));
switch (done.GetDeliveryResult())
{
    case WebhookDeliverySent sent:
        Console.WriteLine($"endpoint answered {sent.ResponseStatus}");
        break;
    case WebhookDeliveryFailed failed:
        Console.WriteLine($"delivery failed: {failed.Reason}");
        break;
}
```

`WebhookSubscriptions.RedriveAsync` delivers a subscription's failed
Events again, and returns the same kind of Operation. A result is a
`WebhookDeliverySent`, a `WebhookDeliveryFailed`, a
`WebhookDeliverySettled` (the Delivery had already ended) or an
`UnknownWebhookDeliveryResult`; handle results you don't recognize, since
more kinds may be added.

## Sign test Deliveries yourself

To test your handler without TeachMeHIPAA, sign a payload the way
TeachMeHIPAA does with `WebhookUtility.GenerateTestHeaders`. It takes the
body, the Signing secret and the path, and returns the headers to send with
the body:

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
`timestamp`. [Testing](testing.md#test-your-webhook-handler) shows this
inside a test of an ASP.NET Core endpoint.
