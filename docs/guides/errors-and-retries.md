# Errors and retries

The client retries what is safe to retry, and throws a typed exception for
everything else. This guide covers what it throws, what each exception
carries, and how retries and idempotency keys keep a retried write from
happening twice.

Samples assume a `client` created as in
[Getting started](getting-started.md#3-make-your-first-call).

## Exception classes

Failed calls throw a subclass of `TeachMeHipaaException`, chosen by HTTP
status. All of them are in the `TeachMeHipaa` namespace.

| Exception                     | When                                                         |
| ----------------------------- | ------------------------------------------------------------ |
| `InvalidRequestException`     | 400, 405, 406, 415 and 422                                   |
| `IdempotencyException`        | 422 `idempotency_key_reuse` (an `InvalidRequestException`)   |
| `AuthenticationException`     | 401                                                          |
| `PermissionDeniedException`   | 403, such as a missing Scope                                 |
| `NotFoundException`           | 404                                                          |
| `ConflictException`           | 409                                                          |
| `PreconditionFailedException` | 412 (stale ETag) and 428 (missing `If-Match`)                |
| `RateLimitException`          | 429, with `RetryAfter`                                       |
| `ApiException`                | 5xx, or a response that isn't JSON (such as an HTML page)    |
| `ApiConnectionException`      | No response arrived                                          |
| `ApiTimeoutException`         | No response within the timeout (an `ApiConnectionException`) |
| `OAuthException`              | The token endpoint refused to issue a token                  |

Three more come from specific features: `OperationFailedException` and
`OperationTimeoutException` from `WaitAsync` (see
[Operations and files](operations-and-files.md)), and
`WebhookSignatureException` from webhook verification (see
[Webhooks](webhooks.md)).

Cancelling a call through its `CancellationToken` throws
`OperationCanceledException`, never a `TeachMeHipaaException`. A client
without credentials throws `InvalidOperationException` for a method that
needs them, and an invalid client option throws `ArgumentException` when
you create the client; neither comes from the API.

## What an exception carries

Each `TeachMeHipaaException` has:

- `Code`: the problem's machine-readable code, such as `not_found` or
  `external_id_conflict`. Branch on it rather than on `Detail`. New codes
  can appear at any time, so keep a fallback.
- `Title` and `Detail`: what went wrong, for people.
- `Errors`: field-level problems, each a `FieldError` with a `Path` and a
  `Message`.
- `StatusCode`, `Headers` and `RequestId`: from the HTTP response, or null
  when no response arrived.
- `RawBody`: the response body as text, for logging what the library
  couldn't read.

<!-- sample: errors -->
```csharp
try
{
    await client.Learners.GetAsync("sub_01JD80AB2C3D4E5F6G7H8J9K0M");
}
catch (NotFoundException)
{
    Console.WriteLine("No such Learner");
}
catch (PermissionDeniedException error)
{
    Console.WriteLine($"Missing a Scope: {error.Detail}");
}
catch (TeachMeHipaaException error)
{
    Console.WriteLine($"{error.Code} {error.Detail} {error.RequestId}");
    foreach (var field in error.Errors)
    {
        Console.WriteLine($"{field.Path} {field.Message}");
    }
}
```

## Request ids

Every response carries a request id: on `error.RequestId` for a failure,
and on `LastResponse.RequestId` for a success. Quote it to TeachMeHIPAA
support and they can find the call.

To use your own, set `RequestOptions.RequestId`: the client sends it as
`x-request-id` on every attempt of the call, so the call can be found in
your logs and TeachMeHIPAA's under the same id. The API echoes an id that
matches `^[A-Za-z0-9._-]{1,128}$` and replaces any other.

## Retries

The client retries network errors, 409 `conflict`, 429 and 5xx up to
`MaxRetries` times (2 by default). It backs off exponentially from 0.5 s up
to 5 s with jitter, and honors `Retry-After` up to 60 s. It only retries
calls that are safe to replay:

- reads;
- writes that carry an `Idempotency-Key`;
- the few writes that are idempotent by nature, such as
  `Learners.CreateLaunchLinkAsync`.

PATCH and DELETE are never retried. The
[resource reference](../reference/resources.md) says for each method
whether it is retried.

When the retries run out, the last error is thrown. A
`RateLimitException` then has `RetryAfter`, how long the API asked you to
wait, if it sent `Retry-After`.

## Idempotency keys

Every create and custom method gets a generated UUID `Idempotency-Key`,
reused by each of its retries, so a retry never enrolls a Learner twice.
Set your own `RequestOptions.IdempotencyKey` to make a call safe to repeat
across processes too, such as a sync job that may run twice for the same
day. For 24 hours, the API answers a repeat of a key and body with the
first response, without doing the work again; reusing a key with a
different body throws `IdempotencyException`. PATCH and DELETE take no key:
the API ignores one on them.

`RequestOptions` also overrides the client's `MaxRetries` and `Timeout` for
one call:

<!-- sample: request-options -->
```csharp
var enrollment = await client.Enrollments.CreateAsync(
    new EnrollmentCreateOptions { Email = "ana@example.com", CourseId = "hipaa-2026" },
    new RequestOptions
    {
        IdempotencyKey = "hr-sync-2026-11-15-ana",
        MaxRetries = 5,
        Timeout = TimeSpan.FromSeconds(10),
        RequestId = "hr-sync-2026-11-15-ana",
    });
Console.WriteLine($"{enrollment.EnrollmentId} {enrollment.LastResponse!.RequestId}");
```
