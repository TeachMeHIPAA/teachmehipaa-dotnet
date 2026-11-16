# Conditional requests

Resources carry a strong ETag that changes whenever the resource does. The
library reads it into `LastResponse.ETag` and sends it back for you, to
stop an update from overwriting a change you haven't seen, and to skip
re-downloading something that hasn't changed.

Samples assume a `client` created as in
[Getting started](getting-started.md#3-make-your-first-call).

## Conditional updates with `If-Match`

Updates that the API guards with ETags take an `ETag`: the ETag string, or
the object you read, whose `LastResponse.ETag` the client sends as
`If-Match`. Both convert to an `ETag` implicitly.

| Method                             | Options                                  | `ETag`                           |
| ---------------------------------- | ---------------------------------------- | -------------------------------- |
| `Learners.UpdateAsync`             | `LearnerUpdateOptions(etag)`             | Required, in the constructor     |
| `WebhookSubscriptions.UpdateAsync` | `WebhookSubscriptionUpdateOptions(etag)` | Required, in the constructor     |
| `Learners.OffboardAsync`           | `LearnerOffboardOptions`                 | Optional, the `IfMatch` property |

A required ETag is a constructor argument, so leaving it out doesn't
compile. If the resource changed since you read it, the call throws
`PreconditionFailedException` (412): read it again, reapply your change and
retry. `"*"` matches any version.

## Merge-patch updates

Updates are JSON merge patches. Their properties are `Optional<T>`: a
property you don't set is not sent and stays unchanged, while `null` is
sent as JSON `null` and clears the field.

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

Here `Name` changes, `ExternalId` is cleared, and every other field stays
as it was. Updates are never retried, since replaying one could undo
someone else's change.

## Conditional reads with `GetIfModifiedAsync`

`Courses`, `Enrollments` and `WebhookSubscriptions` have a
`GetIfModifiedAsync` beside `GetAsync`. It takes the ETag of the copy you
hold, or that copy itself, and sends it as `If-None-Match`. When your copy
is still current, the API answers 304 and the result's `Value` is `null`
(`IsNotModified`), so keep your copy:

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

`GetAsync` itself never returns 304, so you only handle a missing value
where you asked for one. The result's `LastResponse` is set either way.
