# Pagination and Events

List calls return one page at a time, and the library fetches the rest as
you iterate. The Event log is a list too, with a helper that follows it as
new Events arrive.

Samples assume a `client` created as in
[Getting started](getting-started.md#3-make-your-first-call).

## Iterating a list call

A list method returns one `Page<T>`, with `Items`, `NextCursor` and
`HasNextPage`. A page is also an `IAsyncEnumerable<T>`: `await foreach`
over it yields every item across all pages, fetching the next page only
when the loop reaches it, and stops fetching when you `break`.
`ListAutoPagingAsync` does the same without awaiting the first page
yourself, as in Stripe's library. `Limit` sets the page size: 20 by
default, and at most 100 (1,000 for Enrollments and Members).

<!-- sample: pagination -->
```csharp
// Every Learner, page by page, fetched lazily.
await foreach (var learner in client.Learners.ListAutoPagingAsync(new() { Limit = 100 }))
{
    Console.WriteLine(learner.LearnerId);
}

// One page at a time.
var page = await client.Enrollments.ListAsync(new() { Limit = 50 });
Console.WriteLine(page.Items.Count);
while (page.HasNextPage)
{
    page = await page.GetNextPageAsync();
    Console.WriteLine(page.Items.Count);
}
```

`page.PagesAsync()` yields each page in turn. The next page repeats the
first call's filters, such as `Status`, its limit and its `RequestOptions`.
Pass a `CancellationToken` to `ListAutoPagingAsync`, or use
`WithCancellation` on a page, to stop between pages.

## The Event log

An Event records that something happened in your Organization, such as a
Learner being certified. The Organization's Event log keeps Events for 30
days, oldest first. Every Event has an `EventId`, an `OccurredAt` time and
`Data` whose type depends on the Event's class; the
[resource reference](../reference/resources.md#event-types) lists the
Event types and their classes.

`Events.ListAsync` reads the log. Set `Type` to read one Event type, and
`After` to start after an Event you already handled. Tell Events apart by
their class, with a type pattern:

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

New Event types are added over time. An Event of a type this version of
the library doesn't know arrives as `UnknownEvent`, with its fields kept,
so it never breaks your loop. Reading the log needs the `audit:read` or
`webhooks:manage` Scope.

## Tailing the Event log

`Events.TailAsync` reads the Event log oldest first and keeps its own
position in `LastEventId`. Save it, and resume from it after a restart to
catch up on anything you missed while down, without webhooks. In a hosted
service, with `CursorStore` standing in for your own storage:

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

Without `Follow`, the loop ends once it has caught up. With it, the tail
polls every `Interval` (2 seconds by default) until you `break` or cancel,
which throws `OperationCanceledException`.

The log keeps 30 days of Events, so a consumer that is down longer than
that misses Events. For Events as they happen, subscribe to them with
[webhooks](webhooks.md); the log and `TailAsync` are the way to catch up on
Deliveries you missed.
