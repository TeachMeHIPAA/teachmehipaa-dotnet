---
title: Versioning and supported platforms
description: Target frameworks and dependencies, the library's semantic versioning, OPENAPI_VERSION, and how the library tolerates additive API changes.
ms.topic: conceptual
ms.date: 11/16/2026
---

# Versioning and supported platforms

## Supported platforms

| Target framework | Runs on                                      | Extra dependencies                                                                 |
| ---------------- | -------------------------------------------- | ---------------------------------------------------------------------------------- |
| `net8.0`         | .NET 8 and later                             | None beyond the two below                                                          |
| `netstandard2.0` | .NET Framework 4.6.2 or later, Mono, older .NET Core | `System.Text.Json`, `Microsoft.Bcl.AsyncInterfaces`, `Microsoft.Bcl.TimeProvider` |

Both targets depend on `Microsoft.Extensions.Logging.Abstractions`, for
`ILogger`, and `Microsoft.Extensions.Http`, for `AddTeachMeHipaa`.
Nullable reference annotations cover the whole public surface. The
`net8.0` build is trimmable and Native AOT compatible, because its JSON
serialization is source-generated. CI tests every change on .NET 8 and
.NET 10.

## Semantic versioning

The library follows [Semantic Versioning 2.0](https://semver.org/). Within
a major version, an upgrade doesn't break code that uses the public
surface: the public types in the `TeachMeHipaa` namespace, the client's
services and methods, and their documented behavior. Internal types can
change in any release.

| Release | Contains                                                                        |
| ------- | ------------------------------------------------------------------------------- |
| Patch   | Bug fixes                                                                       |
| Minor   | Additions: methods, options, models, properties, Event types                   |
| Major   | Removals, renames, or changes to behavior you might depend on                  |

The [changelog](https://github.com/TeachMeHIPAA/teachmehipaa-dotnet/blob/main/CHANGELOG.md)
lists every change. Until 1.0.0 ships with the Developer API's general
availability, the library is a preview, and names and behavior can still
change between releases.

The client reports its version in the `User-Agent` header as
`teachmehipaa-dotnet/<version>`.

## The OpenAPI release behind a build

The services and models are generated from the Developer API's OpenAPI
document. The repository's
[`OPENAPI_VERSION`](https://github.com/TeachMeHIPAA/teachmehipaa-dotnet/blob/main/OPENAPI_VERSION)
file names the document release a build was generated from, and the
changelog repeats it for each release. A newer API document reaches you
as a new library release.

## Forward compatibility

The Developer API changes additively. It adds endpoints, optional
parameters, response fields, enum values, error codes and Event types, and
it doesn't remove or rename them. An older library version keeps working
when those additions arrive:

- Models keep unknown response fields in `ExtensionData` and parse
  without error.
- Enum-like types such as `LearnerStatus` and `EventType` are open
  structs with a static property per known value. An unknown value parses
  into an instance that equals none of them and keeps its text in
  `Value`.
- An Event of an unknown type arrives as `UnknownEvent`. Other unions have
  their own fallback, such as `UnknownWebhookDeliveryResult`.
- Exception `Code` values are open strings, so a new code still maps to
  the exception for its HTTP status.

Write code with that in mind: handle the values you know and keep a
fallback for the rest, such as a `default` arm in a `switch` over Event
types or enum values, and a general `catch` for exception codes. Upgrade
when you want to use what the API added.
