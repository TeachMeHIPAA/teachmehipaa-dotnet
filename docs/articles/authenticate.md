---
title: Authenticate the TeachMeHIPAA client
description: Sign in with an API client's credentials, narrow Scopes, pick an Environment, bring your own Access tokens or call public methods without credentials.
ms.topic: how-to
ms.date: 11/16/2026
---

# Authenticate the TeachMeHIPAA client

An API client is a machine credential. It belongs to one Organization in
one Environment, and `TeachMeHipaaClient` exchanges its id and secret for
short-lived Access tokens on your behalf.

## Prerequisites

- An API client's id (`cli_…`) and secret (`tmh_cs_…`) from the
  TeachMeHIPAA console, created in the Environment you call.
- The library referenced from your project, as in the
  [quickstart](quickstart.md#create-the-project).

## Authenticate with client credentials

Set `ClientId` and `ClientSecret`. Read them from environment variables,
configuration or a secret manager, and keep them out of source control.

<!-- sample: client-credentials -->
```csharp
using TeachMeHipaa;

var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    ClientId = Environment.GetEnvironmentVariable("TMH_CLIENT_ID")!,
    ClientSecret = Environment.GetEnvironmentVariable("TMH_CLIENT_SECRET")!,
    Environment = TeachMeHipaaEnvironment.Sandbox,
    Scopes = ["learners:read"],
});

TokenIntrospection token = await client.Auth.IntrospectAsync();
Console.WriteLine($"{token.Mode} {token.OrgId} {string.Join(",", token.Scopes)}");
```

The client requests a token on the first call that needs one and caches
it. It renews the token 60 seconds before expiry, and concurrent calls
share a single token request. When the API answers 401 to a token in
flight, the client renews it once and replays the call; that replay
doesn't count against `MaxRetries`. The secret goes only to the token
endpoint, and the library never logs credentials or tokens.

`Auth.IntrospectAsync` needs no Scope. It reports the token's `ActorType`,
its Organization (`OrgId`), its `Scopes`, its Environment in `Mode`
(`test` in the Sandbox, `live` in Live) and `ExpiresAt`.

## Request fewer Scopes

A Scope is a named permission that an Access token carries. By default
each token gets every Scope an admin granted the API client. Set `Scopes`,
as in the previous example, to ask for less: a reporting job that holds
only `learners:read` can't write.

| Scope               | Grants                                                              |
| ------------------- | ------------------------------------------------------------------- |
| `learners:read`     | Reading Learners                                                    |
| `learners:write`    | Creating, updating, offboarding and reminding Learners              |
| `enrollments:read`  | Reading Enrollments and the Seat inventory                          |
| `enrollments:write` | Creating, importing, cancelling, renewing and reminding Enrollments |
| `certificates:read` | Reading, exporting and downloading Certificates                     |
| `compliance:read`   | Reading the Compliance summary                                      |
| `members:read`      | Reading Members                                                     |
| `audit:read`        | Reading Audit events and the Event log                              |
| `webhooks:manage`   | Managing Webhook subscriptions and reading the Event log            |

The [resource reference](../reference/resources.md) lists the Scopes for
each method. A call whose token lacks one throws
`PermissionDeniedException` (403).

## Choose an Environment

Credentials work only in the Environment that issued them.

| `Environment` value                      | Base URL                                  | Use it for                          |
| ---------------------------------------- | ----------------------------------------- | ----------------------------------- |
| `TeachMeHipaaEnvironment.Live` (default) | `https://api.tme.so/v1`                   | Your real Organization and Learners |
| `TeachMeHipaaEnvironment.Sandbox`        | `https://sandbox.api.tme.so/v1` | Development and tests               |

`Environment` defaults to `Live`. Pair Sandbox credentials with
`TeachMeHipaaEnvironment.Sandbox`; credentials sent to the wrong
Environment fail with `OAuthException`. To reach a mock server, set
`BaseUrl`, which replaces the Environment's URL.

## Set the Organization

Organization-scoped paths contain the Organization id, which you never
pass yourself. On the first such call, the client calls
`Auth.IntrospectAsync` once to learn which Organization the token acts for
and keeps the answer. If you already know the id, set `OrganizationId` and
the client skips that request:

<!-- sample: organization -->
```csharp
using TeachMeHipaa;

var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    ClientId = Environment.GetEnvironmentVariable("TMH_CLIENT_ID")!,
    ClientSecret = Environment.GetEnvironmentVariable("TMH_CLIENT_SECRET")!,
    OrganizationId = "org_acme",
});

var compliance = await client.Compliance.GetAsync();
Console.WriteLine($"{compliance.CompliantRate} {compliance.LastResponse!.RequestId}");
```

Options are fixed once the client exists. To act for a second
Organization, create a second client with that Organization's
credentials.

## Supply your own Access tokens

When another component already obtains Access tokens, set `TokenProvider`
in place of the credentials. `StaticTokenProvider` sends one token
unchanged:

<!-- sample: static-token -->
```csharp
using TeachMeHipaa;

var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    TokenProvider = new StaticTokenProvider(accessToken),
    Environment = TeachMeHipaaEnvironment.Sandbox,
});

var compliance = await client.Compliance.GetAsync();
Console.WriteLine(compliance.CompliantRate);
```

A static token is never renewed. After it expires, calls throw
`AuthenticationException`.

For your own token source, implement `ITokenProvider`.
`GetAccessTokenAsync` returns a token to send, and
`Invalidate(accessToken)` discards one the API rejected so that the next
`GetAccessTokenAsync` fetches a fresh one. A token provider owns its
Scopes, so setting `TokenProvider` together with `ClientId`,
`ClientSecret` or `Scopes` throws `ArgumentException`.

## Call public methods without credentials

`Courses.ListAsync`, `Courses.GetAsync` and
`CertificateVerifications.GetAsync` are public and send no Access token.
A client created without credentials can call them, for example to check
a Certificate that someone shows you:

<!-- sample: public-lookups -->
```csharp
using TeachMeHipaa;

var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    Environment = TeachMeHipaaEnvironment.Sandbox,
});

var verification = await client.CertificateVerifications.GetAsync("aB3dE9");
Console.WriteLine($"{verification.Status} {verification.LearnerName}");

await foreach (var course in client.Courses.ListAutoPagingAsync())
{
    Console.WriteLine(course.Title);
}
```

The API limits Certificate verification to 60 requests a minute per IP
address.

## Troubleshoot authentication

| Symptom                                                   | Cause and fix                                                                                 |
| --------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| `OAuthException` with `Code` `invalid_client`             | The id or secret is wrong, or belongs to the other Environment. Check `Environment`.          |
| `AuthenticationException` (401) on every call             | A static token expired. Supply a fresh one, or switch to client credentials.                  |
| `PermissionDeniedException` (403)                         | The token lacks a Scope. Grant it to the API client, or widen `Scopes`.                       |
| `InvalidOperationException` before any request            | A client without credentials called a method that needs a token. The message names the endpoint. |
| `ArgumentException` when you create the client            | Only one of `ClientId` and `ClientSecret` is set, or credentials are mixed with `TokenProvider`. |

## Next steps

- [Use dependency injection](dependency-injection.md)
- [Configure the client](configure.md)
- [Handle errors and retries](handle-errors.md)
