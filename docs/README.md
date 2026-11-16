# TeachMeHIPAA .NET documentation

Guides and reference for `TeachMeHipaa`, the .NET client library for the
TeachMeHIPAA Developer API. Installation and a first sample are in the
[README](../README.md). The same guides are published with the API
reference at <https://teachmehipaa.github.io/teachmehipaa-dotnet/>.

CI compiles every C# sample in these guides and runs it against a fake of
the API, so the code works as written.

## Get started

| Article                                                     | You learn to                                                  |
| ----------------------------------------------------------- | ------------------------------------------------------------- |
| [Quickstart: Call the API from a console app](articles/quickstart.md) | Reference the library, read compliance and enroll a Learner |

## How-to guides

| Article                                                                    | You learn to                                                                  |
| -------------------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| [Authenticate the client](articles/authenticate.md)                        | Use client credentials, narrow Scopes, pick an Environment, bring your own tokens |
| [Use dependency injection](articles/dependency-injection.md)               | Register the client with `AddTeachMeHipaa` and customize its `HttpClient`     |
| [Configure the client](articles/configure.md)                              | Set every option, time out a single call, log requests with `ILogger`        |
| [Handle errors and retries](articles/handle-errors.md)                     | Catch typed exceptions, read request ids, make writes safe to repeat          |
| [Iterate over paged results](articles/paginate.md)                         | Use `await foreach`, step through pages, cancel between pages                 |
| [Read and follow the Event log](articles/event-log.md)                     | List Events by type and tail the log from a `BackgroundService`               |
| [Receive webhooks in ASP.NET Core](articles/receive-webhooks.md)           | Subscribe, verify Deliveries, send Test deliveries, redrive failures          |
| [Wait for long-running operations](articles/long-running-operations.md)    | Poll an `Operation` with `WaitAsync` and resume from another process          |
| [Import and export files](articles/files.md)                               | Export CSV files, import a roster, download Certificate PDFs                  |
| [Prevent lost updates with ETags](articles/optimistic-concurrency.md)      | Update with `If-Match`, send merge patches, skip unchanged reads              |
| [Unit test code that uses the client](articles/unit-testing.md)            | Fake the transport, sign Deliveries, test a webhook endpoint                  |

## Concepts

| Article                                                             | Covers                                                                   |
| ------------------------------------------------------------------- | ------------------------------------------------------------------------ |
| [Versioning and supported platforms](articles/support-policy.md)    | Target frameworks, semantic versioning, `OPENAPI_VERSION`, additive changes |

## Reference

- [Resource reference](reference/resources.md): each service and method
  with its C# signature, HTTP call, Scopes, retry and idempotency
  behavior, and options. `tools/Generator` writes it from the SDK IR.
- [Code samples](reference/code-samples.json): one complete program per
  API operation, for the C# tab of the TeachMeHIPAA API reference. The
  generator writes it too, and `CodeSampleTests` compiles each program.
- [API reference](https://teachmehipaa.github.io/teachmehipaa-dotnet/):
  every public type and member from the XML docs, built with
  [DocFX](https://dotnet.github.io/docfx/) from [`docfx/`](docfx/) and
  published from `main`.

## Examples

Programs in [`examples/`](../examples/) that run against the Sandbox:

- [`Quickstart`](../examples/Quickstart/): a first call.
- [`SyncRoster`](../examples/SyncRoster/): sync a CSV roster with upserts
  by External ID.
- [`WebhookEndpoint`](../examples/WebhookEndpoint/): an ASP.NET Core
  minimal API that verifies Deliveries.
- [`ExportCertificates`](../examples/ExportCertificates/): export
  Certificates to a CSV file and wait for the Operation.

## For contributors

- [CONTRIBUTING.md](../CONTRIBUTING.md): setup, checks, regeneration,
  releases and how these docs are built and tested.
- [GLOSSARY.md](../GLOSSARY.md): the domain terms every TeachMeHIPAA SDK
  shares.
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
- [`agents/`](agents/): how coding agents work in this repository.
