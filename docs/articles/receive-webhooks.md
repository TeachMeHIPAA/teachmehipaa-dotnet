---
title: Receive webhooks in ASP.NET Core
description: Create a Webhook subscription, verify signed Deliveries in a minimal API or a controller, and test the endpoint with Test deliveries and redrives.
ms.topic: how-to
ms.date: 11/16/2026
---

# Receive webhooks in ASP.NET Core

A Webhook subscription posts Events to your HTTPS endpoint as they
happen. Each Delivery is one signed POST carrying one Event. Your endpoint
verifies the signature, handles the Event and answers with a 2xx status;
TeachMeHIPAA retries a failed Delivery until it succeeds or is
dead-lettered.

The library has no ASP.NET Core middleware or endpoint helpers.
`WebhookUtility` verifies a Delivery from its raw bytes, headers and path,
which works with any framework; the examples here wire it into ASP.NET
Core by hand.

## Prerequisites

- An Access token with the `webhooks:manage` Scope, to manage
  subscriptions.
- An ASP.NET Core app reachable over HTTPS. During development, a tunnel
  to `localhost` works.

## Create a Webhook subscription

Subscribe your endpoint to the Event types you handle:

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

> [!IMPORTANT]
> The response holds the subscription's Signing secret in `Secret`, and no
> later call returns it. Store it in a secret manager right away
> (`signingSecrets` stands in for yours). Your endpoint needs it to verify
> every Delivery.

`WebhookSubscriptions` also has `ListAsync`, `GetAsync`, `UpdateAsync` and
`DeleteAsync`. `UpdateAsync` requires an ETag; see
[Prevent lost updates with ETags](optimistic-concurrency.md).

## Verify a Delivery

`WebhookUtility.ConstructEvent` checks the signature and returns a
`WebhookDelivery`. It holds the `Event`, or, when `IsTestDelivery` is
`true`, a `TestDelivery`. It needs these inputs exactly as they arrived:

| Input          | Why it matters                                                                                                             |
| -------------- | -------------------------------------------------------------------------------------------------------------------------- |
| The raw body   | The signature covers the bytes. JSON that was parsed and re-serialized never verifies, so read the body before model binding. |
| The headers    | A dictionary, `HttpHeaders`, or a lookup function such as `name => request.Headers[name]`.                                 |
| The path       | The signature covers the path the Delivery was sent to, with its query string. Dropping `PathBase` or the query fails every Delivery. |
| Signing secret | The `Secret` from the create call.                                                                                         |

A Delivery signed more than 5 minutes away from your clock, in either
direction, fails. Every failure throws `WebhookSignatureException`, whose
`Code` names the check: `missing_header`, `invalid_timestamp`,
`timestamp_outside_tolerance`, `body_hash_mismatch` or
`signature_mismatch`. Each code is a constant on
`WebhookSignatureException`. Answer these with 400.

Verification needs the Signing secret and no API credentials, so an
endpoint can verify without a `TeachMeHipaaClient`. Code that already
holds a client can call the same methods through `client.Webhooks`.

### Verify in a minimal API

Read `request.Body` yourself in place of binding a parameter to it, and
pass `PathBase`, `Path` and `QueryString` together:

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

### Verify in a controller

In an MVC or Web API controller, give the action no `[FromBody]`
parameter so that nothing consumes the body first:

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

### Handle Events safely

- The same Event can arrive more than once. Key your processing on
  `EventId` so a repeat does no harm.
- Event types this version of the library doesn't know arrive as
  `UnknownEvent`. A `switch` with a default arm, as in the minimal API
  example, keeps answering 2xx after the API adds types.

### Run behind a proxy

When a reverse proxy or load balancer rewrites the path, pass the path the
Delivery was sent to, as registered in `EndpointUrl`, instead of the one
ASP.NET Core reports. `UsePathBase` and the forwarded headers middleware
both change `PathBase` and `Path`, so compare what your app sees with the
registered URL.

## Send a Test delivery

`WebhookSubscriptions.TestAsync` sends a signed Test delivery to the
subscription's endpoint, which checks your verifier end to end. A Test
delivery carries no Event: `ConstructEvent` returns a `WebhookDelivery`
with `IsTestDelivery` set and a `TestDelivery` that names the
`SubscriptionId`. Answer it with 2xx, as both handlers above do.

`TestAsync` returns an Operation that finishes once your endpoint has
answered. Wait for it, then read the outcome with `GetDeliveryResult`:

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

## Redeliver failed Events

`WebhookSubscriptions.RedriveAsync` sends a subscription's failed Events
again and returns the same kind of Operation. Its result is one of these
types:

| Result                         | Meaning                                                        |
| ------------------------------ | -------------------------------------------------------------- |
| `WebhookDeliverySent`          | Your endpoint accepted the Delivery; `ResponseStatus` holds its status |
| `WebhookDeliveryFailed`        | Delivery stopped for good; `Reason` says why, such as `endpoint_gone` |
| `WebhookDeliverySettled`       | The Delivery had already ended; `Status` says how              |
| `UnknownWebhookDeliveryResult` | A result kind this version of the library doesn't know         |

## Next steps

- [Unit test code that uses the client](unit-testing.md#test-a-webhook-endpoint):
  sign Deliveries yourself and post them to your endpoint in memory.
- [Read and follow the Event log](event-log.md): catch up on Deliveries
  your endpoint missed.
- [Example webhook endpoint](https://github.com/TeachMeHIPAA/teachmehipaa-dotnet/tree/main/examples/WebhookEndpoint)
