using System.Web;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

/// <summary>Auto-paging lists and the Event log tail, through the public client and the fake transport.</summary>
public class PaginationTests
{
    private const string LearnersRoute = $"GET /v1/organizations/{OrgId}/learners";
    private const string EventsRoute = $"GET /v1/organizations/{OrgId}/events";

    [Fact]
    public async Task IteratingAListFollowsNextCursorThroughAShortPageUntilNull()
    {
        var handler = Scripted(
            LearnersRoute,
            PageReply([Learner("sub_1"), Learner("sub_2")], "cur_2"),
            // Filters apply per page, so a page can be short with more to come.
            PageReply([], "cur_3"),
            PageReply([Learner("sub_3")], null));
        var client = NewClient(handler);

        var learnerIds = new List<string>();
        await foreach (var learner in client.Learners.ListAutoPagingAsync(new LearnerListOptions { Limit = 2 }))
        {
            learnerIds.Add(learner.LearnerId);
        }

        Assert.Equal(["sub_1", "sub_2", "sub_3"], learnerIds);
        Assert.Equal(["limit=2", "limit=2&cursor=cur_2", "limit=2&cursor=cur_3"], Queries(handler, LearnersRoute));
    }

    [Fact]
    public async Task BreakingOutOfTheLoopStopsFetchingPages()
    {
        var handler = Scripted(
            LearnersRoute,
            PageReply([Learner("sub_1"), Learner("sub_2")], "cur_2"),
            PageReply([Learner("sub_3"), Learner("sub_4")], "cur_3"),
            PageReply([Learner("sub_5")], null));
        var client = NewClient(handler);

        var learnerIds = new List<string>();
        await foreach (var learner in client.Learners.ListAutoPagingAsync())
        {
            learnerIds.Add(learner.LearnerId);
            if (learner.LearnerId == "sub_3")
            {
                break;
            }
        }

        Assert.Equal(["sub_1", "sub_2", "sub_3"], learnerIds);
        Assert.Equal(2, handler.RequestsTo(LearnersRoute).Count);
    }

    [Fact]
    public async Task NothingIsFetchedUntilIterationStarts()
    {
        var handler = Scripted(LearnersRoute, PageReply([Learner("sub_1")], null));
        var client = NewClient(handler);

        var learners = client.Learners.ListAutoPagingAsync();

        Assert.Empty(handler.RequestsTo(LearnersRoute));
        Assert.Equal(["sub_1"], await Ids(learners));
    }

    [Fact]
    public async Task EveryPageCarriesTheFirstCallsFiltersLimitAndRequestOptionsUnvalidated()
    {
        var handler = Scripted(
            LearnersRoute,
            PageReply([Learner("sub_1")], "cur_2"),
            PageReply([Learner("sub_2")], null));
        var client = NewClient(handler);

        // 500 is over the API's maximum of 100: the server, not the client, rejects it.
        var learners = client.Learners.ListAutoPagingAsync(
            new LearnerListOptions { Limit = 500, Status = LearnerStatus.InProgress, CourseId = "hipaa-2026" },
            new RequestOptions { RequestId = "req_sync_42" });

        Assert.Equal(["sub_1", "sub_2"], await Ids(learners));
        Assert.Equal(
            ["limit=500&status=in_progress&courseId=hipaa-2026", "limit=500&cursor=cur_2&status=in_progress&courseId=hipaa-2026"],
            Queries(handler, LearnersRoute));
        Assert.All(handler.RequestsTo(LearnersRoute), request => Assert.Equal("req_sync_42", request.Header("x-request-id")));
    }

    [Fact]
    public async Task TheEventLogPagesByAfterTheLastEventsIdNotCursor()
    {
        // §4: `nextCursor` is the last Event id while more rows exist.
        var handler = Scripted(
            EventsRoute,
            PageReply([Event("evt_01"), Event("evt_02")], "evt_02"),
            PageReply([Event("evt_03")], null));
        var client = NewClient(handler);

        var events = client.Events.ListAutoPagingAsync(new EventListOptions { Type = EventType.LearnerCertified });

        Assert.Equal(["evt_01", "evt_02", "evt_03"], await Ids(events));
        Assert.Equal(["type=learner.certified", "after=evt_02&type=learner.certified"], Queries(handler, EventsRoute));
    }

    [Fact]
    public async Task TheEventLogResumesAfterTheLastEventEvenIfNextCursorDiffers()
    {
        // The IR's `after` style keys on Event ids, so the last Event handed
        // out is the authoritative position, not the cursor's spelling of it.
        var handler = Scripted(
            EventsRoute,
            PageReply([Event("evt_01"), Event("evt_02")], "evt_01"),
            PageReply([Event("evt_03")], null));
        var client = NewClient(handler);

        Assert.Equal(["evt_01", "evt_02", "evt_03"], await Ids(client.Events.ListAutoPagingAsync()));
        Assert.Equal(["", "after=evt_02"], Queries(handler, EventsRoute));
    }

    [Fact]
    public async Task TheEventLogFallsBackToNextCursorAfterAnEmptyPage()
    {
        var handler = Scripted(
            EventsRoute,
            PageReply([], "evt_07"),
            PageReply([Event("evt_08")], null));
        var client = NewClient(handler);

        Assert.Equal(["evt_08"], await Ids(client.Events.ListAutoPagingAsync()));
        Assert.Equal(["", "after=evt_07"], Queries(handler, EventsRoute));
    }

    [Fact]
    public async Task AListReturnsItsFirstPageWhichFetchesLaterPagesOnRequest()
    {
        var handler = Scripted(
            LearnersRoute,
            PageReply([Learner("sub_1")], "cur_2") with { Headers = new Dictionary<string, string> { ["x-request-id"] = "req_page_1" } },
            PageReply([Learner("sub_2")], "cur_3"),
            PageReply([Learner("sub_3")], null));
        var client = NewClient(handler);

        var first = await client.Learners.ListAsync(new LearnerListOptions { Limit = 1 });

        Assert.Equal("sub_1", Assert.Single(first.Items).LearnerId);
        Assert.Equal("cur_2", first.NextCursor);
        Assert.Equal("req_page_1", first.LastResponse?.RequestId);
        Assert.True(first.HasNextPage);
        Assert.Single(handler.RequestsTo(LearnersRoute));

        var second = await first.GetNextPageAsync();
        Assert.Equal("sub_2", Assert.Single(second.Items).LearnerId);

        var pages = new List<string[]>();
        await foreach (var page in second.PagesAsync())
        {
            pages.Add([.. page.Items.Select(learner => learner.LearnerId)]);
        }

        Assert.Equal([["sub_2"], ["sub_3"]], pages);
        Assert.Equal(["limit=1", "limit=1&cursor=cur_2", "limit=1&cursor=cur_3"], Queries(handler, LearnersRoute));
    }

    [Fact]
    public async Task GetNextPageOnTheLastPageThrowsWithoutARequest()
    {
        var handler = Scripted(LearnersRoute, PageReply([Learner("sub_1")], null));
        var client = NewClient(handler);

        var last = await client.Learners.ListAsync();

        Assert.False(last.HasNextPage);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => last.GetNextPageAsync());
        Assert.Contains("last page", error.Message, StringComparison.Ordinal);
        Assert.Single(handler.RequestsTo(LearnersRoute));
    }

    [Fact]
    public async Task AFetchedPageIteratesEveryItemFromItselfOnward()
    {
        var handler = Scripted(
            LearnersRoute,
            PageReply([Learner("sub_1")], "cur_2"),
            PageReply([Learner("sub_2")], null),
            PageReply([Learner("sub_1")], "cur_2"),
            PageReply([Learner("sub_2")], null));
        var client = NewClient(handler);

        Assert.Equal(["sub_1", "sub_2"], await Ids(await client.Learners.ListAsync()));
        Assert.Equal(["sub_1", "sub_2"], await Ids((await client.Learners.ListAsync()).AutoPagingAsync()));
    }

    [Fact]
    public async Task CancellingStopsIterationBeforeTheNextPageIsFetched()
    {
        var handler = Scripted(
            LearnersRoute,
            PageReply([Learner("sub_1"), Learner("sub_2")], "cur_2"),
            PageReply([Learner("sub_3")], null));
        var client = NewClient(handler);
        using var cancellation = new CancellationTokenSource();

        var learnerIds = new List<string>();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var learner in client.Learners.ListAutoPagingAsync(cancellationToken: cancellation.Token))
            {
                learnerIds.Add(learner.LearnerId);
                if (learner.LearnerId == "sub_2")
                {
                    await cancellation.CancelAsync();
                }
            }
        });

        Assert.Equal(["sub_1", "sub_2"], learnerIds);
        Assert.Single(handler.RequestsTo(LearnersRoute));
    }

    [Fact]
    public async Task TailResumesFromAnEventIdAndReturnsOnceCaughtUp()
    {
        var time = new TestTime();
        var handler = Scripted(
            EventsRoute,
            PageReply([Event("evt_03"), Event("evt_04")], "evt_04"),
            // §4: `nextCursor` is null at the current end, so it gives no
            // resume point; the tail keeps the last id itself.
            PageReply([Event("evt_05")], null));
        var client = NewClient(handler, time: time);

        var tail = client.Events.TailAsync(new EventTailOptions { After = "evt_02", Type = EventType.LearnerCertified, Limit = 2 });
        Assert.Equal("evt_02", tail.LastEventId);

        Assert.Equal(["evt_03", "evt_04", "evt_05"], await time.RunAsync(Ids(tail)));
        Assert.Equal("evt_05", tail.LastEventId);
        Assert.Equal(
            ["limit=2&after=evt_02&type=learner.certified", "limit=2&after=evt_04&type=learner.certified"],
            Queries(handler, EventsRoute));
        Assert.Empty(time.Fired);
    }

    [Fact]
    public async Task TailWithoutAnIdStartsAtTheOldestRetainedEvent()
    {
        var handler = Scripted(EventsRoute, PageReply([Event("evt_01")], null));
        var client = NewClient(handler);

        var tail = client.Events.TailAsync();
        Assert.Null(tail.LastEventId);

        Assert.Equal(["evt_01"], await Ids(tail));
        Assert.Equal([""], Queries(handler, EventsRoute));
        Assert.Equal("evt_01", tail.LastEventId);
    }

    [Fact]
    public async Task AFollowingTailPollsFromItsLastIdEvery2SecondsUntilCancelled()
    {
        var time = new TestTime();
        var handler = Scripted(
            EventsRoute,
            PageReply([Event("evt_03")], null),
            PageReply([], null),
            PageReply([Event("evt_04"), Event("evt_05")], null));
        var client = NewClient(handler, time: time);
        using var cancellation = new CancellationTokenSource();

        var tail = client.Events.TailAsync(new EventTailOptions { After = "evt_02", Follow = true }, cancellationToken: cancellation.Token);
        var eventIds = new List<string>();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => time.RunAsync(Task.Run(async () =>
        {
            await foreach (var item in tail)
            {
                eventIds.Add(item.EventId);
                if (item.EventId == "evt_04")
                {
                    await cancellation.CancelAsync();
                }
            }
        })));

        // The cancel lands mid-page: evt_05 was fetched but never handed out,
        // so a resume from LastEventId picks it up.
        Assert.Equal(["evt_03", "evt_04"], eventIds);
        Assert.Equal("evt_04", tail.LastEventId);
        Assert.Equal(["evt_02", "evt_03", "evt_03"], AfterParams(handler));
        Assert.Equal([TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2)], time.Fired);
    }

    [Fact]
    public async Task AFollowingTailWaitsTheIntervalAndStopsPollingOnBreak()
    {
        var time = new TestTime();
        var handler = Scripted(EventsRoute, PageReply([], null), PageReply([Event("evt_03")], null));
        var client = NewClient(handler, time: time);

        var tail = client.Events.TailAsync(new EventTailOptions { After = "evt_02", Follow = true, Interval = TimeSpan.FromMilliseconds(250) });
        await time.RunAsync(Task.Run(async () =>
        {
            await foreach (var item in tail)
            {
                Assert.Equal("evt_03", item.EventId);
                break;
            }
        }));

        Assert.Equal(["evt_02", "evt_02"], AfterParams(handler));
        Assert.Equal([TimeSpan.FromMilliseconds(250)], time.Fired);
        Assert.Equal("evt_03", tail.LastEventId);
    }

    [Fact]
    public async Task CancellingAFollowingTailMidWaitEndsIt()
    {
        var time = new PollWatchingTime(TimeSpan.FromSeconds(2));
        var handler = Scripted(EventsRoute, PageReply([Event("evt_03")], null));
        var client = NewClient(handler, time: time);
        using var cancellation = new CancellationTokenSource();

        var tail = client.Events.TailAsync(new EventTailOptions { After = "evt_02", Follow = true });
        var eventIds = new List<string>();
        var tailing = Task.Run(async () =>
        {
            await foreach (var item in tail.WithCancellation(cancellation.Token))
            {
                eventIds.Add(item.EventId);
            }
        });
        await time.PollScheduled;
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => tailing);
        Assert.Equal(["evt_03"], eventIds);
        Assert.Equal("evt_03", tail.LastEventId);
        Assert.Single(handler.RequestsTo(EventsRoute));
    }

    private static FakeHttpMessageHandler Scripted(string route, params ScriptedReply[] pages) =>
        new FakeHttpMessageHandler().Script(TokenRoute, TokenReply()).Script(route, pages);

    /// <summary>A page as the Developer API returns it: <c>{ items, nextCursor }</c>.</summary>
    private static ScriptedReply PageReply(string[] items, string? nextCursor) => new()
    {
        Json = $$"""{"items":[{{string.Join(",", items)}}],"nextCursor":{{(nextCursor is null ? "null" : $"\"{nextCursor}\"")}}}""",
    };

    private static string Learner(string learnerId) => LearnerJson.Replace(LearnerId, learnerId, StringComparison.Ordinal);

    /// <summary>A <c>dev#/components/schemas/Event</c>: a <c>learner.certified</c> envelope.</summary>
    private static string Event(string eventId) => $$$"""
        {"eventId":"{{{eventId}}}","eventType":"learner.certified","orgId":"{{{OrgId}}}","occurredAt":"2026-11-15T16:31:30.000Z",
         "data":{"learnerId":"{{{LearnerId}}}","email":"ana@example.com","externalId":"hr-123","courseId":"hipaa-2026","status":"certified","occurredAt":"2026-11-15T16:31:30.000Z"}}
        """;

    private static async Task<List<string>> Ids(IAsyncEnumerable<Learner> learners)
    {
        var ids = new List<string>();
        await foreach (var learner in learners)
        {
            ids.Add(learner.LearnerId);
        }

        return ids;
    }

    private static async Task<List<string>> Ids(IAsyncEnumerable<Event> events)
    {
        var ids = new List<string>();
        await foreach (var item in events)
        {
            ids.Add(item.EventId);
        }

        return ids;
    }

    private static string[] Queries(FakeHttpMessageHandler handler, string route) =>
        [.. handler.RequestsTo(route).Select(request => request.Uri.Query.TrimStart('?'))];

    /// <summary>Each Event log request's <c>after</c>, or "" when it sent none.</summary>
    private static string[] AfterParams(FakeHttpMessageHandler handler) =>
        [.. handler.RequestsTo(EventsRoute).Select(request => HttpUtility.ParseQueryString(request.Uri.Query)["after"] ?? "")];

    /// <summary>Fake time that never advances on its own and says when the tail starts waiting to poll again.</summary>
    private sealed class PollWatchingTime(TimeSpan interval) : Microsoft.Extensions.Time.Testing.FakeTimeProvider
    {
        private readonly TaskCompletionSource _pollScheduled = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task PollScheduled => _pollScheduled.Task;

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var timer = base.CreateTimer(callback, state, dueTime, period);
            if (dueTime == interval)
            {
                _pollScheduled.TrySetResult();
            }

            return timer;
        }
    }
}
