# Documentation

The API reference and guides for the TeachMeHIPAA client library for .NET
live at
[docs.teachmehipaa.com/api](https://docs.teachmehipaa.com/api?lang=dotnet).
The [README](../README.md) has install steps and a first call.

## Code samples

[`reference/code-samples.json`](reference/code-samples.json) holds one
complete program per API operation, for the C# tab of the hosted API
reference. `tools/Generator` writes it from the SDK IR, and
`CodeSampleTests` compiles each program.

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

- [CONTRIBUTING.md](../CONTRIBUTING.md): setup, checks, regeneration and
  releases.
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
