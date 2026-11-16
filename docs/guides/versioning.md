# Versioning

## Semantic versioning

The library follows [semantic versioning](https://semver.org/). Within a
major version, upgrades don't break code that uses the public API: the
public types in the `TeachMeHipaa` namespace, the client's services and
methods, and their documented behavior. Internal types can change in any
release.

- **Patch** releases fix bugs.
- **Minor** releases add features, such as new methods, options, models,
  properties or Event types.
- **Major** releases may remove or rename something, or change behavior you
  rely on. The [changelog](../../CHANGELOG.md) lists every change.

The library is pre-release until 1.0.0 ships with the Developer API's
general availability; until then, names and behavior can still change.

The client sends its version in its `User-Agent`, as
`teachmehipaa-dotnet/<version>`.

## `OPENAPI_VERSION`

The services and models are generated from the Developer API's OpenAPI
document. [`OPENAPI_VERSION`](../../OPENAPI_VERSION) in this repository
names the release of that document a version of the library was generated
from, and the changelog repeats it for each release. A newer release of the
API document reaches you as a new library release.

## Additive API changes

The Developer API changes additively: it adds endpoints, optional
parameters, response fields, enum values, error codes and Event types, and
doesn't remove or rename them. The library is built so those additions
don't break an older version of it:

- Models keep response fields they don't know, in `ExtensionData`, instead
  of failing to parse.
- Enum-like types are open: `LearnerStatus`, `EventType` and the rest are
  structs with a static property per known value, and a value this version
  doesn't know parses into one that compares unequal to all of them, with
  its text in `Value`.
- An Event of a type this version doesn't know arrives as `UnknownEvent`,
  and other unions have their own `Unknown*` fallback, such as
  `UnknownWebhookDeliveryResult`.
- Exception `Code`s are open strings, so a new code still throws the right
  exception for its HTTP status.

So handle the values you know and keep a fallback for the rest: an
`else` for enum values, a `default` case for Event types, and a catch-all
for exception codes. Upgrade to use what the API added.

## Supported .NET versions

| Target           | Runs on                                      | Extra dependencies                                                                 |
| ---------------- | -------------------------------------------- | ---------------------------------------------------------------------------------- |
| `net8.0`         | .NET 8 and later                             | Only the two `Microsoft.Extensions` packages below                                 |
| `netstandard2.0` | .NET Framework 4.6.2+, Mono, older .NET Core | `System.Text.Json`, `Microsoft.Bcl.AsyncInterfaces`, `Microsoft.Bcl.TimeProvider` |

Both depend on `Microsoft.Extensions.Logging.Abstractions` (for `ILogger`)
and `Microsoft.Extensions.Http` (for `AddTeachMeHipaa`). Nullable reference
annotations are on throughout, and the `net8.0` build is trimmable and
AOT-compatible, since its JSON is source-generated. CI tests every change on
.NET 8 and .NET 10.
