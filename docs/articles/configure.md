---
title: Configure the TeachMeHIPAA client
description: Every TeachMeHipaaClientOptions property and its default, per-call timeouts, your own HttpClient or handler, User-Agent app info and ILogger request logging.
ms.topic: how-to
ms.date: 11/16/2026
---

# Configure the TeachMeHIPAA client

You configure a client through the `init`-only properties of
`TeachMeHipaaClientOptions`. The client copies them in its constructor and
ignores later changes. Nothing is global, so two clients with different
settings can run side by side.

<!-- sample: configuration -->
```csharp
using TeachMeHipaa;

var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    ClientId = Environment.GetEnvironmentVariable("TMH_CLIENT_ID")!,
    ClientSecret = Environment.GetEnvironmentVariable("TMH_CLIENT_SECRET")!,
    Environment = TeachMeHipaaEnvironment.Sandbox,
    Scopes = ["learners:read", "learners:write"],
    OrganizationId = "org_acme",
    MaxRetries = 3,
    Timeout = TimeSpan.FromSeconds(30),
    AppInfo = new AppInfo("acme-hr-sync", "4.2.0", "https://acme.example"),
});

var compliance = await client.Compliance.GetAsync();
Console.WriteLine(compliance.CompliantRate);
```

## Client options

| Property             | Default                                    | Purpose                                                                                  |
| -------------------- | ------------------------------------------ | ---------------------------------------------------------------------------------------- |
| `ClientId`           | None, so public methods only               | The API client's id (`cli_…`)                                                            |
| `ClientSecret`       | Required with `ClientId`                   | The matching secret (`tmh_cs_…`)                                                         |
| `Scopes`             | Every Scope the API client holds           | Narrows the Access token                                                                 |
| `TokenProvider`      | Client credentials                         | Supplies Access tokens instead, for example a `StaticTokenProvider`                      |
| `Environment`        | `TeachMeHipaaEnvironment.Live`             | `Live` or `Sandbox`                                                                      |
| `BaseUrl`            | The Environment's URL                      | Points the client elsewhere, such as `http://localhost:4010/v1` for a mock server        |
| `OrganizationId`     | Looked up once with `Auth.IntrospectAsync` | The Organization to act for                                                              |
| `MaxRetries`         | `2`                                        | Retries of a failed call that is safe to replay                                          |
| `Timeout`            | 80 seconds                                 | How long each attempt waits for a response                                               |
| `AppInfo`            | None                                       | Your app's name, version and URL, added to the `User-Agent`                              |
| `Logger`             | None, so nothing is logged                 | An `ILogger` that receives one `Debug` entry per HTTP response                           |
| `HttpClient`         | A connection pool shared by all clients    | An `HttpClient` you manage; the client never disposes it                                 |
| `HttpMessageHandler` | A connection pool shared by all clients    | The innermost handler, for a proxy, a client certificate or a test fake; never disposed  |
| `TimeProvider`       | `TimeProvider.System`                      | The clock for token expiry, backoff and timeouts, such as `FakeTimeProvider` in tests     |

The constructor validates every option and throws `ArgumentException`
that names the first bad one: a negative `MaxRetries`, a zero `Timeout`,
a relative `BaseUrl`, half a credential pair, or both `HttpClient` and
`HttpMessageHandler`.

In ASP.NET Core, build these options inside `AddTeachMeHipaa`; see
[Use dependency injection](dependency-injection.md).

## Set a timeout for one call

`Timeout` bounds each attempt, 80 seconds by default. Pass a
`RequestOptions` to override `Timeout` or `MaxRetries` for a single call:

<!-- sample: timeouts -->
```csharp
var page = await client.Learners.ListAsync(
    new() { Limit = 50 },
    new RequestOptions { Timeout = TimeSpan.FromSeconds(5), MaxRetries = 0 });
Console.WriteLine(page.Items.Count);
```

A call that runs out of time on its last attempt throws
`ApiTimeoutException`. The `CancellationToken` parameter on every method
stops the call, its retries and any `WaitAsync` with an
`OperationCanceledException`. Operations have their own polling timeout;
see [Wait for long-running operations](long-running-operations.md).

## Send requests through your own handler

By default, every client in the process shares one pooled
`SocketsHttpHandler` whose connections live 5 minutes, so the client
picks up DNS changes and creating many clients doesn't exhaust sockets.
Set `HttpMessageHandler` for a proxy, a client certificate or a test fake,
or `HttpClient` to reuse one you already manage:

<!-- sample: http-handler -->
```csharp
using System.Net;
using TeachMeHipaa;

var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    ClientId = Environment.GetEnvironmentVariable("TMH_CLIENT_ID")!,
    ClientSecret = Environment.GetEnvironmentVariable("TMH_CLIENT_SECRET")!,
    HttpMessageHandler = new SocketsHttpHandler
    {
        Proxy = new WebProxy("http://proxy.internal:3128"),
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    },
});
```

The client stacks its own `DelegatingHandler`s for authentication,
retries, timeouts and logging in front of yours. Keep retry and
resilience handlers, such as Polly policies, out of the handler you pass.

## Identify your app in the User-Agent

`AppInfo` appends your app's name, and optionally its version and URL, to
the `User-Agent` after the library's own `teachmehipaa-dotnet/<version>`.
TeachMeHIPAA support uses it to find your traffic. The library sends no
other telemetry.

## Log requests with ILogger

The library writes nothing to the console or to disk on its own. Give it
an `ILogger` and it logs one `Debug` entry per HTTP response, token
requests included:

<!-- sample: logging -->
```csharp
using Microsoft.Extensions.Logging;
using TeachMeHipaa;

using var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Debug));
var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    ClientId = Environment.GetEnvironmentVariable("TMH_CLIENT_ID")!,
    ClientSecret = Environment.GetEnvironmentVariable("TMH_CLIENT_SECRET")!,
    OrganizationId = "org_acme",
    Logger = loggerFactory.CreateLogger<TeachMeHipaaClient>(),
});

await client.Learners.ListAsync();
// dbug: TeachMeHipaa.TeachMeHipaaClient[1]
//       teachmehipaa request GET /v1/organizations/org_acme/learners 200 in 84 ms, request id req_7f3c2a
```

A response entry has event id 1 (`TeachMeHipaaRequest`) and the
structured fields `Method`, `Path`, `StatusCode`, `LatencyMs` and
`RequestId`. The path leaves out the query string, which can hold Learner
emails. A request that gets no response is logged before its exception as
event id 2 (`TeachMeHipaaRequestFailed`), with `ErrorKind` set to
`connection` or `timeout` in place of the status. Bodies, headers
and credentials never reach the log, because they carry Learner details.

## Next steps

- [Handle errors and retries](handle-errors.md)
- [Unit test code that uses the client](unit-testing.md)
