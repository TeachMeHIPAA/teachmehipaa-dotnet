---
title: Iterate over paged results
description: Read list results with await foreach, page by page with Page<T>, and stop early or cancel between pages.
ms.topic: how-to
ms.date: 11/16/2026
---

# Iterate over paged results

List methods return one `Page<T>` per request. A page is also an
`IAsyncEnumerable<T>`, so `await foreach` walks every item across every
page and requests the next page only when the loop reaches it. The
examples assume a `client` built as in the
[quickstart](quickstart.md#read-compliance-and-list-learners).

## Iterate over every item

Each list method has an `AutoPagingAsync` sibling that returns the items
directly. Use it when you want every item and don't need page metadata:

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

`Limit` sets the page size. It defaults to 20 and tops out at 100, or at
1,000 for Enrollments and Members. A `break` out of the loop stops
further requests.

## Work page by page

The second half of the previous example drives the pages itself.
`Page<T>` exposes these members:

| Member                     | Description                                                                 |
| -------------------------- | --------------------------------------------------------------------------- |
| `Items`                    | The items on this page                                                      |
| `NextCursor`               | The cursor for the next page, or `null` on the last page                    |
| `HasNextPage`              | `true` when `NextCursor` is set                                             |
| `GetNextPageAsync()`       | Fetches the next page with the same filters, limit and `RequestOptions`     |
| `PagesAsync()`             | Yields this page and each later page                                        |
| `AutoPagingAsync()`        | Yields every item from this page onward                                     |
| `LastResponse`             | The HTTP response that produced this page                                   |

Page-by-page reads suit batch work, such as writing a page to a database
in one transaction, or logging progress after each request.

## Cancel between pages

Pass a `CancellationToken` to `ListAutoPagingAsync`, or call
`WithCancellation` on a page inside `await foreach`. The token stops the
loop before the next request and throws `OperationCanceledException`.

## Next steps

- [Read and follow the Event log](event-log.md): the Event log is a list
  with its own tailing helper.
- [Resource reference](../reference/resources.md): the filters each list
  method accepts.
