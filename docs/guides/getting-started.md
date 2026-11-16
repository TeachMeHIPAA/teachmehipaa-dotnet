# Getting started

This guide takes you from an empty project to calls against the Sandbox:
reading your Organization's compliance, listing Learners and enrolling one,
then using the client from ASP.NET Core.

## Requirements

The library targets `net8.0` and `netstandard2.0`, so it runs on .NET 8 and
later, and on .NET Framework 4.6.2 or later through `netstandard2.0`.
Building it from source needs the .NET 8 SDK or later. Every method is
`async`.

## 1. Install the library

> [!IMPORTANT]
> The Developer API isn't generally available yet and the package isn't on
> NuGet. Until it is, build it from source.

Clone the repository next to your project and reference the library's
project from yours:

```sh
git clone https://github.com/TeachMeHIPAA/teachmehipaa-dotnet.git
cd YourApp
dotnet add reference ../teachmehipaa-dotnet/src/TeachMeHipaa/TeachMeHipaa.csproj
```

Once the package is on NuGet, this becomes
`dotnet add package TeachMeHipaa`.

## 2. Get Sandbox credentials

An Organization admin creates an API client in the TeachMeHIPAA console.
It has a client id (`cli_…`) and a secret (`tmh_cs_…`) and belongs to one
Organization in one Environment. Create it in the **Sandbox** Environment
while you build: no real Learners are emailed there, and Sandbox
credentials never work against Live.

Keep the secret out of your code. The samples read both values from
environment variables:

```sh
export TMH_CLIENT_ID=cli_...
export TMH_CLIENT_SECRET=tmh_cs_...
```

In ASP.NET Core, keep them in configuration instead, such as
[user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)
while you develop (see [step 5](#5-use-the-client-from-aspnet-core)).

## 3. Make your first call

<!-- sample: first-call -->
```csharp
using TeachMeHipaa;

var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    ClientId = Environment.GetEnvironmentVariable("TMH_CLIENT_ID")!,
    ClientSecret = Environment.GetEnvironmentVariable("TMH_CLIENT_SECRET")!,
    Environment = TeachMeHipaaEnvironment.Sandbox,
});

ComplianceSummary compliance = await client.Compliance.GetAsync();
Console.WriteLine($"Compliant: {compliance.CompliantRate}");

await foreach (var learner in client.Learners.ListAutoPagingAsync(new() { Limit = 20 }))
{
    Console.WriteLine($"{learner.Email} {learner.Status}");
}
```

On the first call the client exchanges your credentials for an Access
token, then asks the API which Organization the token belongs to. It caches
both, so later calls go straight to the API.

Services hang off the client (`client.Learners`, `client.Enrollments`, …).
A method takes ids in the path first, then an options object such as
`LearnerCreateOptions`, then an optional `RequestOptions` and a
`CancellationToken`. Models have PascalCase properties, and every object a
method returns has a `LastResponse` with the HTTP `StatusCode`, `Headers`,
`RequestId` and `ETag`.

The client is thread-safe and holds the token cache and the Organization,
so create one and share it for the life of your process. It sends through a
pooled connection handler shared by every client, so there is nothing to
dispose.

`ListAutoPagingAsync` yields every item across all pages; see
[Pagination and Events](pagination-and-events.md).

## 4. Enroll a Learner

Samples from here on assume a `client` created as in step 3.

`Learners.CreateAsync` invites a Learner to a Course.
`UpsertByExternalIdAsync` does the same keyed by your own id for the
Learner (their External ID), so a sync job can run it again and again: it
creates and invites the Learner the first time and returns them every time
after. Both say whether they created the Learner in `Created`.

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

The client sends an `Idempotency-Key` with each of these writes and reuses
it if it retries, so a retry never invites a Learner twice. See
[Errors and retries](errors-and-retries.md).

## 5. Use the client from ASP.NET Core

In ASP.NET Core, or any `Microsoft.Extensions.DependencyInjection` host,
`AddTeachMeHipaa` registers one shared `TeachMeHipaaClient` whose requests
go through an `IHttpClientFactory` client:

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
Organization. It asks the factory for an `HttpClient` on every request, so
it always uses the factory's current handler. `AddTeachMeHipaa` returns the
factory client's `IHttpClientBuilder` (named
`TeachMeHipaaServiceCollectionExtensions.HttpClientName`), to add your own
`DelegatingHandler`s or a proxy with `ConfigurePrimaryHttpMessageHandler`.
The options factory runs once, when the client is first resolved, and must
leave `HttpClient` and `HttpMessageHandler` unset. Leave the factory
client's own resilience handlers off: the client already retries what is
safe to retry.

## Next steps

- [Authentication](authentication.md): Scopes, Live credentials and the
  Organization binding.
- [Webhooks](webhooks.md): hear about certifications as they happen.
- [Resource reference](../reference/resources.md): every method.
- [`examples/`](../../examples/): complete programs to copy from.
