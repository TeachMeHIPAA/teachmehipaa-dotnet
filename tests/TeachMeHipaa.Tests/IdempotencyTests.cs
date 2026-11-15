using System.Net;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public partial class IdempotencyTests
{
    private const string CreateRoute = $"POST /v1/organizations/{OrgId}/learners";
    private const string UpdateRoute = $"PATCH /v1/organizations/{OrgId}/learners/{LearnerId}";
    private const string LaunchLinkRoute = $"POST /v1/organizations/{OrgId}/learners/{LearnerId}:createLaunchLink";
    private const string DeleteSubscriptionRoute = $"DELETE /v1/organizations/{OrgId}/webhook-subscriptions/whs_01JD9";

    private static readonly ScriptedReply Unavailable =
        ProblemReply(503, """{"title":"Service Unavailable","status":503,"code":"not_configured"}""");

    private static readonly ScriptedReply NetworkFailure = new() { Throws = new HttpRequestException("Connection refused") };

    private static readonly ScriptedReply Created = new() { Status = HttpStatusCode.Created, Json = LearnerJson };

    /// <summary><c>dev#/components/schemas/LearnerLaunchLink</c>.</summary>
    private static readonly ScriptedReply LaunchLinkReply = new()
    {
        Status = HttpStatusCode.Created,
        Json = """{"url":"https://training.teachmehipaa.com/launch/ll_01JD9","expiresAt":"2026-11-15T12:05:00.000Z"}""",
    };

    [GeneratedRegex("^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$")]
    private static partial Regex UuidV4();

    [Fact]
    public async Task SendsACreateAsJsonWithAGeneratedUuidV4IdempotencyKey()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(CreateRoute, Created);
        var client = NewClient(handler);

        var learner = await client.Learners.CreateAsync(new LearnerCreateOptions
        {
            Email = "ana@example.com",
            ExternalId = "hr-123",
        });

        Assert.Equal(HttpStatusCode.Created, learner.LastResponse?.StatusCode);
        Assert.Equal(LearnerId, learner.LearnerId);
        var request = Assert.Single(handler.RequestsTo(CreateRoute));
        Assert.Equal("application/json", request.Header("Content-Type"));
        Assert.Matches(UuidV4(), request.Header("Idempotency-Key"));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("""{"email":"ana@example.com","externalId":"hr-123"}"""),
            JsonNode.Parse(request.Body!)));
    }

    [Fact]
    public async Task RetriesACreateWithTheSameIdempotencyKeyAndBody()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(CreateRoute, Unavailable, NetworkFailure, Created);
        var client = NewClient(handler, time: time);

        await time.RunAsync(client.Learners.CreateAsync(new LearnerCreateOptions { Email = "ana@example.com" }));

        var attempts = handler.RequestsTo(CreateRoute);
        Assert.Equal(3, attempts.Count);
        Assert.Single(attempts.Select(attempt => attempt.Header("Idempotency-Key")).Distinct());
        Assert.Single(attempts.Select(attempt => attempt.Body).Distinct());
        Assert.Equal("""{"email":"ana@example.com"}""", attempts[2].Body);
    }

    [Fact]
    public async Task GeneratesANewIdempotencyKeyForEachCall()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(CreateRoute, Created, Created);
        var client = NewClient(handler);

        await client.Learners.CreateAsync(new LearnerCreateOptions { Email = "ana@example.com" });
        await client.Learners.CreateAsync(new LearnerCreateOptions { Email = "ana@example.com" });

        var keys = handler.RequestsTo(CreateRoute).Select(request => request.Header("Idempotency-Key")).ToArray();
        Assert.All(keys, key => Assert.Matches(UuidV4(), key));
        Assert.NotEqual(keys[0], keys[1]);
    }

    [Fact]
    public async Task UsesACallerSuppliedIdempotencyKeyAsIsRetriesIncluded()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(CreateRoute, Unavailable, Created);
        var client = NewClient(handler, time: time);

        await time.RunAsync(client.Learners.CreateAsync(
            new LearnerCreateOptions { Email = "ana@example.com" },
            new RequestOptions { IdempotencyKey = "hr-sync-2026-11-15-hr-123" }));

        Assert.Equal(
            ["hr-sync-2026-11-15-hr-123", "hr-sync-2026-11-15-hr-123"],
            handler.RequestsTo(CreateRoute).Select(request => request.Header("Idempotency-Key")));
    }

    [Fact]
    public async Task SendsNoIdempotencyKeyOrBodyOnAGet()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, LearnerReply());
        var client = NewClient(handler);

        await client.Learners.GetAsync(LearnerId);

        var request = Assert.Single(handler.RequestsTo(LearnerRoute));
        Assert.Null(request.Header("Idempotency-Key"));
        Assert.Null(request.Header("Content-Type"));
        Assert.Null(request.Body);
    }

    public static TheoryData<string, ScriptedReply, Type> PatchFailures => new()
    {
        { "a 5xx", Unavailable, typeof(ApiException) },
        { "a network error", NetworkFailure, typeof(ApiConnectionException) },
    };

    [Theory]
    [MemberData(nameof(PatchFailures))]
    public async Task DoesNotRetryAPatch(string failure, ScriptedReply reply, Type exceptionType)
    {
        _ = failure;
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(UpdateRoute, reply, LearnerReply());
        var client = NewClient(handler, time: time);

        var error = await Assert.ThrowsAnyAsync<TeachMeHipaaException>(() => time.RunAsync(
            client.Learners.UpdateAsync(LearnerId, new LearnerUpdateOptions { Name = "Ana Example" })));

        Assert.IsType(exceptionType, error);
        var attempt = Assert.Single(handler.RequestsTo(UpdateRoute));
        Assert.Equal("application/merge-patch+json", attempt.Header("Content-Type"));
        Assert.Null(attempt.Header("Idempotency-Key"));
        Assert.Equal("""{"name":"Ana Example"}""", attempt.Body);
    }

    [Fact]
    public async Task RetriesCreateLaunchLinkSafeByMeaningWithoutAnIdempotencyKey()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LaunchLinkRoute, NetworkFailure, LaunchLinkReply);
        var client = NewClient(handler, time: time);

        var link = await time.RunAsync(client.Learners.CreateLaunchLinkAsync(LearnerId));

        Assert.Equal("https://training.teachmehipaa.com/launch/ll_01JD9", link.Url);
        Assert.Equal(new DateTimeOffset(2026, 11, 15, 12, 5, 0, TimeSpan.Zero), link.ExpiresAt);
        var attempts = handler.RequestsTo(LaunchLinkRoute);
        Assert.Equal(2, attempts.Count);
        Assert.All(attempts, attempt => Assert.Null(attempt.Header("Idempotency-Key")));
        Assert.All(attempts, attempt => Assert.Null(attempt.Body));
    }

    [Fact]
    public async Task AcceptsA204WithNoBody()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(DeleteSubscriptionRoute, new ScriptedReply
            {
                Status = HttpStatusCode.NoContent,
                Headers = new Dictionary<string, string> { ["x-request-id"] = "req-del-1" },
            });
        var client = NewClient(handler);

        var response = await client.WebhookSubscriptions.DeleteAsync("whs_01JD9");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("req-del-1", response.RequestId);
    }

    [Fact]
    public async Task DoesNotRetryADelete()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(DeleteSubscriptionRoute, Unavailable, new ScriptedReply { Status = HttpStatusCode.NoContent });
        var client = NewClient(handler, time: time);

        await Assert.ThrowsAsync<ApiException>(() => time.RunAsync(client.WebhookSubscriptions.DeleteAsync("whs_01JD9")));

        Assert.Single(handler.RequestsTo(DeleteSubscriptionRoute));
    }

    [Fact]
    public async Task RaisesAnHtmlPageForANoContentCallAsAnApiException()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(DeleteSubscriptionRoute, new ScriptedReply
            {
                Headers = new Dictionary<string, string> { ["Content-Type"] = "text/html" },
                Body = "<html>wrong host</html>",
            });
        var client = NewClient(handler);

        await Assert.ThrowsAsync<ApiException>(() => client.WebhookSubscriptions.DeleteAsync("whs_01JD9"));
    }
}
