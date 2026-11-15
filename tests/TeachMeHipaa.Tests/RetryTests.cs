using System.Net;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class RetryTests
{
    private static readonly ScriptedReply NetworkFailure = new() { Throws = new HttpRequestException("Connection refused") };

    private static ScriptedReply Problem(int status, string code, IReadOnlyDictionary<string, string>? headers = null) =>
        ProblemReply(status, $$"""{"title":"Problem","status":{{status}},"code":"{{code}}"}""", headers);

    private static Dictionary<string, string> RetryAfter(string value) => new() { ["Retry-After"] = value };

    public static TheoryData<string, ScriptedReply> TransientFailures => new()
    {
        { "a network error", NetworkFailure },
        { "409 conflict (request in flight)", Problem(409, "conflict") },
        { "429 rate_limited", Problem(429, "rate_limited") },
        { "500", Problem(500, "internal_error") },
        { "502 refund_failed", Problem(502, "refund_failed") },
        { "503", Problem(503, "temporarily_unavailable") },
        { "a non-problem 503", new ScriptedReply { Status = HttpStatusCode.ServiceUnavailable, Body = "upstream overloaded" } },
    };

    [Theory]
    [MemberData(nameof(TransientFailures))]
    public async Task RetriesAGetAfterATransientFailureAndReturnsTheResult(string failure, ScriptedReply reply)
    {
        _ = failure;
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, reply, LearnerReply());
        var client = NewClient(handler, time: time);

        var learner = await time.RunAsync(client.Learners.GetAsync(LearnerId));

        Assert.Equal(LearnerId, learner.LearnerId);
        Assert.Equal(2, handler.RequestsTo(LearnerRoute).Count);
        // The first backoff is 0.5 s: a 0.5 s ceiling with jitter, floored at 0.5 s.
        Assert.Equal([TimeSpan.FromMilliseconds(500)], time.Fired);
    }

    [Fact]
    public async Task RetriesAfterATimeout()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, ScriptedReply.NeverAnswers, LearnerReply());
        var client = NewClient(handler, time: time);

        await time.RunAsync(client.Learners.GetAsync(LearnerId, requestOptions: new RequestOptions { Timeout = TimeSpan.FromSeconds(2) }));

        Assert.Equal(2, handler.RequestsTo(LearnerRoute).Count);
    }

    [Fact]
    public async Task GivesUpAfterTwoRetriesByDefaultAndRaisesTheLastError()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(
                LearnerRoute,
                Problem(503, "temporarily_unavailable"),
                Problem(503, "temporarily_unavailable"),
                Problem(500, "internal_error"));
        var client = NewClient(handler, time: time);

        var error = await Assert.ThrowsAsync<ApiException>(() => time.RunAsync(client.Learners.GetAsync(LearnerId)));

        Assert.Equal(HttpStatusCode.InternalServerError, error.StatusCode);
        Assert.Equal("internal_error", error.Code);
        Assert.Equal(3, handler.RequestsTo(LearnerRoute).Count);
    }

    [Fact]
    public async Task RaisesAnApiConnectionExceptionOnceNetworkRetriesRunOut()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, NetworkFailure, NetworkFailure, NetworkFailure);
        var client = NewClient(handler, time: time);

        await Assert.ThrowsAsync<ApiConnectionException>(() => time.RunAsync(client.Learners.GetAsync(LearnerId)));

        Assert.Equal(3, handler.RequestsTo(LearnerRoute).Count);
    }

    [Fact]
    public async Task DoublesTheBackoffWithJitterUpToA5SecondCap()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, [.. Enumerable.Repeat(Problem(503, "temporarily_unavailable"), 7), LearnerReply()]);
        var client = NewClient(handler, time: time, maxRetries: 7);

        await time.RunAsync(client.Learners.GetAsync(LearnerId));

        // Ceilings 0.5, 1, 2, 4, 5, 5, 5 s; jitter keeps each within 50–100 %
        // of its ceiling and never below 0.5 s.
        (double Low, double High)[] seconds = [(0.5, 0.5), (0.5, 1), (1, 2), (2, 4), (2.5, 5), (2.5, 5), (2.5, 5)];
        Assert.Equal(seconds.Length, time.Fired.Count);
        Assert.All(
            time.Fired.Zip(seconds),
            pair => Assert.InRange(pair.First.TotalSeconds, pair.Second.Low, pair.Second.High));
    }

    [Fact]
    public async Task WaitsAsLongAsRetryAfterSaysInSeconds()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, Problem(429, "rate_limited", RetryAfter("3")), LearnerReply());
        var client = NewClient(handler, time: time);

        await time.RunAsync(client.Learners.GetAsync(LearnerId));

        Assert.Equal([TimeSpan.FromSeconds(3)], time.Fired);
    }

    [Fact]
    public async Task WaitsUntilARetryAfterHttpDate()
    {
        // TestTime starts at 2026-11-15 12:00:00 UTC.
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(
                LearnerRoute,
                Problem(503, "temporarily_unavailable", RetryAfter("Sun, 15 Nov 2026 12:00:10 GMT")),
                LearnerReply());
        var client = NewClient(handler, time: time);

        await time.RunAsync(client.Learners.GetAsync(LearnerId));

        Assert.Equal([TimeSpan.FromSeconds(10)], time.Fired);
    }

    [Fact]
    public async Task WaitsAtMost60SecondsHoweverLongRetryAfterAsksFor()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, Problem(429, "rate_limited", RetryAfter("3600")), LearnerReply());
        var client = NewClient(handler, time: time);

        await time.RunAsync(client.Learners.GetAsync(LearnerId));

        Assert.Equal([TimeSpan.FromSeconds(60)], time.Fired);
    }

    public static TheoryData<int, string, Type> FailuresNotRetried => new()
    {
        { 409, "seat_unavailable", typeof(ConflictException) },
        { 404, "not_found", typeof(NotFoundException) },
        { 400, "validation_failed", typeof(InvalidRequestException) },
        { 412, "precondition_failed", typeof(PreconditionFailedException) },
    };

    [Theory]
    [MemberData(nameof(FailuresNotRetried))]
    public async Task DoesNotRetryAClientError(int status, string code, Type exceptionType)
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, Problem(status, code));
        var client = NewClient(handler, time: time);

        var error = await Assert.ThrowsAnyAsync<TeachMeHipaaException>(() => time.RunAsync(client.Learners.GetAsync(LearnerId)));

        Assert.IsType(exceptionType, error);
        Assert.Single(handler.RequestsTo(LearnerRoute));
        Assert.Empty(time.Fired);
    }

    [Fact]
    public async Task DoesNotRetryWhenTheClientSetsMaxRetriesTo0()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, Problem(429, "rate_limited"));
        var client = NewClient(handler, time: time, maxRetries: 0);

        await Assert.ThrowsAsync<RateLimitException>(() => time.RunAsync(client.Learners.GetAsync(LearnerId)));

        Assert.Single(handler.RequestsTo(LearnerRoute));
    }

    [Fact]
    public async Task LetsOneRequestOverrideTheClientsMaxRetries()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, NetworkFailure, NetworkFailure, NetworkFailure, LearnerReply(), Problem(503, "temporarily_unavailable"));
        var client = NewClient(handler, time: time, maxRetries: 0);

        var learner = await time.RunAsync(client.Learners.GetAsync(LearnerId, requestOptions: new RequestOptions { MaxRetries = 3 }));
        Assert.Equal(HttpStatusCode.OK, learner.LastResponse?.StatusCode);
        Assert.Equal(4, handler.RequestsTo(LearnerRoute).Count);

        await Assert.ThrowsAsync<ApiException>(() => time.RunAsync(client.Learners.GetAsync(LearnerId)));
        Assert.Equal(5, handler.RequestsTo(LearnerRoute).Count);
    }

    [Fact]
    public async Task TimesEachAttemptWithThePerRequestTimeout()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, ScriptedReply.NeverAnswers, ScriptedReply.NeverAnswers, LearnerReply());
        var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            OrganizationId = OrgId,
            HttpMessageHandler = handler,
            TimeProvider = time,
            Timeout = TimeSpan.FromSeconds(30),
        });

        await time.RunAsync(client.Learners.GetAsync(LearnerId, requestOptions: new RequestOptions { Timeout = TimeSpan.FromSeconds(2) }));

        // Each attempt gets its own 2 s, with the backoff in between.
        Assert.Equal(TimeSpan.FromSeconds(2), time.Fired[0]);
        Assert.Equal(TimeSpan.FromMilliseconds(500), time.Fired[1]);
        Assert.Equal(TimeSpan.FromSeconds(2), time.Fired[2]);
        Assert.Equal(3, handler.RequestsTo(LearnerRoute).Count);
    }

    [Fact]
    public async Task StopsWaitingToRetryWhenTheCallIsCancelled()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, Problem(429, "rate_limited", RetryAfter("30")), LearnerReply());
        var client = NewClient(handler, time: time);
        using var cancellation = new CancellationTokenSource();

        var call = client.Learners.GetAsync(LearnerId, cancellationToken: cancellation.Token);
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => call);
        Assert.Single(handler.RequestsTo(LearnerRoute));
        Assert.Empty(time.Fired);
    }
}
