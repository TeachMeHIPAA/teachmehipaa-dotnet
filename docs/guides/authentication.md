# Authentication

An API client is a machine credential that belongs to exactly one
Organization in one Environment. The library turns its client id and
secret into short-lived Access tokens for you, and works out which
Organization they act for.

## Client credentials

Set the API client's id and secret. Read them from the environment, your
configuration or a secret manager; never commit them.

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

The client exchanges the credentials for an Access token on the first call
that needs one, caches it and renews it 60 seconds before it expires.
Concurrent calls share one token request. If the API rejects a token
mid-flight (401), the client renews it once and replays the call; that
replay doesn't count as a retry. The secret is sent only to the token
endpoint, and the library never logs credentials or tokens.

`Auth.IntrospectAsync` describes the Access token: whether it belongs to an
API client (`ActorType`), its Organization (`OrgId`), its `Scopes`, its
Environment (`Mode`: `test` in the Sandbox, `live` in Live) and when it
expires. It needs no Scope.

If the token endpoint refuses the credentials, the call throws
`OAuthException`, with the OAuth error in `Code` (such as
`invalid_client`).

## Scopes

A Scope is a named permission an Access token carries. An API client holds
the Scopes an admin granted it, and by default each Access token gets all
of them. Set `Scopes` to ask for fewer, as above, so a process that only
reads can't write.

| Scope               | Allows                                                              |
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

The [resource reference](../reference/resources.md) lists the Scopes each
method needs. A call whose token lacks one throws
`PermissionDeniedException` (403).

## Environments

Every credential belongs to one Environment, and works only there.

| `Environment`                            | Base URL                                  | Use it for                                     |
| ---------------------------------------- | ----------------------------------------- | ---------------------------------------------- |
| `TeachMeHipaaEnvironment.Live` (default) | `https://api.tme.so/v1`                   | Your real Organization and Learners            |
| `TeachMeHipaaEnvironment.Sandbox`        | `https://api-teachmehipaa-dev.web.app/v1` | Building and testing; no real Learners emailed |

`Environment` defaults to `Live`, so set
`Environment = TeachMeHipaaEnvironment.Sandbox` with Sandbox credentials.
Sandbox credentials sent to Live (or the other way round) fail with
`OAuthException`. To point the client at something else, such as a mock
server, set `BaseUrl`; it replaces the Environment's URL.

## The Organization binding

An API client belongs to one Organization, so you never pass an `orgId`.
On the first Organization-scoped call, the client asks
`Auth.IntrospectAsync` which Organization the token is for, once, and
remembers the answer. Concurrent first calls share that lookup. If you
already know the id, set it and the client skips the lookup:

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

A client's configuration is fixed when you create it. To act for another
Organization, create another client with that Organization's credentials.

## Access tokens you already have

If something else gets Access tokens for you, set `TokenProvider` instead
of credentials. `StaticTokenProvider` sends one token as it is:

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

A static token is never renewed; once it expires, calls throw
`AuthenticationException`.

To fetch tokens your own way, implement `ITokenProvider`:
`GetAccessTokenAsync` returns a token to send, and `Invalidate(accessToken)`
drops one the API refused, so the next `GetAccessTokenAsync` gets a fresh
one. Set either `ClientId` and `ClientSecret`, or `TokenProvider`, not
both; `Scopes` belong to the token provider then.

## Public lookups without credentials

Three methods are public and send no Access token at all:
`Courses.ListAsync`, `Courses.GetAsync` and
`CertificateVerifications.GetAsync`. To call only those, such as to check a
Certificate someone showed you, create a client without credentials:

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

Any other method on such a client throws `InvalidOperationException`
naming the endpoint before it sends anything, the Organization lookup
included. Setting only one of `ClientId` and `ClientSecret` throws
`ArgumentException` when you create the client. Certificate verification
is limited to 60 requests a minute per IP address.
