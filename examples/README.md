# Examples

Small programs that use the library against the Sandbox. Each reads an API
client's credentials from `TMH_CLIENT_ID` and `TMH_CLIENT_SECRET` (create
one in the Sandbox Environment; see
[Authenticate the client](../docs/articles/authenticate.md)). They build with the
solution, so CI compiles them, but CI doesn't run them.

| Example                                               | What it does                                                            |
| ----------------------------------------------------- | ----------------------------------------------------------------------- |
| [`Quickstart`](Quickstart/Program.cs)                 | Prints the Compliance summary and the first page of Learners            |
| [`SyncRoster`](SyncRoster/Program.cs)                 | Syncs a roster CSV with upserts by External ID                          |
| [`WebhookEndpoint`](WebhookEndpoint/Program.cs)       | An ASP.NET Core minimal API that verifies Deliveries and handles Events |
| [`ExportCertificates`](ExportCertificates/Program.cs) | Exports Certificates to a CSV file and waits for the Operation          |

From a checkout of this repository, with the .NET 8 SDK or later:

```sh
export TMH_CLIENT_ID=cli_... TMH_CLIENT_SECRET=tmh_cs_...
dotnet run --project examples/Quickstart
dotnet run --project examples/SyncRoster -- roster.csv hipaa-2026
dotnet run --project examples/ExportCertificates -- certificates.csv
TMH_WEBHOOK_SECRET=... dotnet run --project examples/WebhookEndpoint --urls http://localhost:8000
```

`WebhookEndpoint` needs a webhook Signing secret in `TMH_WEBHOOK_SECRET`
instead of API credentials: the one returned when you created the Webhook
subscription. To receive Deliveries on your machine, expose the port
through a tunnel and register its HTTPS URL, ending in `/webhooks/tmh`, as
the subscription's endpoint.

The examples reference the library's project. In your own project, once
the package is on NuGet, use `<PackageReference Include="TeachMeHipaa" />`
instead.
