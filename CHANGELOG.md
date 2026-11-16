# Changelog

All notable changes to this library are documented here. It follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and
[semantic versioning](https://semver.org/).

## [Unreleased]

The first release, to ship as 1.0.0 when the Developer API launches.
Generated from the Developer API document release `trimmed-2026-11-15`.

### Added

- `TeachMeHipaaClient`, covering all 44 Developer API operations: the token
  endpoint through its built-in authentication, and the rest as async
  services and models generated from the shared SDK IR: Learners,
  Enrollments, Certificates, Certificate verifications, Compliance, Courses,
  Members, Seats, Audit events, Webhook subscriptions, the Event log,
  Operations and `Auth.IntrospectAsync`. Every method takes a
  `CancellationToken`.
- Client credentials authentication with cached, single-flight Access tokens
  renewed 60 s before expiry, and one renewal and replay on a 401.
- Live and Sandbox Environments, a `BaseUrl` override, narrower `Scopes`, and
  `StaticTokenProvider` for Access tokens obtained elsewhere. Public methods
  (`Courses`, `CertificateVerifications.GetAsync`) send no token.
- A client without credentials, `new TeachMeHipaaClient(new() { Environment =
  TeachMeHipaaEnvironment.Sandbox })`, for public lookups such as Certificate
  verification. Any other call throws `InvalidOperationException` naming the
  endpoint before sending a request; setting only one of `ClientId` and
  `ClientSecret` still throws `ArgumentException`.
- `services.AddTeachMeHipaa(...)`, which registers one shared client that
  sends through an `IHttpClientFactory` client and returns its
  `IHttpClientBuilder`.
- The Organization binding: discovered once through `Auth.IntrospectAsync`,
  or given as `OrganizationId`.
- Typed exceptions by status (`InvalidRequestException`,
  `AuthenticationException`, `PermissionDeniedException`,
  `NotFoundException`, `ConflictException`, `PreconditionFailedException`,
  `IdempotencyException`, `RateLimitException`, `ApiException`,
  `ApiConnectionException`, `ApiTimeoutException`, `OAuthException`)
  carrying the problem `Code`, `Detail`, field `Errors`, `Headers` and
  `RequestId`, with a guard that throws `ApiException` for non-JSON
  responses instead of a parse error.
- Retries with exponential backoff and jitter for network errors, 409
  `conflict`, 429 and 5xx, honoring `Retry-After`, only for calls that are
  safe to replay; a generated `Idempotency-Key` per create and custom method,
  reused across retries; and `RequestOptions` per call with `IdempotencyKey`,
  `MaxRetries`, `Timeout` and `RequestId` (sent as `x-request-id`).
- `LastResponse` on every returned object, with status, headers, request id
  and ETag.
- `Page<T>`, an `IAsyncEnumerable<T>` across pages with `Items`,
  `NextCursor`, `HasNextPage`, `GetNextPageAsync` and `PagesAsync`, and
  `ListAutoPagingAsync` on every list.
- Conditional writes and reads with ETags (`If-Match` from an `ETag`
  constructor argument, which takes a string or the object read, and
  `GetIfModifiedAsync` returning a `ConditionalResult<T>` on 304), and
  merge-patch updates whose `Optional<T>` fields tell an unset field from one
  set to `null`.
- `Operation.WaitAsync`, `GetExportResult` for export CSVs and
  `GetDeliveryResult` for Test delivery and redrive results.
- Enrollment CSV imports from a path, bytes or a stream (`FileUpload`), and
  Certificate PDF downloads whole (`DownloadPdfAsync`) or streamed
  (`DownloadPdfStreamingAsync`).
- `WebhookUtility.ConstructEvent`, which verifies a Delivery's signature,
  body hash, path and timestamp and returns a typed Event, `UnknownEvent` or
  `TestDelivery`; and `WebhookUtility.GenerateTestHeaders` for testing
  handlers.
- `Events.TailAsync` to read the Event log from a saved position, once or
  following new Events.
- Opt-in debug logging through an `ILogger`, one entry per response with
  method, path, status, latency and request id, and one per connection
  error or timeout, never bodies, headers or credentials; a `User-Agent` with your `AppInfo`; and a pluggable
  `HttpClient` or `HttpMessageHandler`.
- `net8.0` (trimmable, AOT-compatible) and `netstandard2.0` targets with
  nullable annotations. No telemetry and no disk writes.
