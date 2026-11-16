---
title: "Quickstart: Call the TeachMeHIPAA API from .NET"
description: Build a console app that reads your Organization's compliance from the Sandbox, lists Learners and enrolls one.
ms.topic: quickstart
ms.date: 11/16/2026
---

# Quickstart: Call the TeachMeHIPAA API from a .NET console app

You build a console app that signs in to the TeachMeHIPAA Sandbox, prints
your Organization's compliance rate, lists Learners and enrolls a new one.

> [!IMPORTANT]
> The Developer API is in preview, and the `TeachMeHipaa` package isn't on
> NuGet yet. Until it ships, you reference the library's project from a
> clone of this repository. Type and member names can change before 1.0.0.

## Prerequisites

- The [.NET 8 SDK](https://dotnet.microsoft.com/download) or later, and Git.
- An API client in the **Sandbox** Environment of the TeachMeHIPAA
  console, with its client id (`cli_…`) and secret (`tmh_cs_…`). An
  Organization admin creates it. Sandbox credentials work only against the
  Sandbox, which keeps its data apart from Live.

## Create the project

Create a console app and reference the library from a clone placed beside
it:

```sh
git clone https://github.com/TeachMeHIPAA/teachmehipaa-dotnet.git
dotnet new console --name ComplianceCheck
cd ComplianceCheck
dotnet add reference ../teachmehipaa-dotnet/src/TeachMeHipaa/TeachMeHipaa.csproj
```

After the package ships on NuGet, `dotnet add package TeachMeHipaa`
replaces the last command.

## Store your credentials

Keep the secret out of *Program.cs*. The app reads both values from
environment variables.

In Bash or zsh:

```sh
export TMH_CLIENT_ID=cli_...
export TMH_CLIENT_SECRET=tmh_cs_...
```

In PowerShell:

```powershell
$env:TMH_CLIENT_ID = "cli_..."
$env:TMH_CLIENT_SECRET = "tmh_cs_..."
```

ASP.NET Core apps keep them in configuration instead, such as
[user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)
during development. See [Use dependency injection](dependency-injection.md).

## Read compliance and list Learners

Replace the contents of *Program.cs* with this code:

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

Run the app:

```sh
dotnet run
```

The first call trades your credentials for an Access token, then asks the
API which Organization that token acts for. The client caches both, so
the list call that follows goes straight to the Learners endpoint.
`ListAutoPagingAsync` fetches pages of 20 as the loop asks for them; see
[Iterate over paged results](paginate.md).

A few conventions hold across the library:

- Services hang off the client: `client.Learners`, `client.Enrollments`,
  `client.Certificates` and the rest.
- A method takes path ids first, then an options object such as
  `LearnerCreateOptions`, then an optional `RequestOptions` and a
  `CancellationToken`.
- Every returned object has a `LastResponse` with the HTTP `StatusCode`,
  `Headers`, `RequestId` and `ETag`.

`TeachMeHipaaClient` is thread-safe. Create one per set of credentials and
keep it for the life of the process. It sends through a connection pool
that all clients share, so it has nothing to dispose.

## Enroll a Learner

Add these lines to the end of *Program.cs*:

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

`CreateAsync` invites a Learner to a Course. `UpsertByExternalIdAsync`
keys the same write on your own id for the person (their External ID),
which suits a nightly HR sync: the first run creates and invites the
Learner, and every later run returns the existing record. Both results
carry `Created`, which tells you which case happened.

Each of these writes goes out with an `Idempotency-Key` that the client
reuses on retries, so a retried call never sends a second invite. See
[Handle errors and retries](handle-errors.md#make-a-write-safe-to-repeat).

## Clean up resources

Learners you create in the Sandbox stay there until you remove them. To
offboard the one this quickstart enrolled, call
`client.Learners.OffboardAsync(synced.LearnerId)`.

## Next steps

- [Authenticate the client](authenticate.md): Scopes, Live credentials
  and the Organization binding.
- [Use dependency injection](dependency-injection.md): register the client
  in ASP.NET Core with `IHttpClientFactory`.
- [Receive webhooks in ASP.NET Core](receive-webhooks.md): hear about
  certifications as they happen.
- [Resource reference](../reference/resources.md): every service and
  method.
- [Example programs](https://github.com/TeachMeHIPAA/teachmehipaa-dotnet/tree/main/examples):
  complete apps to copy from.
