# stripe-dotnet: design research for `teachmehipaa-dotnet`

- **Source:** `stripe/stripe-dotnet`, shallow clone at commit `010e2e74da1095731c239f610c299574466eb139` (2026-11-12), package version `53.1.0`, pinned API `2026-10-28.endive`. Local copy: `~/scratch/stripe-src/stripe-dotnet`.
- **Citation format:** `path:line`, linked to the same commit. `GH/` below stands for `https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/`.
- **Docs:** docs.stripe.com, fetched 2026-11-15.
- **Labels:** an *Inference* label marks a conclusion drawn from the code rather than stated by it.

## TL;DR

1. One NuGet package, `Stripe.net`, plus an xUnit test project. Targets: `net8.0;net9.0;net10.0;netstandard2.0;net462`. C# `LangVersion 8`, nullable reference types effectively off, StyleCop on, warnings treated as errors.
2. JSON moved to **System.Text.Json (STJ)** as the default in v51 (2026-03). Newtonsoft is still a dependency, kept only for backward compatibility. Every model carries both sets of attributes.
3. Entry point is `new StripeClient(key | StripeClientOptions)`, then `client.V1.Customers.CreateAsync(...)`. The older global `StripeConfiguration` plus `new CustomerService()` pattern still works but is labelled legacy.
4. Each operation has its own options class (`CustomerCreateOptions`, and so on). A per-call `RequestOptions` carries `ApiKey`, `IdempotencyKey`, `StripeAccount` and similar. Every method ships as a sync + async pair; the sync versions block on the async code.
5. Paging: `StripeList<T>` (`has_more` plus the last item's id) and V2 `next_page_url`. `ListAutoPaging` returns `IEnumerable<T>`; `ListAutoPagingAsync` returns `IAsyncEnumerable<T>` and honours `CancellationToken`.
6. Errors: a single `StripeException` with `HttpStatusCode`, `StripeError` (the parsed envelope) and `StripeResponse` (headers, request id). V2 errors are mapped by `error.type` to typed subclasses through a generated `switch`.
7. Retries: 2 by default. Backoff runs 0.5 s × 2ⁿ, capped at 5 s, with 75–100% jitter. Retried cases: network errors, 409, 5xx, and whatever `Stripe-Should-Retry` says. **`Retry-After` is ignored and 429 is not retried.** Every POST gets an automatic GUID `Idempotency-Key`, reused on each retry.
8. Webhooks: `EventUtility.ConstructEvent` / `ValidateSignature` check an HMAC-SHA256 of `"{t}.{body}"` against `v1=`, with a 300 s tolerance, constant-time comparison, and an optional API-version mismatch error. Newer "thin events" use `EventNotification` plus a typed handler that dispatches by event.
9. Escape hatches: `RawRequestAsync`, `Deserialize<T>`, `AddExtraParam`, `RawJsonElement`. Uploads use multipart through `MultipartFileContent{Stream}`. PDF downloads return a `Stream` (`RequestStreamingAsync`), but the body is fully buffered first.
10. For us: copy the overall shape (client, services, options, RequestOptions, AutoPaging, a single HTTP seam, retry policy, idempotency, webhook utility). Write the core fresh on STJ source-gen, async-first, with nullable enabled. Add what Stripe lacks: token-caching OAuth, problem+json, ETag, LRO, `Retry-After`, a real streaming download, and logging/`ActivitySource`.

---

## 1. Repo layout, projects, TFMs, dependencies, analyzers, signing

**Layout.** The root holds `VERSION` (`53.1.0`), `OPENAPI_VERSION` (`v2596`), `CODEGEN_VERSION` (a hash), `CHANGELOG.md`, `justfile` (`Makefile` is marked deprecated), `.hark/` (changelog fragments) and `.github/workflows/{ci,canary,changelog,rules,zizmor}.yml`. Under `src/`:

| Path | Role |
|---|---|
| `src/Stripe.net.sln` | Projects: `Stripe.net`, `StripeTests`, `Examples`, `ExampleWebApp` |
| `src/Stripe.net/Services/<Resource>/` | `XService.cs` plus one `X<Op>Options.cs` per operation |
| `src/Stripe.net/Entities/<Resource>/` | Response models |
| `src/Stripe.net/Infrastructure/Public/` | Hand-written core: `StripeClient`, `SystemNetHttpClient`, `LiveApiRequestor`, `StripeRequest`/`StripeResponse`, `StripeException`, `StripeConfiguration` |
| `src/Stripe.net/Infrastructure/JsonConverters/` | 28 converters (Newtonsoft plus `STJ*` versions) |
| `src/Stripe.net/Infrastructure/FormEncoding/` | Form, multipart and JSON request encoders |
| `src/Stripe.net/Exceptions/V2/` | Typed V2 exceptions (generated) |
| `src/Stripe.net/Events/` | Typed thin-event notifications (generated) |
| `src/StripeTests/` | xUnit tests and stripe-mock fixtures |
| `src/_stylecop/*.ruleset` | Analyzer rulesets |

**Main csproj:** [`src/Stripe.net/Stripe.net.csproj`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Stripe.net.csproj).

- `<TargetFrameworks>net8.0;net9.0;net10.0;netstandard2.0;net462</TargetFrameworks>`, `<LangVersion>8</LangVersion>`.
- `TreatWarningsAsErrors=true`, `GenerateDocumentationFile=true`, `DebugType=embedded`, `EmbedUntrackedSources`, SourceLink (`Microsoft.SourceLink.GitHub`).
- `EnablePackageValidation=true`. A baseline API-compat check runs when `RunBaselineCheck=true` (CI `compat` job).
- Dependencies:
  - `Newtonsoft.Json 13.0.3` on all TFMs.
  - `System.Configuration.ConfigurationManager` on all TFMs (feeds the `AppSettings["StripeApiKey"]` fallback).
  - On netstandard2.0 and net462: `System.Text.Json 6.0.10`, `Microsoft.Bcl.AsyncInterfaces 6.0.0` (for `IAsyncEnumerable`), and `Microsoft.CSharp`.

**Runtime policy** ([docs.stripe.com/sdks/versioning](https://docs.stripe.com/sdks/versioning?server=dotnet)):

- Supports ".NET Framework 4.6.2+" and ".NET 8+".
- LTS releases stay supported for 1 year after EOL; STS releases only while they are current.
- v53.0.0 dropped .NET 6 and added net10.0 ([`CHANGELOG.md`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/CHANGELOG.md) "53.0.0").

**Nullable reference types.** Not enabled project-wide. Only `Events/UnknownEventNotification.cs:16` has `#nullable enable`, and with `LangVersion 8` the public API is effectively nullable-oblivious.

**Analyzers.**

- `StyleCop.Analyzers 1.1.118` with [`src/_stylecop/StyleCopRules.ruleset`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/_stylecop/StyleCopRules.ruleset). It disables file headers (SA1633), regions (SA1124) and documentation rules (SA1600 and others).
- [`.editorconfig`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/.editorconfig) requires `this.` qualification and sets `CA2007` (ConfigureAwait) to **error** for `src/Stripe.net/**`. As a result, every await in the library uses `.ConfigureAwait(false)`.

**Strong naming.** The csproj sets `<SignAssembly>True</SignAssembly>`, but the repo has **no `.snk`** and no `AssemblyOriginatorKeyFile`. [`Properties/InternalsVisibleTo.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Properties/InternalsVisibleTo.cs) also uses friend names without a `PublicKey=`, which would not compile (CS1726) if the assembly were really strong-named. *Inference:* the shipped assembly is not strong-name signed in any useful sense.

## 2. Client construction

**`StripeClient`** ([`Infrastructure/Public/StripeClient.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Infrastructure/Public/StripeClient.cs)):

```csharp
public class StripeClient : IStripeClient {
  public StripeClient(string apiKey = null, string clientId = null, IHttpClient httpClient = null,
                      string apiBase = null, string connectBase = null, string filesBase = null,
                      string meterEventsBase = null);
  public StripeClient(StripeClientOptions options);            // :78
  public static string DefaultApiBase => "https://api.stripe.com";
  public virtual V1Services V1 => this.v1 ??= new V1Services(this.Requestor);   // :126
  public virtual V2Services V2 => this.v2 ??= new V2Services(this.Requestor);
  public T Deserialize<T>(string response) where T : IStripeEntity;              // :136
  public Task<StripeResponse> RawRequestAsync(HttpMethod, string path, string content = null,
                                              RawRequestOptions = null, CancellationToken = default); // :197
  public StripeClient WithStripeContext(StripeContext context);                  // :310
}
```

**`StripeClientOptions`** ([`StripeClientOptions.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Infrastructure/Public/StripeClientOptions.cs)):

- Properties: `ApiKey`, `ClientId`, `HttpClient` (`IHttpClient`), `ApiBase`/`ConnectBase`/`FilesBase`/`MeterEventsBase`, `StripeAccount`, `StripeContext`.
- Its doc comment says: "Changes to this object after creating the StripeClient are ignored." `LiveApiRequestor` clones it (`LiveApiRequestor.cs` constructor, `options?.Clone()`). The client is effectively immutable and safe to share across threads.
- `WithStripeContext` returns a **new** client rather than mutating the existing one.

**Key validation at construction** (`LiveApiRequestor` constructor): an empty key or a key containing whitespace throws `ArgumentException`. A missing key fails later, at request time, with `StripeException` (`StripeRequest.cs` `BuildAuthorizationHeader`).

**Legacy global configuration** ([`StripeConfiguration.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Infrastructure/Public/StripeConfiguration.cs)):

- Static `ApiKey` (falls back to `ConfigurationManager.AppSettings["StripeApiKey"]`), `MaxNetworkRetries`, `EnableTelemetry`, `AppInfo`, and `SerializerOptions` (STJ).
- A lazily built static `StripeClient`. Each setter nulls the cached client so it gets rebuilt.
- `new CustomerService()` with no client falls back to `StripeConfiguration.StripeClient`. `Service.Client` is `[Obsolete]` and `EditorBrowsable(Never)` ([`Services/_base/Service.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Services/_base/Service.cs)).

**The README's recommendation** ([`README.md`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/README.md) "Using StripeClient"):

```csharp
// StripeClient pattern (Recommended)
var client = new StripeClient("sk_test_...");
Customer customer = client.V1.Customers.Get("cus_1234");
// Global Configuration pattern (Legacy)
StripeConfiguration.ApiKey = "sk_test_..."; var service = new CustomerService();
```

The README says accessors were added in v46 "to be the entry point to access all services". `V1Services` is a generated class with a lazy `??=` property per resource (`Services/V1Services.cs:156`, `Customers`). Nested resources hang off their parent: `client.V1.Customers.TaxIds`, from the `CustomerService` properties.

**DI / `IHttpClientFactory`.** No first-party guidance exists. The repo contains no `IServiceCollection` extensions or Microsoft.Extensions dependencies. The only examples construct `new StripeClient(...)` directly (`src/Examples/**`). The wiki ([Advanced client usage](https://github.com/stripe/stripe-dotnet/wiki/Advanced-client-usage)) covers a proxy through `HttpClientHandler`, custom handlers, custom base URLs and `maxNetworkRetries`, but not DI or the factory. What the code allows:

- Pass a factory-built `HttpClient` into `new SystemNetHttpClient(httpClient, maxNetworkRetries, appInfo, enableTelemetry)`, then into `StripeClientOptions.HttpClient`.
- With no client supplied, a **process-wide static `Lazy<HttpClient>`** is shared, with an 80 s timeout (`SystemNetHttpClient.cs:125`, `LazyDefaultHttpClient`). That avoids socket exhaustion but ignores DNS rotation.
- Per-request headers (User-Agent and others) are set on each `HttpRequestMessage`, not on `DefaultRequestHeaders`, "because we want these headers to be present even when a custom HTTP client is used" (`SystemNetHttpClient.cs:161`).

## 3. Services, options, RequestOptions, sync/async, custom methods, codegen

**Service base** ([`Services/_base/Service.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Services/_base/Service.cs)):

- Abstract `Service` with internal `Request<T>`, `RequestAsync<T>`, `RequestStreaming[Async]`, `ListRequestAutoPaging[Async]` and `SearchRequestAutoPaging[Async]`.
- Capabilities are marked by interfaces in `Services/_interfaces/`: `ICreatable<TEntity,TOptions>`, `IRetrievable`, `IUpdatable`, `IDeletable`, `IListable`, `ISearchable`, plus `INested*` versions for sub-resources.

**Generated service** ([`Services/Customers/CustomerService.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Services/Customers/CustomerService.cs)):

```csharp
// File generated from our OpenAPI spec
public partial class CustomerService : Service,
    ICreatable<Customer, CustomerCreateOptions>, IListable<Customer, CustomerListOptions>, ... {
  public CustomerService(IStripeClient client) : base(client) { }
  public virtual Customer Create(CustomerCreateOptions options, RequestOptions requestOptions = null)
    => this.Request<Customer>(BaseAddress.Api, HttpMethod.Post, $"/v1/customers", options, requestOptions);
  public virtual Task<Customer> CreateAsync(CustomerCreateOptions options, RequestOptions requestOptions = null,
                                            CancellationToken cancellationToken = default) => ...;
  public virtual Task<Customer> GetAsync(string id, CustomerGetOptions options = null, ...)
    => this.RequestAsync<Customer>(..., HttpMethod.Get, $"/v1/customers/{WebUtility.UrlEncode(id)}", ...);
  public virtual IAsyncEnumerable<Customer> ListAutoPagingAsync(CustomerListOptions options = null, ...);
}
```

- **Every public method is `virtual`**, which makes the services mockable with Moq.
- Hand-written extras go in `partial` files. Example: `CustomerService.partial.cs` holds `[Obsolete]` legacy methods.
- **Sync is sync-over-async**: `Request<T>` calls `RequestAsync(...).ConfigureAwait(false).GetAwaiter().GetResult()` (`Service.cs`). Only the net462 auto-paging path has a separate `#if NETFRAMEWORK` sync loop.

**Options classes.**

- `BaseOptions` ([`Services/_base/BaseOptions.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Services/_base/BaseOptions.cs)) has `List<string> Expand`, `[JsonExtensionData] IDictionary<string, object> ExtraParams`, `AddExpand`, `AddExtraParam` and an internal `SetTracker`.
- `ListOptions : BaseOptions` adds `long? Limit`, `StartingAfter` and `EndingBefore`.
- Per-operation classes include `CustomerCreateOptions`, `CustomerUpdateOptions`, `CustomerListOptions : ListOptions`, `CustomerGetOptions` and `CustomerDeleteOptions`.
- **SetTracker** (v51, [`Infrastructure/SetTracker.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Infrastructure/SetTracker.cs)): nullable properties record when a setter runs (`this.SetTracker.Track()` via `[CallerMemberName]`). This lets the SDK tell "explicitly set to null, so clear the field" apart from "never touched". It replaced `Emptyable<T>` (CHANGELOG 51.0.0 #3342).

**`RequestOptions`** ([`Services/_common/RequestOptions.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Services/_common/RequestOptions.cs)):

```csharp
public class RequestOptions {
  public string ApiKey { get; set; }            // per-call key override
  public string IdempotencyKey { get; set; }
  public string StripeAccount { get; set; }     // Connect: Stripe-Account header
  public string StripeContext { get; set; }
  public string StripeRequestTrigger { get; set; }
  internal string StripeVersion { get; set; }  internal string InternalBaseUrl { get; set; }
}
```

`WithClientOptions` merges these with the client defaults: a per-call value wins, otherwise the client value is used.

**Custom (non-CRUD) methods** are ordinary generated methods that POST to a sub-path, e.g. `PaymentIntentService.CancelAsync(string id, PaymentIntentCancelOptions options = null, ...)` → `POST /v1/payment_intents/{id}/cancel` (`Services/PaymentIntents/PaymentIntentService.cs:92`). Path ids are escaped with `WebUtility.UrlEncode`. That function does form encoding (a space becomes `+`), not RFC 3986 path encoding.

**Generated vs hand-written.**

- 6,459 of 6,677 `.cs` files begin with `// File generated from our OpenAPI spec`.
- Hand-written files contain generated regions between `// The beginning of the section generated from our OpenAPI spec` and its end marker. This convention is documented in [`CONTRIBUTING.md`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/CONTRIBUTING.md) "Generated code".
- The generator is private (`import? '../sdk-codegen/utils.just'` in `justfile`).

## 4. Pagination

- **V1 `StripeList<T>`** ([`Entities/StripeList.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Entities/StripeList.cs)): `Object`, `List<T> Data`, `bool HasMore` and `Url`. It implements `IEnumerable<T>`, so you can `foreach` over a single page.
- **V2 `Stripe.V2.StripeList<T>`** ([`Entities/V2/StripeList.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Entities/V2/StripeList.cs)): `Data` and `NextPageUrl`.
- **Auto-paging** (`Service.cs`, `V1ListRequestAutoPagingAsync` / `V2ListRequestAutoPagingAsync`):
  - V1 clones the options, sets `StartingAfter = lastItem.Id` (or `EndingBefore` when paging backwards, reversing each page), and stops when `!HasMore`.
  - V2 follows `NextPageUrl` with empty options, because the URL already carries the cursor.
  - Both are `async IAsyncEnumerable<T>` with `[EnumeratorCancellation]` and call `ThrowIfCancellationRequested()` per page and per item.
  - `null` items (unknown polymorphic types) are skipped.
- **Sync** `ListAutoPaging` wraps the async enumerator with `AsyncUtils.ToEnumerable` (`Infrastructure/AsyncUtils.cs`).
- **Search** pages use `StripeSearchResult<T>` with `NextPage` tokens.

```csharp
await foreach (var c in client.V1.Customers.ListAutoPagingAsync(new CustomerListOptions { Limit = 100 }, null, ct)) { ... }
```

## 5. Errors

- **`StripeException : Exception`** ([`Infrastructure/Public/StripeException.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Infrastructure/Public/StripeException.cs)) has settable `HttpStatusCode`, `StripeError` and `StripeResponse` properties.
  - `Message` is overridden to prefix `[{RequestId}]` when the response carried a `Request-Id` header.
  - V1 has **no exception subclasses**. Callers branch on `e.StripeError.Type` / `.Code`.
- **`StripeError`** ([`Entities/StripeError.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Entities/StripeError.cs)) is the parsed `{"error": {...}}` envelope: `Type`, `Code`, `DeclineCode`, `Message`, `Param`, `DocUrl`, `RequestLogUrl`, `PaymentIntent` and others.
- **`StripeResponse`** ([`StripeResponseBase.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Infrastructure/Public/StripeResponseBase.cs)) exposes `StatusCode`, `Headers`, `Date`, `IdempotencyKey`, `RequestId` and string `Content`. Every successful entity also carries `.StripeResponse`, set in `LiveApiRequestor.ProcessResponse`.
- **Mapping** (`LiveApiRequestor.cs:182` `BuildStripeException`, `:223` `BuildV2StripeException`):
  - **Any status other than 200 is an error** (`response.StatusCode != HttpStatusCode.OK`, `:327`). 201 and 204 are not treated as success.
  - The body is parsed with `JsonDocument`. If there is no `error` key, or the JSON is invalid, the result is "Invalid response object from API".
  - A string `error` means an OAuth error.
  - V2 switches on `error.type` and calls a generated `StripeException.ParseV2Exception(type, ...)`, which builds `Stripe.V2.RateLimitException`, `NotCancelableException`, `AlreadyCanceledException` and so on. Unknown types fall back to the plain exception.
- Configuration errors (missing key, bad webhook secret) also throw `StripeException`. Invalid arguments throw `ArgumentException`.

## 6. Retries, idempotency, and the HTTP seam

**`SystemNetHttpClient`** ([`Infrastructure/Public/SystemNetHttpClient.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Infrastructure/Public/SystemNetHttpClient.cs)):

| Item | Value / behaviour | Line |
|---|---|---|
| `DefaultMaxNumberRetries` | `2` (default since #2006) | :25 |
| `DefaultHttpTimeout` | 80 s | :125 |
| `MinNetworkRetriesDelay` / `MaxNetworkRetriesDelay` | 500 ms / 5 s | :135 / :130 |
| Backoff | `min * 2^(n-1)`, capped at max, × jitter `(3 + rand)/4` (75–100%), floored at min | :451–:481 |
| Retry triggers | `HttpRequestException`; `OperationCanceledException` *when the caller didn't cancel* (i.e. HttpClient timeout); `Stripe-Should-Retry: true/false` header **overrides** status; 409; ≥500 | :378–:427 |
| Not handled | `Retry-After`; 429 (only retried if header says true); `Task.Delay(SleepTime(retry))` **without** the `CancellationToken` | :277 |
| Test knob | `internal bool NetworkRetriesSleep` (tests set false) | |

**Automatic idempotency** (`StripeRequest.cs:216–259`):

- If `RequestOptions.IdempotencyKey` is empty and the method is POST (or DELETE on V2), the SDK adds `Idempotency-Key: Guid.NewGuid()`.
- The headers dictionary is built once per `StripeRequest`, and `BuildRequestMessage` copies it onto a new `HttpRequestMessage` for each attempt. So **every retry reuses the same key**.
- `StripeRequest.Content` "creates a new instance every time it is called" (`:107`), because `HttpContent` can't be re-sent.
- The README states: "Idempotency keys are always added to requests to make any such subsequent retries safe."
- Stripe docs ([error-low-level](https://docs.stripe.com/error-low-level)): keys are kept "at least 24 hours". `Idempotent-Replayed: true` marks a replayed response. `Stripe-Should-Retry` absent means "Use the status code…".

**Seams:**

```csharp
public interface IHttpClient {                         // IHttpClient.cs
  Task<StripeResponse> MakeRequestAsync(StripeRequest request, CancellationToken ct = default);
  Task<StripeStreamedResponse> MakeStreamingRequestAsync(StripeRequest request, CancellationToken ct = default);
}
public interface IStripeClient { /* ApiBase, ApiKey, ..., RequestAsync<T>, RequestStreamingAsync */ }
```

- `IHttpClient` is the public test and injection point: it receives a fully built `StripeRequest` (URI, auth header, Stripe headers, content).
- Request construction (`LiveApiRequestor`) sits above it. Retries and telemetry sit inside it, in `SystemNetHttpClient`. So replacing `IHttpClient` also drops retries unless you wrap `SystemNetHttpClient`. The tests instead mock the inner `HttpClientHandler` (§11).
- `ApiRequestor` is internal and abstract. `ApiRequestorAdapter` wraps a user-supplied `IStripeClient`.
- `StripeRequest.ValidatePath` (`:160`) rejects paths that don't start with exactly one `/`. Its comment explains that a path like `@evil.example/...` would otherwise send the API key to another host. This matters because some paths come from webhook bodies (`next_page_url`, related-object URLs).

## 7. User-Agent, telemetry headers, AppInfo, logging

- **`User-Agent`** (`SystemNetHttpClient.cs:360`): `Stripe/{v1|v2} .NetBindings/{version}`, plus `AppInfo.FormatUserAgent()` (`Name/Version (Url)`), plus ` AIAgent/{slug}` when an env var such as `CLAUDECODE` or `CURSOR_AGENT` is detected (`AIAgents` table, `:61`, generated).
- **`X-Stripe-Client-User-Agent`** (`:293`): JSON with `bindings_version`, `lang: ".net"`, `stripe_net_target_framework` (chosen by `#if NET8_0`…), `lang_version`, `platform` (OS + architecture, telemetry only), `newtonsoft_json_version`, `application` (AppInfo), `ai_agent`, and `telemetry_id`. Each probe is wrapped in try/catch (comment cites issue #1986).
- **`X-Stripe-Client-Telemetry`** ([`RequestTelemetry.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Infrastructure/Public/RequestTelemetry.cs)): after each response with a `Request-Id`, `{request_id, request_duration_ms, usage}` goes onto a `ConcurrentQueue` capped at 100 entries. The next request dequeues one item and sends it.
- **`TelemetryId`** ([`Infrastructure/TelemetryId.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Infrastructure/TelemetryId.cs)): a persisted id stored in a per-user config directory. Telemetry is on by default and can be disabled with `StripeConfiguration.EnableTelemetry = false` or the `SystemNetHttpClient` constructor argument.
- **`AppInfo`** ([`AppInfo.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Infrastructure/Public/AppInfo.cs)): `Name` (required), `Version`, `Url`, `PartnerId`, intended "for plugins".
- **Logging: none.** There is no `ILogger`, `EventSource` or `ActivitySource`. The only output is `Console.Error.WriteLine` of the `Stripe-Notice` response header (`LiveApiRequestor.cs:254–259`), which `STRIPE_SUPPRESS_NOTICES=true` silences.

## 8. Webhooks

**Snapshot events.** [`Services/Events/EventUtility.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Services/Events/EventUtility.cs):

```csharp
public const int DefaultTimeTolerance = 300;                                               // :24
public static Event ConstructEvent(string json, string stripeSignatureHeader, string secret,
    long tolerance = DefaultTimeTolerance, bool throwOnApiVersionMismatch = true);        // :122
public static void ValidateSignature(string json, string header, string secret, long tolerance, long utcNow); // :190
public static string ComputeSignature(string secret, string timestamp, string payload);   // :415
public static string GenerateSignatureHeader(string payload, string secret, long? timestamp = null); // :431 (for tests)
public static Event ConstructEventWithoutVerification(string json, bool throwOnApiVersionMismatch = false); // :381
```

- **Header scheme:** `t=<unix>,v1=<hex>[,v1=...][,v0=...][,secret=...,destination=...,payload=...]`. It is parsed into an `ILookup`, so repeated `v1` entries are allowed (secret rolling). The docs say an integration "must accept unknown entries and must not depend on a fixed entry count or order" ([manage-webhook-endpoints](https://docs.stripe.com/webhooks/signature)).
- **Signature:** `HMACSHA256(secretUtf8, "{t}.{payload}")` → lowercase hex, matched against any `v1` with `StringUtils.SecureEquals` (constant time, `:331`). UTF-8 encoding is strict (`throwOnInvalidBytes: true`).
- **Rejections:** empty or whitespace-only secrets (53.1.0 #3477). Timestamps where `|utcNow − t| > tolerance`.
- **Failure messages:** debug metadata in the header is used to explain what went wrong (wrong secret length or suffix, body hash mismatch) without being trusted for verification.
- **API-version gate:** `throwOnApiVersionMismatch` compares release trains (`2026-10-28.endive` → `endive`) via `IsCompatibleApiVersion` (`:26`).
- **Wrong-method guards:** passing a thin event to `ConstructEvent`, or the reverse, throws `ArgumentException` (v51 #3338).
- `MaybeExtractFromCloudProviderEnvelope` unwraps AWS EventBridge (`detail`) and Azure Event Grid (`specversion` + `data`) envelopes.

**Thin events (V2).**

- `client.ParseEventNotification(json, sigHeader, secret, tolerance)` (`StripeClient.cs:224`) returns a `V2.Core.EventNotification` with `Id`, `Type`, `Created`, `Livemode`, `Reason` and `Context`.
- Generated subclasses, e.g. `Events/V1BillingMeterErrorReportTriggeredEventNotification.cs`, expose `RelatedObject`, `FetchRelatedObjectAsync()` and `FetchEventAsync()`. The full data is fetched on demand through the client, which is scoped to the event's context.
- `client.NotificationHandler(secret, fallback)` returns a `StripeEventNotificationHandler`. It exposes .NET `event` fields per type plus `PreHandle` with `e.Cancel` for de-duplication ([`src/Examples/V2/EventNotificationHandlerEndpoint.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Examples/V2/EventNotificationHandlerEndpoint.cs)). The base class, `StripeEventNotificationHandlerBase.cs`, is 3,820 generated lines.

## 9. Raw requests, file upload, binary download

**Raw requests** (`LiveApiRequestor.cs:406`).

- `RawRequestAsync(HttpMethod, path, string content = null, RawRequestOptions)` sends a body only for POST (otherwise it throws `InvalidOperationException`).
- `RawRequestOptions : RequestOptions` adds `BaseUrl` and `Dictionary<string,string> AdditionalHeaders`.
- It returns a `StripeResponse` (raw string). `client.Deserialize<T>(response.Content)` produces a typed object.
- Usage gets tagged `raw_request` for telemetry.

**File upload.**

- `FileCreateOptions.File` is a `MultipartFileContent { Stream Data; string Name; string Type; }` ([`Services/_common/MultipartFileContent.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Services/_common/MultipartFileContent.cs)).
- `ContentEncoder.CreateHttpContent` flattens the options. If every value is a string the body is `application/x-www-form-urlencoded`; otherwise it switches to `multipart/form-data` (`Infrastructure/FormEncoding/ContentEncoder.cs:~63`).
- Uploads go to `BaseAddress.Files` (`files.stripe.com`).
- *Inference:* because content is rebuilt on each retry from the same `Stream`, a retried upload will read from the stream's current position unless the stream is rewound. Treat this as a hazard to design around.

**Binary download.**

- Generated methods return `Stream`, e.g. `QuoteService.PdfAsync(id, ...) : Task<Stream>` → `RequestStreamingAsync(BaseAddress.Files, GET, "/v1/quotes/{id}/pdf")` (`Services/Quotes/QuoteService.cs:172`).
- `LiveApiRequestor.RequestStreamingAsync` (`:144`) returns `response.Body` on 200. On any other status it reads the body and throws the normal `StripeException`.
- Two caveats:
  1. The headers (Content-Type, Content-Disposition, ETag) are lost; the caller gets a bare `Stream`.
  2. `SystemNetHttpClient` calls `httpClient.SendAsync(request, ct)` without `HttpCompletionOption.ResponseHeadersRead` (the repo contains no `HttpCompletionOption` at all). With the default `ResponseContentRead`, the whole PDF is buffered in memory before the `Stream` is returned.

## 10. JSON (de)serialization

- **Library.** STJ has been the default since v51 (CHANGELOG #3327): "System.Text.Json replaces Newtonsoft Json.NET as the default JSON library…". `StripeConfiguration.SerializerOptions = DefaultStjSerializerOptions()`: case-sensitive, `MaxDepth=128`, no comments, no trailing commas. Newtonsoft `SerializerSettings` and `RawJObject` are `[Obsolete]`.
  - Every property carries **both** `[JsonProperty("x")]` and `[STJS.JsonPropertyName("x")]`. The Wholesome test `NewtonsoftAndSystemTextJsonOutputTheSameObject.cs` keeps the two in sync.
  - It is all reflection-based: converter factories call `Activator.CreateInstance(typeof(X<>).MakeGenericType(...))`. There is **no source-generated `JsonSerializerContext`**, so it is not trim- or AOT-friendly.
- **`StripeEntity`** ([`Entities/_base/StripeEntity.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/Stripe.net/Entities/_base/StripeEntity.cs)): `[STJS.JsonConverter(typeof(STJStripeEntityConverter))]`.
  - The converter reads the token into a raw JSON string, then parses it again into the object, keeping the string for lazy `RawJsonElement` / `RawJObject` (`STJStripeEntityConverter.cs`). That costs two parses per entity.
  - Other members: `StripeResponse`, `ToJson()` and `FromJson<T>(string)`.
- **Unknown properties.** `STJDefaultConverter` sends them to a `[JsonExtensionData]` dictionary if the type has one, otherwise skips them (`STJDefaultConverter.cs:~108–118`). Entities have no extension-data member, so unknown response fields are reachable only through `RawJsonElement`. Request-side `BaseOptions.ExtraParams` *is* `[JsonExtensionData]`.
- **Unknown enum values.** Response enums are plain `string` properties whose doc comment notes "This enum can grow over time" (`Entities/PaymentIntents/PaymentIntent.cs:739`, `Status`). Nothing breaks on new values. `StringEnum` (a class wrapping a string with static instances) is used only for request-side literal unions, and its STJ converter is **write-only** (`STJStringEnumConverterFactory.Read` throws).
- **Polymorphism.** Interface-typed fields (`IPaymentSource`, list items) dispatch on the `"object"` discriminator through the generated `StripeTypeRegistry.ObjectsToTypes`. An unknown `object` returns `null`, and auto-paging skips nulls (`STJStripeObjectConverter.cs`). V2 events dispatch on `"type"` (`STJV2EventConverter`).
- **Expandable fields** (`Entities/Customers/Customer.cs:95–136`). Each expandable is three members:
  - public `string DefaultSourceId`,
  - public `IPaymentSource DefaultSource` (the expanded object),
  - an internal `ExpandableField<T> InternalDefaultSource` with `STJExpandableFieldConverter<T>`, which accepts either a string id or an object.
- **Other converters:** Unix-epoch `DateTime` (`STJUnixDateTimeConverter`), decimal-as-string (`DecimalStringConverter`), int64-as-string for V2 (`Int64StringConverter`), `AnyOf`, discriminated unions (`STJDiscriminatedUnionConverterFactory`), and a dictionary converter that preserves nulls.
- **Request encoding:** V1 uses form encoding with indexed arrays. V2 uses JSON (`JsonEncodedContent`), and `SetTracker` decides whether an explicit `null` gets written.

## 11. Testing, CI, release, codegen, README

**Tests** ([`src/StripeTests/StripeTests.csproj`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/StripeTests/StripeTests.csproj)):

- Stack: xUnit 2.4.1, Moq 4.14.7, coverlet. Targets `net8.0;net9.0;net10.0;net462`. `InternalsVisibleTo("StripeTests")`.
- **stripe-mock** ([`StripeMockFixture.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/StripeTests/StripeMockFixture.cs)): an OpenAPI-driven mock server, minimum version `0.119.0`.
  - It is either started by `StripeMockHandler` or found through `STRIPE_MOCK_PORT` (default `12111`).
  - Clients are built with `ApiKey="sk_test_123"` and a `SystemNetHttpClient` wrapping a forwarding handler.
  - CI resolves the spec and fixtures from `OPENAPI_VERSION`, or from canary channels ([`.github/actions/test/action.yml`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/.github/actions/test/action.yml)).
- **Unit HTTP mocking** ([`MockHttpClientFixture.cs`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/src/StripeTests/MockHttpClientFixture.cs)): `Mock<HttpClientHandler>{CallBase=true}` wrapped in a real `SystemNetHttpClient`, then `MockHandler.Protected().Verify("SendAsync", ...)` for request assertions. Real retry logic runs against fake transports.
- **Functional:** `Functional/NetworkRetriesTest.cs` and `TelemetryTest.cs`.
- **Wholesome** (`Wholesome/README.md`: "checks on the state of the Stripe.net codebase … rely heavily on reflection"). Examples: `JsonNamesAreSnakeCase`, `PropertiesHaveJsonAttributes`, `UseListsInsteadOfArrays`, `NullableValueTypes`, `DontForgetIHasInterfaces`.

**CI** ([`.github/workflows/ci.yml`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/.github/workflows/ci.yml)):

1. A `prepare` job derives the test matrix from the csproj TFMs (`justfile` `print_test_matrix_json`).
2. Tests run per TFM on Ubuntu; net462 runs on `windows-latest`.
3. `build`, then `build-examples`.
4. A `compat` job on PRs runs `dotnet pack -p:RunBaselineCheck=true` (API compatibility against the current version).
5. On a tag `v*`, CI packs, then publishes to nuget.org through **`NuGet/login` OIDC** (`id-token: write`, `environment: release`) with no long-lived key.

All actions are pinned by SHA, the workflow declares `permissions: {}`, and zizmor runs as a workflow linter.

**Versioning and release.**

- Semver, with "Major … breaking changes"; each SDK version pins the API version that was current at release ([docs](https://docs.stripe.com/sdks/versioning)).
- `Constants/ApiVersion.cs` holds `Current = "2026-10-28.endive"` and is sent as `Stripe-Version` on every request (`StripeRequest.cs:223`).
- Preview builds use `-beta.N` / `-alpha.N` suffixes.
- `CHANGELOG.md` is "a generated file … edit a corresponding `.change.md` file and run `hark build`". Fragments live in `.hark/changes/`, and migration guides in `.hark/migration-guides` plus the wiki.
- Version bumps touch `VERSION`, the csproj `<Version>` and `Constants/Version.cs` (`Makefile` `update-version`).

**README** ([`README.md`](https://github.com/stripe/stripe-dotnet/blob/010e2e74da1095731c239f610c299574466eb139/README.md)) sections, in order:

1. Installation, Requirements, Documentation.
2. Usage: `StripeClient`, Authentication, CRUD examples, Listing, Auto-pagination, Per-request configuration.
3. Custom `HttpClient`, Automatic retries, Undocumented params/properties (`AddExtraParam`, `RawJObject`).
4. Writing a plugin (`AppInfo`), Telemetry opt-out, Public/Private preview SDKs, Custom requests (`RawRequestAsync`).
5. Support, Development (`just test`, `just format`).

---

## Adopt / Adapt / Skip for teachmehipaa-dotnet

| # | Stripe pattern (evidence) | Verdict | What we do for the TeachMeHIPAA API |
|---|---|---|---|
| 1 | Static secret key → `Authorization: Bearer` built per request (`StripeRequest.cs:213`) | **Adapt** | Use an `ITokenProvider` seam. **Client credentials:** cache the 15-minute JWT and refresh it 60 s before `exp`. Use **single-flight via `SemaphoreSlim(1,1)`** with a double-checked expiry test so concurrent callers share one token fetch. On a 401, invalidate and retry once. The provider's HTTP client must not run the auth handler (no recursion). **Auth code + PKCE:** refresh tokens rotate, so serialise refreshes behind the same semaphore and persist the new pair through a caller-supplied `ITokenStore.SaveAsync` *before* using it. Losing a rotated refresh token logs the user out. The token is never placed in `RequestOptions`, logs or exception messages. |
| 2 | `StripeClient` immutable after construction; options cloned (`StripeClientOptions.cs` doc; `LiveApiRequestor` ctor) | **Adopt** | `TeachMeHipaaClient(TeachMeHipaaClientOptions)` is thread-safe and meant to be a singleton. Use `init`-only options plus a `With…()` copy method (as `WithStripeContext` does) for a per-organization scope. |
| 3 | `client.V1.Customers` lazy accessors; nested `Customers.TaxIds` (`V1Services.cs:156`) | **Adopt** | `client.Organizations`, `client.Organizations.Enrollments`, `client.Certificates`, … No `V1` prefix unless we expect a v2. Accessors are `virtual` (or interfaces) so callers can mock them. |
| 4 | Global `StripeConfiguration` + `new XService()` fallback, `ConfigurationManager` lookup (`StripeConfiguration.cs`) | **Skip** | No static mutable state. Stripe labels this "Legacy" itself. |
| 5 | Per-op options classes + `RequestOptions{ApiKey, IdempotencyKey, StripeAccount}` (`RequestOptions.cs`) | **Adopt/Adapt** | `EnrollmentCreateOptions`, `EnrollmentUpdateOptions`, `EnrollmentListOptions : ListOptions`. `RequestOptions` carries `IdempotencyKey`, `IfMatch` / `IfNoneMatch` (ETag), `AdditionalHeaders`, `Timeout` and `CancellationToken` (the token goes as a method parameter, as Stripe does). No per-call credential override; scope a client instead. |
| 6 | Sync + async pairs; sync = `GetAwaiter().GetResult()` (`Service.cs`) | **Skip sync** | Async-only (`…Async(…, CancellationToken)`). Sync-over-async risks deadlocks and doubles the API surface. Revisit only if a .NET Framework customer asks. |
| 7 | AIP-136 custom methods = generated methods on a sub-path, `WebUtility.UrlEncode(id)` (`PaymentIntentService.cs:92`) | **Adapt** | `client.Organizations.Enrollments.CancelAsync(orgId, id, EnrollmentCancelOptions?)` → `POST /organizations/{Uri.EscapeDataString(orgId)}/enrollments/{Uri.EscapeDataString(id)}:cancel`. Use **RFC 3986 escaping (`Uri.EscapeDataString`), not `WebUtility.UrlEncode`**, and keep the `:verb` suffix literal. Auto-attach an Idempotency-Key, since these are POSTs. |
| 8 | Auto `Idempotency-Key: Guid` on POST, reused across retries (`StripeRequest.cs:241–248`) | **Adopt** | The same pattern applies to POST and AIP-136 custom methods, and PATCH if our API honours it. Build the key once per logical call; the retry loop reuses it. Document "persist your own key across process restarts" (Stripe docs say the same). |
| 9 | Retry policy: 2 retries, 0.5 s→5 s exponential, 75–100% jitter, 409/5xx/network, `Stripe-Should-Retry` (`SystemNetHttpClient.cs:378–481`) | **Adapt** | Keep the constants and jitter. **Add:** honour `Retry-After` (seconds or HTTP-date, capped), retry 429, retry 503/502/504; pass the `CancellationToken` into `Task.Delay` (Stripe doesn't, `:277`). Retry non-idempotent methods only when an Idempotency-Key is present. Rewind or buffer upload streams before a retry. Expose `MaxRetries` in options and a test knob for skipping the sleep. |
| 10 | `IHttpClient` seam that owns retries; static shared `HttpClient` default (`IHttpClient.cs`, `:LazyDefaultHttpClient`) | **Adapt** | Accept a plain `HttpClient` (or `HttpMessageHandler`). Implement auth, retry and telemetry as **`DelegatingHandler`s** in our pipeline, so `IHttpClientFactory` and Polly users fit naturally. Ship `TeachMeHipaa.Extensions.DependencyInjection` with `services.AddTeachMeHipaa(o => …)` using `AddHttpClient`, since Stripe has no DI guidance. Default to a shared `SocketsHttpHandler` with `PooledConnectionLifetime` (handles DNS rotation; netstandard2.0 falls back to `HttpClientHandler`). |
| 11 | Error envelope `{error:{type,code,message,param}}`, single `StripeException`, V2 typed subclasses by `error.type` (`LiveApiRequestor.cs:182–250`; `StripeException.cs`) | **Adapt** | Parse **RFC 9457 `application/problem+json`** into a `ProblemDetails` record (`Type`, `Title`, `Status`, `Detail`, `Instance`, `Extensions` as `JsonElement` dictionary). Throw `TeachMeHipaaApiException(StatusCode, Problem, Response)` and map well-known `type` URIs to subclasses through a generated switch (`ValidationException` with `errors[]`, `NotFoundException`, `ConflictException`, `PreconditionFailedException` for 412 ETag, `RateLimitException` with `RetryAfter`, `AuthenticationException`). Fall back to the base type when the body isn't JSON. Prefix `Message` with the request id as Stripe does (`StripeException.Message`). Treat **all 2xx as success**, not only 200 (Stripe's 200-only check would break our 201/202/204). |
| 12 | `StripeResponse` on every entity (headers, `RequestId`, `IdempotencyKey`) (`StripeResponseBase.cs`) | **Adopt** | `ApiResponse` with `StatusCode`, `Headers`, `RequestId`, `ETag`, `IdempotentReplayed`, attached to models through a non-serialized property. Also a `…WithResponseAsync` variant returning `Response<T>` so callers can read the ETag for the next `If-Match`. |
| 13 | `StripeList<T>` + `ListAutoPagingAsync` → `IAsyncEnumerable<T>` with `[EnumeratorCancellation]` (`Service.cs`) | **Adopt (V2 style)** | `Page<T>{Data, NextCursor}` (IEnumerable) and `ListAsync(opts)`. `ListAutoPagingAsync(opts, ct)` loops on our cursor (like V2 `next_page_url`, not V1's last-id), plus `.AsPagesAsync()` for page-level access. If the server returns a next *URL*, run it through Stripe's `ValidatePath` defence. |
| 14 | `ValidatePath` host-injection guard (`StripeRequest.cs:160`) | **Adopt** | The same check covers raw requests and any server-supplied relative URL (LRO `operation.name`, pagination links). |
| 15 | No ETag support | **Add** | `RequestOptions.IfMatch` / `IfNoneMatch`. `Response.ETag`. A 412 maps to `PreconditionFailedException`; a 304 maps to a "not modified" result on GETs that opt in. |
| 16 | No LRO concept | **Add** | AIP-151: `Operation<T>` (`Name`, `Done`, `Metadata`, `Result` / `Error`) with `UpdateStatusAsync()` and `WaitForCompletionAsync(pollInterval?, ct)` that respects `Retry-After`. Shape it after Azure.Core's `Operation<T>`; Stripe has no precedent. |
| 17 | Webhooks: HMAC-SHA256 of `"{t}.{body}"`, 300 s tolerance, multiple `v1`, constant-time compare, test helper `GenerateSignatureHeader`, wrong-method guards (`EventUtility.cs`) | **Adopt (scheme per our spec)** | Write `WebhookUtility.ConstructEvent(string body, string signatureHeader, string secret, TimeSpan? tolerance = 5 min, TimeProvider? clock)` and `ValidateSignature`. Use `CryptographicOperations.FixedTimeEquals` (net8+) or a polyfill. Accept unknown header entries and multiple signatures (secret rotation). Ship a `GenerateSignatureHeader` test helper. Throw a dedicated `WebhookSignatureException`, not the API exception. Keep `throwOnApiVersionMismatch` only if events carry an API version. Skip the cloud-envelope unwrapping. |
| 18 | Thin events + generated typed handler with .NET `event`s (`StripeEventNotificationHandlerBase.cs`, 3,820 lines) | **Skip for v1** | Use a typed `Event` model with a discriminated `Data` union (STJ `[JsonPolymorphic]`) and an `UnknownEvent` fallback. A handler/dispatcher can come later if demand appears. |
| 19 | `RawRequestAsync` + `Deserialize<T>` + `AddExtraParam` (`StripeClient.cs:197`, `BaseOptions.cs`) | **Adopt** | `client.SendRawAsync(HttpMethod, path, HttpContent?, RequestOptions?, ct)` → `ApiResponse` with the body as a stream or string, plus `client.Deserialize<T>()`. `Options.ExtraParams` uses `[JsonExtensionData]`. |
| 20 | Upload `MultipartFileContent{Stream}`, auto multipart (`ContentEncoder.cs`) | **Adapt** | Only if our API takes uploads: a `FileUpload(Stream, fileName, contentType)` type, with a seekable-stream check for retries. |
| 21 | PDF download returns a bare `Stream`, fully buffered (no `ResponseHeadersRead`) (`LiveApiRequestor.cs:144`, `QuoteService.cs:172`) | **Adapt** | `DownloadCertificatePdfAsync(...)` → `BinaryResponse : IAsyncDisposable` holding `Stream Content`, `ContentType`, `FileName` (from Content-Disposition), `ContentLength` and `ETag`. Send with **`HttpCompletionOption.ResponseHeadersRead`** to really stream. Errors are still parsed as problem+json. Request `Accept: application/pdf, application/problem+json`. |
| 22 | Newtonsoft + STJ dual attributes; STJ default since v51 (`CHANGELOG` #3327) | **STJ only** | A new SDK in 2026 should depend only on System.Text.Json; Stripe kept Newtonsoft purely for back-compat. Use **source-generated `JsonSerializerContext`** (trim/AOT-safe; Stripe's reflection plus `Activator` converters are not). `JsonNamingPolicy.SnakeCaseLower` or explicit `[JsonPropertyName]`. On netstandard2.0, reference the current `System.Text.Json` package (Stripe pins an old 6.0.10). |
| 23 | Entities keep raw JSON (double parse) for `RawJsonElement` (`STJStripeEntityConverter.cs`) | **Adapt** | Use `[JsonExtensionData] IDictionary<string, JsonElement>? AdditionalProperties` on models. Unknown fields stay reachable at a single parse, and callers can read new API fields before an SDK release. |
| 24 | Response enums as plain `string` ("can grow over time"); `StringEnum` write-only (`PaymentIntent.cs:739`, `StringEnum.cs`) | **Adapt** | Extensible enums: `readonly struct EnrollmentStatus : IEquatable<…>` with static known values, implicit conversion from string, and a read/write converter. Unknown values round-trip instead of throwing. This beats both bare strings and C# `enum`. |
| 25 | Polymorphism via `"object"` registry; unknown → `null` (`STJStripeObjectConverter.cs`) | **Adapt** | Use STJ `[JsonPolymorphic(TypeDiscriminatorPropertyName = …)]` mirroring the OpenAPI `discriminator`, with `UnknownDerivedTypeHandling = FallBackToBaseType` (an `Unknown*` base keeps the raw data). Never silently produce `null`. |
| 26 | `ExpandableField<T>` triple-property pattern (`Customer.cs:95–136`) | **Skip** | Only relevant if our API supports `expand`. |
| 27 | `SetTracker` to tell an explicit null from an unset field in PATCH (`SetTracker.cs`) | **Adopt (if PATCH merge semantics)** | For `.partial()`-style PATCH bodies, track which properties were set so `null` means "clear" and absent means "leave unchanged". Alternative: an `Optional<T>` wrapper. |
| 28 | Target frameworks `net8;net9;net10;netstandard2.0;net462` (`Stripe.net.csproj`) | **Adapt** | **`net10.0;net8.0;netstandard2.0`**. netstandard2.0 covers .NET Framework 4.6.2+ and older hosts. Skip STS `net9.0` (Stripe's docs table lists its EOL as Nov 2026) and the separate `net462` target unless netstandard2.0 shows a gap. Follow Stripe's LTS+1-year policy for dropping TFMs. Add `Microsoft.Bcl.AsyncInterfaces` and polyfills (e.g. `TimeProvider` via `Microsoft.Bcl.TimeProvider`) for netstandard2.0. |
| 29 | `LangVersion 8`, nullable off (`csproj`) | **Skip → enable** | Use the latest `LangVersion` with `<Nullable>enable</Nullable>`, so the public API carries correct nullability annotations. |
| 30 | StyleCop + `TreatWarningsAsErrors` + `CA2007` ConfigureAwait as error (`.editorconfig`) | **Adopt** | Same, plus `AnalysisLevel=latest-recommended`, `EnablePackageValidation` with a baseline compat check in PR CI (Stripe's `compat` job), SourceLink, embedded PDB, deterministic CI builds, `GenerateDocumentationFile`. |
| 31 | `SignAssembly=True` with no key (not effectively signed) | **Adapt** | Either strong-name with a key checked into the repo (Microsoft's open-source guidance; harmless on .NET Core, helps .NET Framework consumers) or omit it deliberately. Don't copy Stripe's half state. |
| 32 | `User-Agent` + JSON `X-Stripe-Client-User-Agent`, `AppInfo` (`SystemNetHttpClient.cs:293,360`) | **Adopt** | `User-Agent: teachmehipaa-dotnet/{ver} ({tfm}; {runtime}) [App/Ver]` plus `X-TMH-Client-User-Agent` JSON. `AppInfo` lets integrators identify themselves. |
| 33 | `X-Stripe-Client-Telemetry` latency piggyback, persisted `telemetry_id` on disk, AI-agent env sniffing (`RequestTelemetry.cs`, `TelemetryId.cs`, `:61`) | **Skip** | In a HIPAA context, don't persist machine identifiers or send usage telemetry by default. If we ever want latency data, make it opt-in. |
| 34 | No logging; `Console.Error` for `Stripe-Notice` (`LiveApiRequestor.cs:259`) | **Adapt** | `ActivitySource("TeachMeHipaa")` (built-in, no dependency) for request spans, plus optional `ILogger` in the DI package. **Redact `Authorization`, tokens and request/response bodies by default (PHI).** Never write to the console. |
| 35 | Live vs test via key prefix (`sk_live_` / `sk_test_`); base-URL overrides (`StripeClientOptions.ApiBase`) | **Adapt** | `TeachMeHipaaEnvironment.Live` / `.Sandbox` (an enum or well-known `Uri`s) on options, plus a raw `BaseUri` override for local mocks. Optionally guard against using sandbox credentials with the live URL. |
| 36 | Codegen: private generator, `// File generated…` headers, generated regions in hand-written files, `partial` classes (`CONTRIBUTING.md`) | **Adopt convention** | Hand-write the core (transport, auth, errors, paging, LRO, webhooks). Generate models, options and services from our OpenAPI 3.1 spec with the same header and region markers, and use `partial` classes for hand-written additions. Pin `OPENAPI_VERSION` / spec hash in the repo as Stripe does. |
| 37 | xUnit + Moq-on-`HttpClientHandler` + stripe-mock + reflection "Wholesome" tests (`MockHttpClientFixture.cs`, `Wholesome/`) | **Adapt** | xUnit with a fake `HttpMessageHandler` (typed fake, no Moq needed) for unit tests. An OpenAPI-driven mock (Prism, or WireMock.Net with recorded fixtures) for contract tests. Adopt Wholesome-style reflection tests (every public model property has `JsonPropertyName`, no arrays in public API, nullable value types for optional fields). Retry tests run with the delay disabled. |
| 38 | CI matrix per TFM, net462 on Windows, OIDC trusted publishing via `NuGet/login`, SHA-pinned actions, `permissions: {}` (`ci.yml`) | **Adopt** | Same: matrix per TFM, compat check on PRs, tag-triggered pack and publish through OIDC with a protected `release` environment, zizmor or an equivalent workflow linter. |
| 39 | Semver; API version pinned per SDK release; `hark` changelog fragments; `-beta.N` previews; wiki migration guides | **Adapt** | Semver, with the server API version recorded in the package. Changelog from fragments (Changesets-style or release-please, since hark is Stripe-internal). `-beta.N` for preview endpoints. A `MIGRATING.md` for each major. |
| 40 | README structure (Install → Usage/CRUD → Pagination → Per-request options → HttpClient → Retries → Undocumented params → Plugins → Telemetry → Raw requests) | **Adopt** | Same skeleton plus sections for Authentication (client credentials and PKCE/token store), Errors (problem+json), ETags, Long-running operations, Webhooks, and Downloading certificates. |
