# TeachMeHIPAA client library for .NET

[![CI](https://github.com/TeachMeHIPAA/teachmehipaa-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/TeachMeHIPAA/teachmehipaa-dotnet/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Official .NET library for the TeachMeHIPAA Developer API.

## Documentation

**[docs.teachmehipaa.com/api](https://docs.teachmehipaa.com/api?lang=dotnet)**
has the API reference with C# samples for every operation, plus guides for
authentication, pagination, errors and webhooks.

## Install

> [!IMPORTANT]
> **Preview.** The Developer API isn't generally available, and the
> `TeachMeHipaa` package isn't on NuGet yet. Build the library from source
> as described below. Type and member names can change before 1.0.0.

Clone this repository beside your project and add a project reference:

```sh
git clone https://github.com/TeachMeHIPAA/teachmehipaa-dotnet.git
cd YourApp
dotnet add reference ../teachmehipaa-dotnet/src/TeachMeHipaa/TeachMeHipaa.csproj
```

When the package ships, the command becomes
`dotnet add package TeachMeHipaa`.

## Requirements

- .NET 8 or later, or .NET Framework 4.6.2 or later through the
  `netstandard2.0` build. Building from source needs the .NET 8 SDK or
  later.
- An API client created in the TeachMeHIPAA console, with its client id
  (`cli_…`) and secret (`tmh_cs_…`). Start in the Sandbox Environment
  (`https://sandbox.api.tme.so/v1`).

## Usage

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
```

The client is thread-safe: create one per set of credentials and reuse it.
In ASP.NET Core, register it with `AddTeachMeHipaa`. The
[`examples/`](examples/) folder holds four complete programs.

## Support

<!-- TODO(Nick): confirm the support address before launch. -->

Every failed call throws a subclass of `TeachMeHipaaException` whose
`RequestId` identifies the call. Quote it when you contact TeachMeHIPAA
support. Release notes are in the [changelog](CHANGELOG.md).

## Security

To report a security vulnerability, follow [SECURITY.md](SECURITY.md) and
don't open a public issue.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) to build and test the library.

## License

Licensed under the [MIT License](LICENSE).
