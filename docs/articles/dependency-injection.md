---
title: Use the TeachMeHIPAA client with dependency injection
description: Register TeachMeHipaaClient in ASP.NET Core or any .NET generic host, send its requests through IHttpClientFactory and add your own handlers.
ms.topic: how-to
ms.date: 11/16/2026
---

# Use the TeachMeHIPAA client with dependency injection

`AddTeachMeHipaa` registers one shared `TeachMeHipaaClient` in an
`IServiceCollection` and routes its requests through a named
`IHttpClientFactory` client. It works in ASP.NET Core, worker services and
any other host built on `Microsoft.Extensions.DependencyInjection`.

## Prerequisites

- An app built on the .NET generic host or `WebApplication`.
- Credentials in configuration, for example under a `TeachMeHipaa`
  section in user secrets, environment variables or a key vault.

The library already depends on `Microsoft.Extensions.Http`, so
`AddHttpClient` and `IHttpClientFactory` need no extra package.

## Register the client

Call `AddTeachMeHipaa` with a factory that builds the client's options
from the container:

<!-- sample: dependency-injection -->
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeachMeHipaa;

builder.Services.AddTeachMeHipaa(services => new TeachMeHipaaClientOptions
{
    ClientId = builder.Configuration["TeachMeHipaa:ClientId"]!,
    ClientSecret = builder.Configuration["TeachMeHipaa:ClientSecret"]!,
    Environment = TeachMeHipaaEnvironment.Sandbox,
    Logger = services.GetRequiredService<ILogger<TeachMeHipaaClient>>(),
});
```

The extension method lives in the `Microsoft.Extensions.DependencyInjection`
namespace, beside `AddHttpClient`. The options factory runs once, when
the container first resolves the client.

## Inject the client

Take `TeachMeHipaaClient` as a parameter wherever the container supplies
services: a minimal API handler, a controller constructor or a
`BackgroundService`.

<!-- sample: dependency-injection-endpoint -->
```csharp
using Microsoft.AspNetCore.Builder;
using TeachMeHipaa;

app.MapGet("/compliance", async (TeachMeHipaaClient tmh, CancellationToken cancellationToken) =>
{
    ComplianceSummary summary = await tmh.Compliance.GetAsync(cancellationToken: cancellationToken);
    return new { summary.CompliantRate };
});
```

Pass the request's `CancellationToken` through, so a client that hangs up
cancels the API call and its retries.

## Customize the HTTP pipeline

`AddTeachMeHipaa` returns the `IHttpClientBuilder` of the factory client,
which is named `TeachMeHipaaServiceCollectionExtensions.HttpClientName`.
Chain the usual builder methods on it:

- `AddHttpMessageHandler` adds your own `DelegatingHandler`, for example
  one that records metrics.
- `ConfigurePrimaryHttpMessageHandler` swaps the innermost handler, for a
  proxy or a client certificate.

Leave resilience handlers such as `AddStandardResilienceHandler` off this
client. `TeachMeHipaaClient` already retries the calls that are safe to
replay, with idempotency keys, and a second retry layer would replay
writes it must not. The factory client's `HttpClient.Timeout` is
disabled, because the library applies its own `Timeout` to each attempt.

## Understand the client's lifetime

The client is a singleton because it caches the Access token and the
Organization; a scoped client would fetch both again on every request. It
asks the factory for an `HttpClient` on each request, so it always uses
the handler the factory currently hands out and never holds one past its
rotation.

The options your factory returns must leave `HttpClient` and
`HttpMessageHandler` unset, since the factory client sends the requests.
Setting either throws `ArgumentException` when the container resolves the
client.

## Next steps

- [Configure the client](configure.md): every option and its default.
- [Receive webhooks in ASP.NET Core](receive-webhooks.md)
- [Read and follow the Event log](event-log.md): a `BackgroundService`
  that consumes Events.
