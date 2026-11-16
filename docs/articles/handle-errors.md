---
title: Handle errors and retries
description: The exception types TeachMeHipaaClient throws, what each carries, how the client retries and how idempotency keys make writes safe to repeat.
ms.topic: how-to
ms.date: 11/16/2026
---

# Handle errors and retries

`TeachMeHipaaClient` retries failures that are safe to retry and throws a
typed exception for the rest. The examples assume a `client` built as in
the [quickstart](quickstart.md#read-compliance-and-list-learners).

## Exception types

Every API failure derives from `TeachMeHipaaException`. The HTTP status
picks the subclass, and all of them live in the `TeachMeHipaa` namespace.

| Exception                     | Thrown for                                                      |
| ----------------------------- | --------------------------------------------------------------- |
| `InvalidRequestException`     | 400, 405, 406, 415 and 422                                      |
| `IdempotencyException`        | 422 with `Code` `idempotency_key_reuse`; derives from `InvalidRequestException` |
| `AuthenticationException`     | 401                                                             |
| `PermissionDeniedException`   | 403, such as a missing Scope                                    |
| `NotFoundException`           | 404                                                             |
| `ConflictException`           | 409                                                             |
| `PreconditionFailedException` | 412 (stale ETag) and 428 (missing `If-Match`)                   |
| `RateLimitException`          | 429; `RetryAfter` holds the wait the API asked for              |
| `ApiException`                | 5xx, or a response body that isn't JSON, such as an HTML page   |
| `ApiConnectionException`      | No response arrived                                             |
| `ApiTimeoutException`         | No response within `Timeout`; derives from `ApiConnectionException` |
| `OAuthException`              | The token endpoint refused to issue a token                     |
| `OperationFailedException`    | `WaitAsync` saw the Operation finish with an error              |
| `OperationTimeoutException`   | `WaitAsync` gave up before the Operation finished               |
| `WebhookSignatureException`   | Webhook verification rejected a Delivery                        |

Some failures never reach the API and so throw standard .NET exceptions.
Cancelling through a `CancellationToken` throws
`OperationCanceledException`. A client without credentials throws
`InvalidOperationException` when you call a method that needs a token,
and an invalid option throws `ArgumentException` from the client's
constructor.

## Catch and inspect an exception

Catch the specific types you handle, then `TeachMeHipaaException` as the
fallback:

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

| Property                             | Contains                                                                     |
| ------------------------------------ | ---------------------------------------------------------------------------- |
| `Code`                               | The machine-readable problem code, such as `not_found` or `external_id_conflict` |
| `Title`, `Detail`                    | Text written for people                                                      |
| `Errors`                             | Field-level problems, each a `FieldError` with a `Path` and a `Message`      |
| `StatusCode`, `Headers`, `RequestId` | Response metadata; `null` when no response arrived                           |
| `RawBody`                            | The response body as text, for logging a body the library couldn't parse    |

Branch on `Code`, since `Detail` is prose and its wording can change.
The API adds codes over time, so keep a fallback branch for codes you
don't recognize.

## Find a call by its request id

Each response carries a request id: `error.RequestId` on a failure,
`LastResponse.RequestId` on a success. Quote it to TeachMeHIPAA support
and they can trace the call.

To choose the id yourself, set `RequestOptions.RequestId`. The client
sends it as `x-request-id` on every attempt, so your logs and
TeachMeHIPAA's share one id for the call. The API echoes ids that match
`^[A-Za-z0-9._-]{1,128}$` and replaces any other.

## Understand the retry policy

The client retries network errors, timeouts, 409 `conflict`, 429 and 5xx
responses, up to `MaxRetries` times (2 by default). The delay starts at
0.5 seconds and doubles, with jitter, to a 5-second ceiling. A
`Retry-After` header overrides it, capped at 60 seconds.

Only calls that are safe to replay get retried:

- Reads.
- Writes that carry an `Idempotency-Key`.
- The few writes that are idempotent by nature, such as
  `Learners.CreateLaunchLinkAsync`.

PATCH and DELETE calls are never retried. The
[resource reference](../reference/resources.md) states the policy for each
method. When retries run out, the client throws the last error; a final
`RateLimitException` carries the server's `RetryAfter` when it sent one.

## Make a write safe to repeat

Every create call and custom method sends a generated UUID as its
`Idempotency-Key` and reuses it for each retry, so a retry never enrolls a
Learner twice. Set `RequestOptions.IdempotencyKey` yourself when the
repeat can come from a different process, such as a sync job that might
run twice for the same day:

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

For 24 hours the API answers a repeated key and body with the original
response and does no new work. The same key with a different body throws
`IdempotencyException`. PATCH and DELETE take no key, and the API ignores
one sent on them.

## Next steps

- [Wait for long-running operations](long-running-operations.md)
- [Prevent lost updates with ETags](optimistic-concurrency.md)
