# Documentation

Documentation for `TeachMeHipaa`, the .NET library for the TeachMeHIPAA
Developer API. Start with the [README](../README.md) for installation and a
quickstart.

## Guides

Every C# sample in these guides is compiled and run against a fake of the
API in CI, so the samples work as written.

| Guide                                                    | Covers                                                                                |
| -------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| [Getting started](guides/getting-started.md)             | Install, Sandbox credentials, a first call, enrolling a Learner, dependency injection |
| [Authentication](guides/authentication.md)               | Client credentials, Scopes, Environments, the Organization binding, static tokens     |
| [Errors and retries](guides/errors-and-retries.md)       | Exception classes, `Code`, request ids, retries, idempotency keys                     |
| [Pagination and Events](guides/pagination-and-events.md) | Pages, `await foreach`, the Event log, `Events.TailAsync`                             |
| [Webhooks](guides/webhooks.md)                           | Webhook subscriptions, verifying Deliveries, Test deliveries, test headers            |
| [Operations and files](guides/operations-and-files.md)   | `WaitAsync`, CSV exports and imports, Certificate PDFs                                |
| [Conditional requests](guides/conditional-requests.md)   | ETags, `If-Match`, `GetIfModifiedAsync`, merge-patch updates                          |
| [Testing](guides/testing.md)                             | The Sandbox, a fake `HttpMessageHandler`, signing test Deliveries                     |
| [Configuration](guides/configuration.md)                 | Every client option and its default, timeouts, your own handler, logging              |
| [Versioning](guides/versioning.md)                       | Semantic versioning, `OPENAPI_VERSION`, additive API changes, target frameworks       |

## API reference

- [Resource reference](reference/resources.md): every service and method,
  with its C# signature, HTTP call, required Scopes, retry, idempotency and
  conditional behavior, options and return type. Generated from the SDK IR
  by `tools/Generator`.
- [API reference](https://teachmehipaa.github.io/teachmehipaa-dotnet/):
  every public type and member with its XML documentation, built with
  [DocFX](https://dotnet.github.io/docfx/) and published from `main`.
  Build it locally with the command in
  [CONTRIBUTING.md](../CONTRIBUTING.md#documentation).

## Examples

[`examples/`](../examples/) holds small programs you can run against the
Sandbox:

- [`Quickstart`](../examples/Quickstart/): a first call.
- [`SyncRoster`](../examples/SyncRoster/): sync a CSV roster with upserts
  by External ID.
- [`WebhookEndpoint`](../examples/WebhookEndpoint/): an ASP.NET Core
  minimal API that verifies Deliveries.
- [`ExportCertificates`](../examples/ExportCertificates/): export
  Certificates to a CSV file and wait for the Operation.

## Contributing / design records

For people working on the library rather than with it:

- [CONTRIBUTING.md](../CONTRIBUTING.md): setup, checks, regeneration,
  releases and how these docs are built.
- [GLOSSARY.md](../GLOSSARY.md): the domain terms every SDK uses.
- [`adr/`](adr/): architecture decision records.
  - [0001](adr/0001-generated-surface-from-shared-ir.md) Hand-written core,
    generated surface from one shared IR
  - [0002](adr/0002-client-object-only.md) Instantiated client only, no
    global configuration
  - [0003](adr/0003-organization-bound-to-client.md) The Organization is
    bound to the client, not passed per call
  - [0004](adr/0004-refresh-tokens-are-never-retried.md) Refresh-token
    exchanges are never retried (superseded)
  - [0005](adr/0005-webhook-verification-needs-the-path.md) Webhook
    verification takes the request path, and Test deliveries are their own
    type
  - [0006](adr/0006-tolerant-models-open-enums.md) Models tolerate unknown
    fields and enums are open
  - [0007](adr/0007-no-telemetry-no-body-logging.md) No telemetry beyond
    User-Agent, and bodies are never logged
  - [0008](adr/0008-async-only-system-text-json.md) Async-only,
    System.Text.Json source generation, `DelegatingHandler` pipeline
- [`design/`](design/): the design grilling the SDKs were built from.
- [`research/`](research/): research into Stripe's .NET library, whose
  conventions this library follows where they fit.
- [`agents/`](agents/): how coding agents work in this repository (issue
  tracker, triage labels, domain docs).
- [`docfx/`](docfx/): the DocFX project that builds the API reference.
