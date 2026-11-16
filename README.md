# TeachMeHIPAA client library for .NET

[![CI](https://github.com/TeachMeHIPAA/teachmehipaa-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/TeachMeHIPAA/teachmehipaa-dotnet/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Use the TeachMeHIPAA client library for .NET to enroll your workforce in
HIPAA training and track their compliance from your own HR system or back
office.

[Source code](src/TeachMeHipaa) |
[Guides](docs/README.md) |
[Resource reference](docs/reference/resources.md) |
[API reference](https://teachmehipaa.github.io/teachmehipaa-dotnet/) |
[Examples](examples/)

> [!IMPORTANT]
> **Preview.** The Developer API isn't generally available, and the
> `TeachMeHipaa` package isn't on NuGet yet. Build the library from source
> as described below. Type and member names can change before 1.0.0.

## Getting started

### Prerequisites

- .NET 8 or later, or .NET Framework 4.6.2 or later through the
  `netstandard2.0` build. Building from source needs the .NET 8 SDK or
  later.
- An API client created in the TeachMeHIPAA console, with its client id
  (`cli_…`) and secret (`tmh_cs_…`). Start in the Sandbox Environment.

### Install the library

Clone this repository beside your project and add a project reference:

```sh
git clone https://github.com/TeachMeHIPAA/teachmehipaa-dotnet.git
cd YourApp
dotnet add reference ../teachmehipaa-dotnet/src/TeachMeHipaa/TeachMeHipaa.csproj
```

When the package ships, the command becomes
`dotnet add package TeachMeHipaa`.

### Authenticate the client

Pass the API client's credentials and the Environment they belong to.
The client exchanges them for Access tokens and renews the tokens before
they expire:

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

In ASP.NET Core, register the client with `AddTeachMeHipaa` in place of
`new`. See [Use dependency injection](docs/articles/dependency-injection.md).

## Key concepts

`TeachMeHipaaClient` is the entry point. Services hang off it, one per
API resource: `Learners`, `Enrollments`, `Certificates`, `Compliance`,
`WebhookSubscriptions` and the rest. A client acts for the one
Organization its credentials belong to, so no method asks for an
Organization id.

### Thread safety and lifetime

The client is thread-safe. Create one per set of credentials and reuse it
for the life of the process: it caches the Access token and the
Organization. All clients share a pooled connection handler by default,
so there is nothing to dispose.

### Additional concepts

| Concept | Where to read more |
| --- | --- |
| Client options, timeouts, `ILogger` logging | [Configure the client](docs/articles/configure.md) |
| Typed exceptions, retries, idempotency keys | [Handle errors and retries](docs/articles/handle-errors.md) |
| `await foreach` over list results | [Iterate over paged results](docs/articles/paginate.md) |
| Exports, imports and other `Operation`s | [Wait for long-running operations](docs/articles/long-running-operations.md) |
| ETags and merge-patch updates | [Prevent lost updates with ETags](docs/articles/optimistic-concurrency.md) |
| Testing with a fake `HttpMessageHandler` | [Unit test code that uses the client](docs/articles/unit-testing.md) |
| Target frameworks and versioning | [Versioning and supported platforms](docs/articles/support-policy.md) |

## Examples

- [Quickstart: Call the API from a console app](docs/articles/quickstart.md)
- [Authenticate the client](docs/articles/authenticate.md): Scopes,
  Environments and public calls without credentials.
- [Receive webhooks in ASP.NET Core](docs/articles/receive-webhooks.md):
  verify signed Deliveries in a minimal API or a controller.
- [Read and follow the Event log](docs/articles/event-log.md): tail Events
  from a `BackgroundService`.
- [Import and export files](docs/articles/files.md): CSV rosters and
  Certificate PDFs.

The [`examples/`](examples/) folder holds four complete programs that
build with the solution.

## Troubleshooting

Every failed call throws a subclass of `TeachMeHipaaException`, chosen by
HTTP status, such as `NotFoundException` for 404 or `RateLimitException`
for 429. `Code` carries the API's machine-readable problem code, and
`RequestId` identifies the call. Quote the request id when you contact
TeachMeHIPAA support. The [error handling guide](docs/articles/handle-errors.md)
lists every exception type.

<!-- TODO(Nick): confirm the support address before launch. -->

## Next steps

- Read the [guides](docs/README.md), each with compiled and tested
  samples.
- Browse every method in the [resource reference](docs/reference/resources.md).

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) to build and test the library, and
the [changelog](CHANGELOG.md) for release notes. To report a security
vulnerability, follow [SECURITY.md](SECURITY.md) and don't open a public
issue. Licensed under the [MIT License](LICENSE).
