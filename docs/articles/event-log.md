---
title: Read and follow the Event log
description: List your Organization's Events, match them by type with C# patterns, and tail the log from a BackgroundService that resumes after restarts.
ms.topic: how-to
ms.date: 11/16/2026
---

# Read and follow the Event log

An Event records something that happened in your Organization, such as a
Learner earning a Certificate. The Event log holds 30 days of Events,
oldest first. The examples assume a `client` built as in the
[quickstart](quickstart.md#read-compliance-and-list-learners).

## Prerequisites

- An Access token with the `audit:read` or `webhooks:manage` Scope.

## List Events

`Events.ListAsync` reads the log. Filter it with `Type`, or start after
an Event you already handled with `After`. Each Event type is its own
class, so a type pattern picks out the ones you care about:

<!-- sample: events-list -->
```csharp
await foreach (var @event in client.Events.ListAutoPagingAsync(new() { Type = EventType.LearnerCertified }))
{
    if (@event is EventLearnerCertified certified)
    {
        Console.WriteLine($"{certified.Data.Email} {certified.OccurredAt:yyyy-MM-dd}");
    }
}
```

Every Event has an `EventId`, an `OccurredAt` timestamp and a `Data`
object typed for its class. The
[resource reference](../reference/resources.md#event-types) maps each
Event type to its class.

The API adds Event types over time. An Event of a type this version of
the library doesn't know arrives as `UnknownEvent` with its fields intact,
so a `switch` with a default case keeps working after the API changes.

## Follow the log from a background service

`Events.TailAsync` reads the log oldest first and tracks its position in
`LastEventId`. Persist that id and pass it back as `After` after a
restart, and the tail catches up on everything it missed. This hosted
service does that, with `CursorStore` standing in for your own storage:

<!-- sample: events-tail -->
```csharp
using Microsoft.Extensions.Hosting;
using TeachMeHipaa;

public sealed class EventLogFollower(TeachMeHipaaClient client, CursorStore cursors) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tail = client.Events.TailAsync(
            new EventTailOptions { After = await cursors.LoadAsync(stoppingToken), Follow = true, Interval = TimeSpan.FromSeconds(5) },
            cancellationToken: stoppingToken);
        await foreach (var @event in tail)
        {
            Console.WriteLine(@event.EventId);
            await cursors.SaveAsync(tail.LastEventId, stoppingToken);
        }
    }
}
```

Register it with `builder.Services.AddHostedService<EventLogFollower>()`
next to `AddTeachMeHipaa`, as shown in
[Use dependency injection](dependency-injection.md).

| `EventTailOptions` property | Default    | Effect                                                         |
| --------------------------- | ---------- | -------------------------------------------------------------- |
| `After`                     | `null`     | Starts after this Event id; `null` starts at the oldest Event   |
| `Type`                      | `null`     | Reads only one Event type                                      |
| `Limit`                     | 20         | Events per request, from 1 to 100                              |
| `Follow`                    | `false`    | Keeps polling for new Events after catching up                  |
| `Interval`                  | 2 seconds  | Delay between polls while following                            |

Without `Follow`, the loop ends once it catches up. With `Follow`, it
polls until you `break` or the token is cancelled, which throws
`OperationCanceledException`.

## Choose between the log and webhooks

A consumer that stays down longer than 30 days loses Events, because the
log drops them. To react as Events happen, subscribe with
[webhooks](receive-webhooks.md), and keep `TailAsync` for catching up on
Deliveries you missed.

## Next steps

- [Receive webhooks in ASP.NET Core](receive-webhooks.md)
- [Iterate over paged results](paginate.md)
