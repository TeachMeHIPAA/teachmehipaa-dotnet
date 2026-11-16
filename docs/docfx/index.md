---
title: TeachMeHIPAA client library for .NET
---

# TeachMeHIPAA client library for .NET

`TeachMeHipaa` calls the TeachMeHIPAA Developer API from .NET: enroll your
people in HIPAA training, track their compliance and react to
certifications from your own systems. It targets `net8.0` and
`netstandard2.0`, and every method is asynchronous and takes a
`CancellationToken`.

> [!IMPORTANT]
> The Developer API is in preview, and the package isn't on NuGet yet.
> Build it from source as the [quickstart](../articles/quickstart.md) shows.
> Type and member names can change before 1.0.0.

## Get started

- [Quickstart: Call the API from a console app](../articles/quickstart.md)
- [Authenticate the client](../articles/authenticate.md)
- [Use dependency injection](../articles/dependency-injection.md)

## Common tasks

- [Handle errors and retries](../articles/handle-errors.md)
- [Iterate over paged results](../articles/paginate.md)
- [Receive webhooks in ASP.NET Core](../articles/receive-webhooks.md)
- [Wait for long-running operations](../articles/long-running-operations.md)
- [Import and export files](../articles/files.md)

## Reference

- [Resource reference](../reference/resources.md): every service and method
  with its HTTP call, Scopes and retry behavior.
- [API reference](xref:TeachMeHipaa): every public type and member.
  Start with <xref:TeachMeHipaa.TeachMeHipaaClient> and
  <xref:TeachMeHipaa.TeachMeHipaaClientOptions>; webhook verification is
  <xref:TeachMeHipaa.WebhookUtility>.
- [Source code and examples](https://github.com/TeachMeHIPAA/teachmehipaa-dotnet)
