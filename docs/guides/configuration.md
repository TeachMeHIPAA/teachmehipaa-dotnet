# Configuration

A client's options are the `init`-only properties of
`TeachMeHipaaClientOptions`, and the client copies them when you create it.
There is no global configuration: two clients with different settings can
live side by side.

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

## Options

| Option               | Default                                    | Description                                                                             |
| -------------------- | ------------------------------------------ | --------------------------------------------------------------------------------------- |
| `ClientId`           | None: public methods only                  | Your API client's id (`cli_…`)                                                          |
| `ClientSecret`       | Required with `ClientId`                   | The matching secret (`tmh_cs_…`)                                                        |
| `Scopes`             | Every Scope the API client holds           | Narrows the Access token's Scopes                                                       |
| `TokenProvider`      | Client credentials                         | Supplies Access tokens instead, such as a `StaticTokenProvider`                         |
| `Environment`        | `TeachMeHipaaEnvironment.Live`             | `Live` or `Sandbox`; credentials work only in their own Environment                     |
| `BaseUrl`            | The Environment's URL                      | Overrides it, e.g. `http://localhost:4010/v1` for a mock server                         |
| `OrganizationId`     | Looked up once with `Auth.IntrospectAsync` | The Organization to act for                                                             |
| `MaxRetries`         | `2`                                        | Retries of a failed call that is safe to replay                                         |
| `Timeout`            | 80 seconds                                 | How long to wait for each response, per attempt                                         |
| `AppInfo`            | None                                       | Your software's name, version and URL, sent in the `User-Agent`                         |
| `Logger`             | None: nothing is logged                    | An `ILogger` for one debug entry per attempt (see [Logging](#logging))                  |
| `HttpClient`         | A connection pool shared by every client   | Sends requests, e.g. one from `IHttpClientFactory`; never disposed by the client        |
| `HttpMessageHandler` | A connection pool shared by every client   | The innermost handler, e.g. with a proxy, a client certificate or a test fake; not both |
| `TimeProvider`       | `TimeProvider.System`                      | The clock for token expiry, backoff and timeouts, e.g. `FakeTimeProvider` in tests      |

Set either `ClientId` and `ClientSecret`, or `TokenProvider`, not both;
`Scopes` then belong to the token provider. With neither, the client makes
only public calls (see [Authentication](authentication.md)). The base URLs
are `https://api.tme.so/v1` for Live and
`https://api-teachmehipaa-dev.web.app/v1` for the Sandbox. An invalid
option throws `ArgumentException` naming it when you create the client.

In ASP.NET Core, `AddTeachMeHipaa` builds the options from your services
and configuration (see
[Getting started](getting-started.md#5-use-the-client-from-aspnet-core)).

## Timeouts

`Timeout` is how long to wait for each response, per attempt, 80 seconds by
default. `RequestOptions` overrides `Timeout` and `MaxRetries` for one
call, and adds a `RequestId`:

<!-- sample: timeouts -->
```csharp
var page = await client.Learners.ListAsync(
    new() { Limit = 50 },
    new RequestOptions { Timeout = TimeSpan.FromSeconds(5), MaxRetries = 0 });
Console.WriteLine(page.Items.Count);
```

A call that gets no response in time throws `ApiTimeoutException`, after
any retries. Every method also takes a `CancellationToken`, which cancels
the call, its retries and any `WaitAsync` with an
`OperationCanceledException`. An Operation's `WaitAsync` has its own
`timeout` and `interval` for how long and how often to poll; see
[Operations and files](operations-and-files.md).

## Your own HttpClient or handler

By default every client sends through one pooled connection handler
shared by the process, so creating clients doesn't exhaust sockets, and
DNS changes are picked up every 5 minutes on .NET 8 and later. To
send through a proxy, a client certificate or a test fake, set
`HttpMessageHandler`; to send through an `HttpClient` you manage, such as
one from `IHttpClientFactory`, set `HttpClient`. Set one or the other, not
both. The client never disposes either.

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

The client puts its own handlers for authentication, retries, timeouts and
logging in front of yours, so leave retry and resilience handlers such as
Polly's off.

## Identifying your software

`AppInfo` adds your software's name, and optionally its version and URL,
to the `User-Agent` the client sends, after the library's own
`teachmehipaa-dotnet/<version>`. It helps TeachMeHIPAA support find your
calls. The library sends no other telemetry.

## Logging

The library writes nothing to the console or to disk and sends no
telemetry. Give it an `ILogger` and it logs one `Debug` entry per HTTP
response, token requests included, with the method, the path (without its
query string, which can hold Learner emails), the status, the latency and
the request id. A request that gets no response is logged too, before its
exception is thrown, with `ErrorKind` `connection` or `timeout` in place of
the status and the request id you sent, if any. Request and response
bodies, headers and credentials are never logged, because they hold
Learner details.

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

Each entry has the event id 1 (`TeachMeHipaaRequest`) and carries
`Method`, `Path`, `StatusCode`, `LatencyMs` and `RequestId` as structured
fields. With `AddTeachMeHipaa`, take the logger from the container, as in
[Getting started](getting-started.md#5-use-the-client-from-aspnet-core).
