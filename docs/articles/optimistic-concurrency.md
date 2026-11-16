---
title: Prevent lost updates with ETags
description: Use If-Match for optimistic concurrency on updates, send merge patches with Optional<T>, and skip unchanged reads with GetIfModifiedAsync.
ms.topic: how-to
ms.date: 11/16/2026
---

# Prevent lost updates with ETags

Each resource carries a strong ETag that changes whenever the resource
does. The client reads it into `LastResponse.ETag` and can send it back,
which stops an update from overwriting a change you haven't seen and
skips downloading a resource that hasn't changed. The examples assume a
`client` built as in the
[quickstart](quickstart.md#read-compliance-and-list-learners).

## Update with If-Match

Updates guarded by an ETag take an `ETag` value. You can pass the ETag
string, or the object you read, whose `LastResponse.ETag` the client sends
as `If-Match`; both convert to `ETag` implicitly.

| Method                             | Where the ETag goes                       | Required |
| ---------------------------------- | ----------------------------------------- | -------- |
| `Learners.UpdateAsync`             | `LearnerUpdateOptions(etag)` constructor  | Yes      |
| `WebhookSubscriptions.UpdateAsync` | `WebhookSubscriptionUpdateOptions(etag)` constructor | Yes |
| `Learners.OffboardAsync`           | `LearnerOffboardOptions.IfMatch`          | No       |

A required ETag is a constructor parameter, so the compiler catches a
missing one. `"*"` matches any version.

<!-- sample: conditional-write -->
```csharp
var learner = await client.Learners.GetAsync("sub_01JD80AB2C3D4E5F6G7H8J9K0M");
try
{
    var updated = await client.Learners.UpdateAsync(
        learner.LearnerId,
        new LearnerUpdateOptions(learner) { Name = "Ana Q. Example", ExternalId = null });
    Console.WriteLine(updated.Name);
}
catch (PreconditionFailedException)
{
    Console.WriteLine("Someone else changed this Learner; read it again");
}
```

When the resource changed after your read, the call throws
`PreconditionFailedException` (412). Read it again, reapply your change
and retry. The client never retries an update on its own, since a replay
could undo another writer's change.

## Send a merge patch

Updates are JSON merge patches, and their properties are `Optional<T>`:

- A property you leave unset isn't sent, and the field keeps its value.
- A property set to `null` is sent as JSON `null`, which clears the field.

In the previous example, `Name` changes, `ExternalId` is cleared and
every other field stays as it was.

## Skip unchanged reads

`Courses`, `Enrollments` and `WebhookSubscriptions` pair `GetAsync` with
`GetIfModifiedAsync`. It takes the ETag of the copy you hold, or the copy
itself, and sends it as `If-None-Match`. When your copy is current, the
API answers 304 and the result's `Value` is `null`, with `IsNotModified`
set:

<!-- sample: not-modified -->
```csharp
var course = await client.Courses.GetAsync("hipaa-2026");

var latest = await client.Courses.GetIfModifiedAsync("hipaa-2026", course);
if (latest.IsNotModified)
{
    Console.WriteLine("still current");
}

course = latest.Value ?? course;
Console.WriteLine(course.Title);
```

`GetAsync` never returns 304, so only code that asks for a conditional
read handles a missing value. The result's `LastResponse` is set in both
cases.

## Next steps

- [Handle errors and retries](handle-errors.md)
- [Resource reference](../reference/resources.md): which methods take
  `If-Match` or offer `GetIfModifiedAsync`.
