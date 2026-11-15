# TeachMeHIPAA .NET Library

The official .NET library for the
[TeachMeHIPAA Developer API](https://api.tme.so/v1). It handles Access tokens,
the Organization binding, retries, idempotency keys, typed exceptions,
pagination, ETags, Operations and webhook verification, so your code only
calls methods. Every method is `async` and takes a `CancellationToken`.

> [!NOTE]
> The Developer API has not launched yet, and this package is not on NuGet.
> Until it is, reference the project from a local checkout.

## Contents

- [Installation](#installation)
- [Quickstart](#quickstart)
- [Dependency injection](#dependency-injection)
- [The Organization binding](#the-organization-binding)
- [Public lookups without credentials](#public-lookups-without-credentials)
- [Creating Learners](#creating-learners)
- [Pagination](#pagination)
- [Exceptions](#exceptions)
- [Retries and idempotency](#retries-and-idempotency)
- [Conditional writes and reads](#conditional-writes-and-reads)
- [Operations](#operations)
- [Uploads and downloads](#uploads-and-downloads)
- [Webhooks](#webhooks)
- [Partner apps](#partner-apps)
- [Tailing the Event log](#tailing-the-event-log)
- [Configuration](#configuration)
- [Logging](#logging)
- [Target frameworks](#target-frameworks)
- [Versioning](#versioning)
- [Support and security](#support-and-security)

## Installation

```sh
dotnet add package TeachMeHipaa
```

The package targets `net8.0` and `netstandard2.0` (see
[Target frameworks](#target-frameworks)). It uses System.Text.Json with
source generation, so it is trim- and AOT-safe on .NET 8 and later.

## Quickstart

Create an API client in the TeachMeHIPAA console to get a client id
(`cli_…`) and secret (`tmh_cs_…`). Start in the Sandbox Environment, where no
real Learners are emailed and no real money moves.

<!-- sample: quickstart -->
```csharp
using TeachMeHipaa;

var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    ClientId = Environment.GetEnvironmentVariable("TMH_CLIENT_ID")!,
    ClientSecret = Environment.GetEnvironmentVariable("TMH_CLIENT_SECRET")!,
    Environment = TeachMeHipaaEnvironment.Sandbox,
});

Learner learner = await client.Learners.GetAsync("sub_01JD80AB2C3D4E5F6G7H8J9K0M");
Console.WriteLine($"{learner.Name} {learner.Status}");

await foreach (var item in client.Learners.ListAutoPagingAsync(new() { Status = LearnerStatus.InProgress }))
{
    Console.WriteLine(item.Email);
}
```

The client exchanges your credentials for an Access token on the first call,
caches it and renews it 60 seconds before it expires. Concurrent calls share
one token request. If the API rejects a token mid-flight (401), the client
renews it once and replays the call; that replay doesn't count as a retry.

Services hang off the client (`client.Learners`, `client.Enrollments`, …).
Methods take ids in the path first, then an options object such as
`LearnerCreateOptions`, then an optional `RequestOptions` and a
`CancellationToken`. Every object they return has a `LastResponse` with the
HTTP `StatusCode`, `Headers`, `RequestId` and `ETag`.

The client is thread-safe. Create one and share it: it holds the token cache
and the Organization, and sends through a pooled connection handler shared by
every client in the process. To ask for fewer Scopes than your API client
holds, set `Scopes = ["learners:read"]`.

## Dependency injection

In ASP.NET Core or any `Microsoft.Extensions.DependencyInjection` host,
`AddTeachMeHipaa` registers one shared `TeachMeHipaaClient` whose requests go
through an `IHttpClientFactory` client:

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

Then take the client wherever the container injects services:

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

The client is a singleton, because it caches the Access token and the
Organization. It asks the factory for an `HttpClient` on every request, so it
always uses the factory's current handler. `AddTeachMeHipaa` returns the
factory client's `IHttpClientBuilder` (named
`TeachMeHipaaServiceCollectionExtensions.HttpClientName`), to add your own
`DelegatingHandler`s or a proxy with `ConfigurePrimaryHttpMessageHandler`.
The options factory runs once, when the client is first resolved, and must
leave `HttpClient` and `HttpMessageHandler` unset. Leave the factory client's
own resilience handlers off: the client already retries what is safe to
retry.

## The Organization binding

An API client belongs to one Organization, so you never pass an `orgId`. On
the first Organization-scoped call, the client asks `Auth.IntrospectAsync`
which Organization the token is for, once, and remembers the answer.
Concurrent first calls share that lookup. If you already know the id, pass it
and the client skips the lookup:

<!-- sample: organization -->
```csharp
using TeachMeHipaa;

var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    ClientId = Environment.GetEnvironmentVariable("TMH_CLIENT_ID")!,
    ClientSecret = Environment.GetEnvironmentVariable("TMH_CLIENT_SECRET")!,
    OrganizationId = "org_acme",
});

var compliance = await client.Compliance.GetAsync();
Console.WriteLine($"{compliance.CompliantRate} {compliance.LastResponse!.RequestId}");
```

A client's configuration is fixed when you create it. To act for another
Organization, create another client. Partner-level calls
(`client.Partners`) are not Organization-scoped and take the `partnerId` as
their first argument.

## Public lookups without credentials

Public methods, `Courses.ListAsync`, `Courses.GetAsync` and
`CertificateVerifications.GetAsync`, send no Access token at all. To call only
those, such as to check a Certificate someone showed you, create a client
without credentials:

<!-- sample: public-lookups -->
```csharp
using TeachMeHipaa;

var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    Environment = TeachMeHipaaEnvironment.Sandbox,
});

var verification = await client.CertificateVerifications.GetAsync("aB3dE9");
Console.WriteLine($"{verification.Status} {verification.LearnerName}");

await foreach (var course in client.Courses.ListAutoPagingAsync())
{
    Console.WriteLine(course.Title);
}
```

Any other method on such a client throws `InvalidOperationException` naming
the endpoint before it sends anything, the Organization lookup included. Set
`ClientId` and `ClientSecret`, or a `TokenProvider`, to call them. Setting
only one of `ClientId` and `ClientSecret` still throws `ArgumentException`
when you create the client.

## Creating Learners

`CreateAsync` and `UpsertByExternalIdAsync` say whether they created the
Learner in `Created`. `UpsertByExternalIdAsync` is safe to replay from a sync
job: it creates and invites the Learner the first time and returns them every
time after.

<!-- sample: learner-writes -->
```csharp
var created = await client.Learners.CreateAsync(new LearnerCreateOptions
{
    Email = "ana@example.com",
    Name = "Ana Example",
    CourseId = "hipaa-2026",
});
Console.WriteLine(created.Created ? "invited" : "already enrolled");

var synced = await client.Learners.UpsertByExternalIdAsync(
    "hr-123",
    new LearnerUpsertByExternalIdOptions { Email = "ana@example.com", Name = "Ana Example" });
Console.WriteLine(synced.LearnerId);
```

Samples from here on assume a `client` created as in the
[Quickstart](#quickstart), with `using TeachMeHipaa;`.

## Pagination

A list method returns one `Page<T>`, with `Items`, `NextCursor` and
`HasNextPage`. A page is also an `IAsyncEnumerable<T>`: `await foreach` over
it yields every item across all pages, fetching the next page only when the
loop reaches it, and stops fetching when you `break`. `ListAutoPagingAsync`
does the same without awaiting the first page yourself, as in Stripe's
library.

<!-- sample: pagination -->
```csharp
// Every Learner, page by page, fetched lazily.
await foreach (var learner in client.Learners.ListAutoPagingAsync(new() { Limit = 100 }))
{
    Console.WriteLine(learner.LearnerId);
}

// One page at a time.
var page = await client.Enrollments.ListAsync(new() { Limit = 50 });
Console.WriteLine(page.Items.Count);
while (page.HasNextPage)
{
    page = await page.GetNextPageAsync();
    Console.WriteLine(page.Items.Count);
}
```

`page.PagesAsync()` yields each page in turn. The next page repeats the
first call's filters, limit and `RequestOptions`.

## Exceptions

Failed calls throw a subclass of `TeachMeHipaaException`, chosen by HTTP
status:

| Exception                     | When                                                         |
| ----------------------------- | ------------------------------------------------------------ |
| `InvalidRequestException`     | 400, 405, 406, 415 and 422                                   |
| `IdempotencyException`        | 422 `idempotency_key_reuse` (an `InvalidRequestException`)   |
| `AuthenticationException`     | 401                                                          |
| `PermissionDeniedException`   | 403, such as a missing Scope                                 |
| `NotFoundException`           | 404                                                          |
| `ConflictException`           | 409                                                          |
| `PreconditionFailedException` | 412 (stale ETag) and 428 (missing `If-Match`)                |
| `RateLimitException`          | 429, with `RetryAfter`                                       |
| `ApiException`                | 5xx, or a response that isn't JSON (such as an HTML page)    |
| `ApiConnectionException`      | No response arrived                                          |
| `ApiTimeoutException`         | No response within the timeout (an `ApiConnectionException`) |
| `OAuthException`              | The token endpoint refused to issue a token                  |

Each carries the problem's `Code` (branch on it; new codes can appear at any
time), `Title`, `Detail`, field-level `Errors`, the `StatusCode`, the
response `Headers` and the `RequestId` to quote to support. Cancelling a call
through its `CancellationToken` throws `OperationCanceledException`, never a
`TeachMeHipaaException`.

<!-- sample: errors -->
```csharp
try
{
    await client.Learners.GetAsync("sub_01JD80AB2C3D4E5F6G7H8J9K0M");
}
catch (NotFoundException)
{
    Console.WriteLine("No such Learner");
}
catch (PermissionDeniedException error)
{
    Console.WriteLine($"Missing a Scope: {error.Detail}");
}
catch (TeachMeHipaaException error)
{
    Console.WriteLine($"{error.Code} {error.Detail} {error.RequestId}");
    foreach (var field in error.Errors)
    {
        Console.WriteLine($"{field.Path} {field.Message}");
    }
}
```

## Retries and idempotency

The client retries network errors, 409 `conflict`, 429 and 5xx up to
`MaxRetries` times (2 by default), backing off from 0.5 s up to 5 s with
jitter and honoring `Retry-After` up to 60 s. It only retries calls that are
safe to replay: reads, writes that carry an `Idempotency-Key`, and the few
writes that are idempotent by nature, such as `SeatOrders.QuoteAsync`. PATCH
and DELETE are never retried, and take no key: the API ignores one on them.

Every create and custom method gets a generated UUID `Idempotency-Key`,
reused by each of its retries, so a retry never enrolls a Learner twice or
charges twice. Pass your own in `RequestOptions` to make a call safe to
repeat across processes. `MaxRetries` and `Timeout` override the client's
settings for one call, and `RequestId` sends your own `x-request-id` on every
attempt, so the call can be found in your logs and TeachMeHIPAA's. The API
echoes an id that matches `^[A-Za-z0-9._-]{1,128}$` and replaces any other:

<!-- sample: request-options -->
```csharp
var enrollment = await client.Enrollments.CreateAsync(
    new EnrollmentCreateOptions { Email = "ana@example.com", CourseId = "hipaa-2026" },
    new RequestOptions
    {
        IdempotencyKey = "hr-sync-2026-11-15-ana",
        MaxRetries = 5,
        Timeout = TimeSpan.FromSeconds(10),
        RequestId = "hr-sync-2026-11-15-ana",
    });
Console.WriteLine($"{enrollment.EnrollmentId} {enrollment.LastResponse!.RequestId}");
```

## Conditional writes and reads

Updates that the API guards with ETags take an `ETag` in their options'
constructor: the ETag string, or the object you read, whose
`LastResponse.ETag` the client sends as `If-Match`. `LearnerUpdateOptions` and
`WebhookSubscriptionUpdateOptions` require it, since the API refuses those
updates without one; on `LearnerOffboardOptions` it is an optional property.
A stale ETag throws `PreconditionFailedException`.

Updates are merge patches. Their fields are `Optional<T>`: a field you don't
set is not sent and stays unchanged, while `null` is sent as JSON `null` and
clears the field.

<!-- sample: conditional-write -->
```csharp
var learner = await client.Learners.GetAsync("sub_01JD80AB2C3D4E5F6G7H8J9K0M");
try
{
    var updated = await client.Learners.UpdateAsync(
        learner.LearnerId,
        new LearnerUpdateOptions(learner) { Name = "Ana Q. Example", ExternalId = null });
    Console.WriteLine(updated.Name);
}
catch (PreconditionFailedException)
{
    Console.WriteLine("Someone else changed this Learner; read it again");
}
```

Retrieves that support ETags have a `GetIfModifiedAsync` twin that sends
`If-None-Match`. When your copy is still current the API answers 304 and the
result's `Value` is `null` (`IsNotModified`), so keep your copy:

<!-- sample: not-modified -->
```csharp
var course = await client.Courses.GetAsync("hipaa-2026");

var latest = await client.Courses.GetIfModifiedAsync("hipaa-2026", course);
if (latest.IsNotModified)
{
    Console.WriteLine("still current");
}

course = latest.Value ?? course;
Console.WriteLine(course.Title);
```

## Operations

Exports, imports, batch writes, Test deliveries and redrives return an
`Operation`. `WaitAsync` polls until it is done (backing off from `interval`,
default 1 s, to 10 s) and throws `OperationTimeoutException` after `timeout`
(default 5 minutes), or `OperationFailedException` if it finished with an
error; both carry the `Operation`. An Operation that is already done, as
exports always are, returns without a request. `GetExportResult` hands back an
export's CSV, and `GetDeliveryResult` a Test delivery's or redrive's outcome.

<!-- sample: operations -->
```csharp
var export = await client.Learners.ExportAsync(new LearnerExportOptions
{
    Format = LearnersExportRequestFormat.Csv,
    CourseId = "hipaa-2026",
});
var done = await export.WaitAsync(timeout: TimeSpan.FromMinutes(1), interval: TimeSpan.FromSeconds(2));
ExportResult csv = done.GetExportResult();
Console.WriteLine($"{csv.FileName} {csv.RowCount} {csv.Csv.Length}");

var test = await client.WebhookSubscriptions.TestAsync("whs_01JD8D");
switch ((await test.WaitAsync()).GetDeliveryResult())
{
    case WebhookDeliverySent sent:
        Console.WriteLine($"delivered: {sent.ResponseStatus}");
        break;
    case WebhookDeliveryFailed failed:
        Console.WriteLine($"failed: {failed.Reason}");
        break;
}
```

`Operation.OperationIdFromName(operation.Name)` gives the id that
`Operations.GetAsync` takes, if you want to check on an Operation later.

## Uploads and downloads

An Enrollment import takes the CSV as a `FileUpload`: a file path, a
`byte[]` or a `Stream`, each of which converts to one implicitly, or
`FileUpload.FromBytes(bytes, "roster.csv")` to name it. The file is read once
per call, so a retry resends the same bytes.

Certificate PDFs come back whole from `DownloadPdfAsync` as a
`BinaryResponse` (`Data`, `ContentType`, `FileName`).
`DownloadPdfStreamingAsync` returns once the headers arrive, so you read the
file from the connection as it comes; dispose it to release the connection.

<!-- sample: files -->
```csharp
var preview = await client.Enrollments.PreviewImportAsync(
    new EnrollmentPreviewImportOptions("roster.csv") { Options = new() { CourseId = "hipaa-2026" } });
Console.WriteLine($"{preview.ValidRows} of {preview.TotalRows} rows are valid");
await (await client.Enrollments.ImportAsync(new EnrollmentImportOptions("roster.csv"))).WaitAsync();

var pdf = await client.Certificates.DownloadPdfAsync("cert_01JD8F");
await File.WriteAllBytesAsync(pdf.FileName ?? "certificate.pdf", pdf.Data);

await using var stream = await client.Certificates.DownloadPdfStreamingAsync("cert_01JD8F");
await using var copy = File.Create("certificate-copy.pdf");
await stream.Content.CopyToAsync(copy);
```

## Webhooks

`WebhookUtility.ConstructEvent` verifies a Delivery and returns a
`WebhookDelivery` holding its `Event`, or a `TestDelivery` when
`IsTestDelivery`. It needs three things exactly as they arrived:

1. **The raw body.** Parsed and re-serialized JSON never verifies, so read
   the body as bytes before anything binds or parses it.
2. **The headers.** A dictionary or a lookup function.
3. **The request path, with its query string, or the full URL.** The
   signature covers the path the Delivery was sent to, so a verifier that
   drops the path, its query string or the app's path base rejects every
   Delivery.

Deliveries signed more than 5 minutes from your clock, in either direction,
are rejected. Verification throws `WebhookSignatureException` with a `Code`
such as `signature_mismatch`; answer it with 400. Event types this version
doesn't know arrive as an `UnknownEvent`, so new types never break your
handler. `WebhookUtility` needs only the Signing secret, never API
credentials; it is also on a client as `client.Webhooks`.

In an ASP.NET Core minimal API, read the body from `request.Body` yourself
rather than binding a parameter to it, and pass the path base, path and query
string:

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

To test your handler, sign a payload the way TeachMeHIPAA does with
`WebhookUtility.GenerateTestHeaders`:

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

## Partner apps

A Partner app acts for many Organizations, each connected through a Grant
that an admin approves on the TeachMeHIPAA consent screen.
`TeachMeHipaaOAuth` builds the consent URL with a PKCE challenge and
exchanges the returned code. Keep the `state` and code verifier in the
admin's server-side session between the two steps. `ConsentSessions` and
`GrantDatabase` below stand in for your own storage.

<!-- sample: partner-consent -->
```csharp
using TeachMeHipaa;

public sealed class ConsentFlow(ConsentSessions sessions, GrantDatabase grants)
{
    private static readonly Uri RedirectUri = new("https://partner.example.com/tmh/callback");

    /// <summary>Returns the URL to send the Organization's admin to.</summary>
    public string Start()
    {
        var pkce = TeachMeHipaaOAuth.CreatePkcePair();
        var state = TeachMeHipaaOAuth.CreateState();
        sessions.Save(state, pkce.CodeVerifier);
        return TeachMeHipaaOAuth.AuthorizeUrl(new AuthorizeUrlOptions
        {
            ClientId = Environment.GetEnvironmentVariable("TMH_APP_ID")!,
            RedirectUri = RedirectUri,
            Scopes = ["learners:read", "learners:write"],
            State = state,
            CodeChallenge = pkce.CodeChallenge,
        });
    }

    /// <summary>Handles the redirect back to <see cref="RedirectUri"/>.</summary>
    public async Task FinishAsync(string code, string state, CancellationToken cancellationToken)
    {
        var saved = sessions.Load();
        if (state != saved.State)
        {
            throw new UnauthorizedAccessException("state mismatch: possible CSRF");
        }

        var grant = await TeachMeHipaaOAuth.ExchangeCodeAsync(
            new ExchangeCodeOptions
            {
                ClientId = Environment.GetEnvironmentVariable("TMH_APP_ID")!,
                ClientSecret = Environment.GetEnvironmentVariable("TMH_APP_SECRET")!,
                Code = code,
                RedirectUri = RedirectUri,
                CodeVerifier = saved.CodeVerifier,
            },
            cancellationToken: cancellationToken);
        await grants.SaveAsync("org_acme", grant.RefreshToken, cancellationToken);
    }
}
```

A code works once, within 10 minutes, so the exchange is never retried.

Then create a client per authorized Organization with a
`PartnerGrantTokenProvider` and an `ITokenStore`. Every token exchange
rotates the Grant's Refresh token, and the provider saves each new one to the
store before it uses the Access token that came with it.

<!-- sample: partner-grant -->
```csharp
using TeachMeHipaa;

var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    TokenProvider = new PartnerGrantTokenProvider(new PartnerGrantTokenProviderOptions
    {
        ClientId = Environment.GetEnvironmentVariable("TMH_APP_ID")!,
        ClientSecret = Environment.GetEnvironmentVariable("TMH_APP_SECRET")!,
        TokenStore = new GrantStore(grants, "org_acme"),
    }),
    OrganizationId = "org_acme",
});

var learners = await client.Learners.ListAsync();
Console.WriteLine(learners.Items.Count);

/// <summary>The Refresh token of one Organization's Grant.</summary>
public sealed class GrantStore(GrantDatabase grants, string orgId) : ITokenStore
{
    public ValueTask<string> LoadAsync(CancellationToken cancellationToken) =>
        new(grants.LoadAsync(orgId, cancellationToken));

    public ValueTask SaveAsync(string refreshToken, CancellationToken cancellationToken) =>
        new(grants.SaveAsync(orgId, refreshToken, cancellationToken));
}
```

> [!WARNING]
> Presenting a superseded Refresh token revokes the whole Grant (ADR-0004).
> So `SaveAsync` must persist durably before it completes, you should create
> one provider per Grant and share it between clients, and a Refresh token
> exchange is **never retried**: a network error during one throws
> `ApiConnectionException`, and an `OAuthException` with code
> `invalid_grant` means the Organization must go through consent again.
> Never retry these yourself with an old Refresh token. If `SaveAsync`
> throws, the client throws `TokenStoreException` and doesn't use the new
> token.

Give the provider the same `Environment` (and `BaseUrl`) as the client: a
client refuses, with `ArgumentException`, a provider that would exchange
Refresh tokens against another Environment.

For the Partner's own calls, such as listing and creating customer
Organizations, authenticate with the Partner app's own id (`app_…`) and
secret (`tmh_as_…`) as `ClientId` and `ClientSecret`, and pass your
`partnerId`:

<!-- sample: partner-organizations -->
```csharp
using TeachMeHipaa;

var partner = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    ClientId = Environment.GetEnvironmentVariable("TMH_APP_ID")!,
    ClientSecret = Environment.GetEnvironmentVariable("TMH_APP_SECRET")!,
});

await foreach (var organization in partner.Partners.Organizations.ListAutoPagingAsync("ptn_01JD8E2C3D4E5F6G7H8J9K0M1N"))
{
    Console.WriteLine($"{organization.OrgId} {organization.GrantActive}");
}
```

If you get Access tokens some other way, set
`TokenProvider = new StaticTokenProvider(accessToken)`, or implement
`ITokenProvider`. A static token is never renewed; once it expires, calls
throw `AuthenticationException`.

## Tailing the Event log

`Events.TailAsync` reads the Event log oldest first and keeps its own
position in `LastEventId`. Save it, and resume from it after a restart to
catch up on anything you missed while down, without webhooks. In a hosted
service, with `CursorStore` standing in for your own storage:

<!-- sample: events-tail -->
```csharp
using Microsoft.Extensions.Hosting;
using TeachMeHipaa;

public sealed class EventLogFollower(TeachMeHipaaClient client, CursorStore cursors) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tail = client.Events.TailAsync(
            new EventTailOptions { After = await cursors.LoadAsync(stoppingToken), Follow = true, Interval = TimeSpan.FromSeconds(5) },
            cancellationToken: stoppingToken);
        await foreach (var @event in tail)
        {
            Console.WriteLine(@event.EventId);
            await cursors.SaveAsync(tail.LastEventId, stoppingToken);
        }
    }
}
```

Without `Follow`, the loop ends once it has caught up. With it, the tail
polls every `Interval` until you `break` or cancel, which throws
`OperationCanceledException`.

## Configuration

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

Options are `init`-only, and the client copies them when it is created:

| Option               | Default                                     | Description                                                                              |
| -------------------- | ------------------------------------------- | ---------------------------------------------------------------------------------------- |
| `ClientId`           | None: public methods only                   | Your API client's id (`cli_…`), or a Partner app's (`app_…`)                             |
| `ClientSecret`       | Required with `ClientId`                    | The matching secret (`tmh_cs_…` or `tmh_as_…`)                                           |
| `Scopes`             | Every Scope the API client holds            | Narrows the Access token's Scopes                                                        |
| `TokenProvider`      | Client credentials                          | Supplies Access tokens instead: `PartnerGrantTokenProvider`, `StaticTokenProvider`       |
| `Environment`        | `TeachMeHipaaEnvironment.Live`              | `Live` or `Sandbox`; credentials work only in their own Environment                      |
| `BaseUrl`            | The Environment's URL                       | Overrides it, e.g. `http://localhost:4010/v1` for a mock server                          |
| `OrganizationId`     | Looked up once with `Auth.IntrospectAsync`  | The Organization to act for                                                              |
| `MaxRetries`         | `2`                                         | Retries of a failed call that is safe to replay                                          |
| `Timeout`            | 80 seconds                                  | How long to wait for each response, per attempt                                          |
| `AppInfo`            | None                                        | Your software's name, version and URL, sent in the `User-Agent`                          |
| `Logger`             | None: nothing is logged                     | An `ILogger` for one debug entry per attempt (see [Logging](#logging))                   |
| `HttpClient`         | A connection pool shared by every client    | Sends requests, e.g. one from `IHttpClientFactory`; never disposed by the client          |
| `HttpMessageHandler` | A connection pool shared by every client    | The innermost handler, e.g. with a proxy, a client certificate or a test fake; not both  |
| `TimeProvider`       | `TimeProvider.System`                       | The clock for token expiry, backoff and timeouts, e.g. `FakeTimeProvider` in tests        |

Give either `ClientId` and `ClientSecret`, or `TokenProvider`, not both;
`Scopes` then belong to the token provider. With neither, the client makes
only public calls. Base URLs: Live is `https://api.tme.so/v1`; Sandbox is
`https://api-teachmehipaa-dev.web.app/v1`. An invalid option throws
`ArgumentException` naming it when you create the client.

## Logging

The library writes nothing to the console or to disk and sends no telemetry.
Give it an `ILogger` and it logs one `Debug` entry per HTTP response, token
requests included, with the method, the path (without its query string,
which can hold Learner emails), the status, the latency and the request id.
A request that gets no response is logged too, before its exception is
raised, with `ErrorKind` `connection` or `timeout` in place of the status and
the request id you sent, if any. Request and response bodies, headers and credentials are never logged
(ADR-0007), because they hold Learner details.

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

Each entry has the event id 1 (`TeachMeHipaaRequest`) and carries `Method`,
`Path`, `StatusCode`, `LatencyMs` and `RequestId` as structured fields.

## Target frameworks

| Target           | Runs on                                       | Extra dependencies                                                     |
| ---------------- | --------------------------------------------- | ---------------------------------------------------------------------- |
| `net8.0`         | .NET 8 and later                              | Only the two `Microsoft.Extensions` packages below                     |
| `netstandard2.0` | .NET Framework 4.6.2+, Mono, older .NET Core  | `System.Text.Json`, `Microsoft.Bcl.AsyncInterfaces`, `Microsoft.Bcl.TimeProvider` |

Both depend on `Microsoft.Extensions.Logging.Abstractions` (for `ILogger`)
and `Microsoft.Extensions.Http` (for `AddTeachMeHipaa`). Nullable reference
annotations are on throughout, and the `net8.0` build is trimmable and
AOT-compatible.

## Versioning

The library follows semantic versioning. Its services and models are
generated from the Developer API's OpenAPI document; `OPENAPI_VERSION` in
this repository names the document release this version was generated from.
Models are tolerant: unknown response fields are kept in `ExtensionData`, and
enum-like fields are open (`LearnerStatus`, `EventType`, …), accepting values
this version doesn't know, so API additions don't break your code. See the
[changelog](CHANGELOG.md).

## Support and security

<!-- TODO(Nick): confirm the support address before launch. -->

For help, contact TeachMeHIPAA support and quote the `RequestId` from the
exception or `LastResponse`. To report a vulnerability, see
[SECURITY.md](SECURITY.md); please don't open a public issue.

See [CONTRIBUTING.md](CONTRIBUTING.md) to work on the library.
