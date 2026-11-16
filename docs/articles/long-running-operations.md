---
title: Wait for long-running operations
description: Poll an Operation to completion with WaitAsync, read its result, handle failure and timeout, and resume waiting from another process.
ms.topic: how-to
ms.date: 11/16/2026
---

# Wait for long-running operations

Exports, imports, batch writes, Test deliveries and redrives can finish
after the request that started them returns. Those methods return an
`Operation`: a `Name`, a `Done` flag and, once done, a `Response` or an
`Error`. The examples assume a `client` built as in the
[quickstart](quickstart.md#read-compliance-and-list-learners).

## Methods that return an Operation

| Method                                  | Read the result with                        |
| --------------------------------------- | ------------------------------------------- |
| `Certificates.ExportAsync`              | `GetExportResult()`                         |
| `Enrollments.ExportAsync`               | `GetExportResult()`                         |
| `Learners.ExportAsync`                  | `GetExportResult()`                         |
| `Enrollments.ImportAsync`               | `Response`, as a `JsonElement`              |
| `Enrollments.BatchByFilterAsync`        | `Response`, as a `JsonElement`              |
| `Learners.BatchUpsertAsync`             | `Response`, as a `JsonElement`              |
| `WebhookSubscriptions.TestAsync`        | `GetDeliveryResult()`                       |
| `WebhookSubscriptions.RedriveAsync`     | `GetDeliveryResult()`                       |
| `Operations.GetAsync`                   | Whichever applies to the original method    |

## Wait for completion

`WaitAsync` polls until the Operation is done and returns the finished
copy. Here an export runs to completion and hands back its CSV file:

<!-- sample: operations -->
```csharp
var export = await client.Learners.ExportAsync(new LearnerExportOptions
{
    Format = LearnersExportRequestFormat.Csv,
    CourseId = "hipaa-2026",
});
var done = await export.WaitAsync(timeout: TimeSpan.FromMinutes(1), interval: TimeSpan.FromSeconds(2));
ExportResult csv = done.GetExportResult();
Console.WriteLine($"{csv.FileName} {csv.RowCount} {csv.Csv.Length}");
```

| `WaitAsync` parameter | Default   | Effect                                                                    |
| --------------------- | --------- | ------------------------------------------------------------------------- |
| `timeout`             | 5 minutes | Total time to wait before throwing `OperationTimeoutException`            |
| `interval`            | 1 second  | Delay before the first poll; later polls back off with jitter to 10 seconds apart |
| `cancellationToken`   | `default` | Stops waiting, mid-delay included, with `OperationCanceledException`      |

An Operation that is already done returns at once without a request.
Exports always come back done, so their `WaitAsync` costs nothing.

## Handle failure and timeout

`WaitAsync` throws `OperationFailedException` when the Operation finishes
with an `Error`, and `OperationTimeoutException` when `timeout` passes
first. Both carry the `Operation`. A timeout leaves the work running on
the server, so you can wait again later.

## Resume waiting from another process

Keep the Operation's `Name` to check on it from a different process or
after a restart. `Operation.OperationIdFromName(operation.Name)` returns
the id that `client.Operations.GetAsync` takes, and the Operation that
call returns has its own `WaitAsync`.

## Next steps

- [Import and export files](files.md)
- [Receive webhooks in ASP.NET Core](receive-webhooks.md#send-a-test-delivery)
