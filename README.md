# TeachMeHIPAA .NET Library

[![CI](https://github.com/TeachMeHIPAA/teachmehipaa-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/TeachMeHIPAA/teachmehipaa-dotnet/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

The official .NET library for the TeachMeHIPAA Developer API: enroll your
people in HIPAA training and track their compliance from your own systems.

> [!IMPORTANT]
> **Status: pre-release.** The Developer API isn't generally available yet,
> and this package isn't on NuGet. Until it is, build it from source as
> shown below. Names and behavior can still change before 1.0.0.

The library handles Access tokens, the Organization binding, retries,
idempotency keys, typed exceptions, pagination, ETags, Operations and
webhook verification, so your code only calls methods. Every method is
`async` and takes a `CancellationToken`. It targets `net8.0` and
`netstandard2.0`, and is trimmable and AOT-compatible on .NET 8 and later.

## Installation

Clone the repository next to your project and reference the library's
project from yours. Building it needs the .NET 8 SDK or later.

```sh
git clone https://github.com/TeachMeHIPAA/teachmehipaa-dotnet.git
cd YourApp
dotnet add reference ../teachmehipaa-dotnet/src/TeachMeHipaa/TeachMeHipaa.csproj
```

Once the package is on NuGet, this becomes `dotnet add package TeachMeHipaa`.

## Quickstart

Create an API client in the TeachMeHIPAA console to get a client id
(`cli_…`) and secret (`tmh_cs_…`), and start in the Sandbox Environment:

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

The [getting started guide](docs/guides/getting-started.md) walks through
this step by step.

## Documentation

Guides, each with tested samples:

- [Getting started](docs/guides/getting-started.md): install, credentials,
  a first call, enrolling a Learner and dependency injection in ASP.NET
  Core.
- [Authentication](docs/guides/authentication.md): client credentials,
  Scopes, Live and Sandbox Environments, the Organization binding, static
  Access tokens and public lookups without credentials.
- [Errors and retries](docs/guides/errors-and-retries.md): exception
  classes, `Code`, request ids, retries and idempotency keys.
- [Pagination and Events](docs/guides/pagination-and-events.md): pages,
  `await foreach`, the Event log and `Events.TailAsync`.
- [Webhooks](docs/guides/webhooks.md): Webhook subscriptions, verifying
  Deliveries in ASP.NET Core minimal APIs and controllers, and Test
  deliveries.
- [Operations and files](docs/guides/operations-and-files.md):
  `WaitAsync`, CSV exports and imports, and Certificate PDFs.
- [Conditional requests](docs/guides/conditional-requests.md): ETags,
  `If-Match`, `GetIfModifiedAsync` and merge-patch updates.
- [Testing](docs/guides/testing.md): the Sandbox, a fake
  `HttpMessageHandler` and signed test Deliveries.
- [Configuration](docs/guides/configuration.md): every client option,
  timeouts, your own `HttpClient` and logging.
- [Versioning](docs/guides/versioning.md): semantic versioning, target
  frameworks and how API changes reach the library.

Reference:

- [Resource reference](docs/reference/resources.md): every service and
  method, with its signature, HTTP call, Scopes and return type.
- [API reference](https://teachmehipaa.github.io/teachmehipaa-dotnet/):
  every public type and member, built from the XML docs with DocFX.
- [Examples](examples/): small programs you can run against the Sandbox.

The [docs index](docs/README.md) lists everything, design records included.

## Support and security

<!-- TODO(Nick): confirm the support address before launch. -->

For help, contact TeachMeHIPAA support and quote the `RequestId` from the
exception or `LastResponse`. To report a vulnerability, see
[SECURITY.md](SECURITY.md); please don't open a public issue.

See [CONTRIBUTING.md](CONTRIBUTING.md) to work on the library, and the
[changelog](CHANGELOG.md) for what changed. Licensed under the
[MIT License](LICENSE).
