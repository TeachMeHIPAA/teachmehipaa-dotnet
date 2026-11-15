using System.Net;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class AccessTokenReplayTests
{
    private const string CreateRoute = $"POST /v1/organizations/{OrgId}/learners";

    /// <summary>What the API answers for a revoked or expired Access token.</summary>
    private static readonly ScriptedReply Unauthorized = ProblemReply(
        401,
        """{"title":"Unauthorized","status":401,"code":"unauthorized"}""",
        new Dictionary<string, string> { ["WWW-Authenticate"] = "Bearer realm=\"TeachMeHIPAA API\", error=\"invalid_token\"" });

    private static readonly ScriptedReply Unavailable =
        ProblemReply(503, """{"title":"Service Unavailable","status":503,"code":"temporarily_unavailable"}""");

    private static readonly ScriptedReply NetworkFailure = new() { Throws = new HttpRequestException("Connection refused") };

    private static IEnumerable<string?> Authorizations(FakeHttpMessageHandler handler, string route = LearnerRoute) =>
        handler.RequestsTo(route).Select(request => request.Header("Authorization"));

    [Fact]
    public async Task ReMintsTheAccessTokenOnceAndReplaysTheRequest()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply("jwt-1"), TokenReply("jwt-2"))
            .Script(LearnerRoute, Unauthorized, LearnerReply());
        var client = NewClient(handler);

        var learner = await client.Learners.GetAsync(LearnerId);

        Assert.Equal(HttpStatusCode.OK, learner.LastResponse?.StatusCode);
        Assert.Equal(["Bearer jwt-1", "Bearer jwt-2"], Authorizations(handler));
    }

    [Fact]
    public async Task DoesNotCountTheReplayAsARetry()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply("jwt-1"), TokenReply("jwt-2"))
            .Script(LearnerRoute, Unauthorized, LearnerReply());
        var client = NewClient(handler, maxRetries: 0);

        var learner = await client.Learners.GetAsync(LearnerId);

        Assert.Equal(HttpStatusCode.OK, learner.LastResponse?.StatusCode);
    }

    [Fact]
    public async Task StillHasEveryRetryLeftAfterAReplay()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply("jwt-1"), TokenReply("jwt-2"))
            .Script(LearnerRoute, Unauthorized, Unavailable, Unavailable, LearnerReply());
        var client = NewClient(handler, time: time);

        await time.RunAsync(client.Learners.GetAsync(LearnerId));

        Assert.Equal(["Bearer jwt-1", "Bearer jwt-2", "Bearer jwt-2", "Bearer jwt-2"], Authorizations(handler));
    }

    [Fact]
    public async Task ReplaysOnlyOnceAndThenRaisesAnAuthenticationException()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply("jwt-1"), TokenReply("jwt-2"))
            .Script(LearnerRoute, Unauthorized, Unauthorized);
        var client = NewClient(handler);

        var error = await Assert.ThrowsAsync<AuthenticationException>(() => client.Learners.GetAsync(LearnerId));

        Assert.Equal("unauthorized", error.Code);
        Assert.Equal(2, handler.RequestsTo(TokenRoute).Count);
        Assert.Equal(2, handler.RequestsTo(LearnerRoute).Count);
    }

    [Fact]
    public async Task DoesNotReplayA401WithAnotherCode()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply("jwt-1"))
            .Script(LearnerRoute, ProblemReply(401, """{"title":"Unauthorized","status":401,"code":"mfa_required"}"""));
        var client = NewClient(handler);

        await Assert.ThrowsAsync<AuthenticationException>(() => client.Learners.GetAsync(LearnerId));

        Assert.Single(handler.RequestsTo(TokenRoute));
    }

    [Fact]
    public async Task ReplaysAPostWithTheSameIdempotencyKeyAndBody()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply("jwt-1"), TokenReply("jwt-2"))
            .Script(CreateRoute, Unauthorized, new ScriptedReply { Status = HttpStatusCode.Created, Json = LearnerJson });
        var client = NewClient(handler);

        await client.Learners.CreateAsync(new LearnerCreateOptions { Email = "ana@example.com" });

        var attempts = handler.RequestsTo(CreateRoute);
        Assert.Equal(["Bearer jwt-1", "Bearer jwt-2"], Authorizations(handler, CreateRoute));
        Assert.NotNull(attempts[0].Header("Idempotency-Key"));
        Assert.Equal(attempts[0].Header("Idempotency-Key"), attempts[1].Header("Idempotency-Key"));
        Assert.Equal("""{"email":"ana@example.com"}""", attempts[1].Body);
    }

    [Fact]
    public async Task ReMintsOnceWhenConcurrentCallsHit401WithTheSameToken()
    {
        var bothSent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var arrived = 0;
        Task<ScriptedReply> UnauthorizedOnceBothArrive(RecordedRequest request, CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref arrived) == 2)
            {
                bothSent.SetResult(true);
            }

            return bothSent.Task.ContinueWith(_ => Unauthorized, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply("jwt-1"), TokenReply("jwt-2"))
            .Script(LearnerRoute, UnauthorizedOnceBothArrive)
            .Script(LearnerRoute, UnauthorizedOnceBothArrive)
            .Script(LearnerRoute, LearnerReply(), LearnerReply());
        var client = NewClient(handler);

        await Task.WhenAll(client.Learners.GetAsync(LearnerId), client.Learners.GetAsync(LearnerId));

        Assert.Equal(2, handler.RequestsTo(TokenRoute).Count);
        Assert.Equal(["Bearer jwt-1", "Bearer jwt-1", "Bearer jwt-2", "Bearer jwt-2"], Authorizations(handler));
    }

    [Fact]
    public async Task RaisesTheOAuthExceptionWhenTheReMintFails()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply("jwt-1"), new ScriptedReply
            {
                Status = HttpStatusCode.Unauthorized,
                Json = """{"error":"invalid_client","error_description":"The API client was revoked."}""",
            })
            .Script(LearnerRoute, Unauthorized);
        var client = NewClient(handler);

        var error = await Assert.ThrowsAsync<OAuthException>(() => client.Learners.GetAsync(LearnerId));

        Assert.Equal("invalid_client", error.Code);
    }

    public static TheoryData<string, ScriptedReply> MintFailures => new()
    {
        { "a network error", NetworkFailure },
        { "a 5xx", Unavailable },
    };

    [Theory]
    [MemberData(nameof(MintFailures))]
    public async Task RetriesAClientCredentialsMint(string failure, ScriptedReply reply)
    {
        _ = failure;
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, reply, TokenReply("jwt-1"))
            .Script(LearnerRoute, LearnerReply());
        var client = NewClient(handler, time: time);

        await time.RunAsync(client.Learners.GetAsync(LearnerId));

        Assert.Equal(2, handler.RequestsTo(TokenRoute).Count);
        Assert.Equal(["Bearer jwt-1"], Authorizations(handler));
        Assert.Equal(
            handler.RequestsTo(TokenRoute)[0].Body,
            handler.RequestsTo(TokenRoute)[1].Body);
    }

    [Theory]
    [InlineData(400, "invalid_scope")]
    [InlineData(401, "invalid_client")]
    public async Task DoesNotRetryAMintTheTokenEndpointRefused(int status, string error)
    {
        var handler = new FakeHttpMessageHandler().Script(TokenRoute, new ScriptedReply
        {
            Status = (HttpStatusCode)status,
            Json = $$"""{"error":"{{error}}"}""",
        });
        var client = NewClient(handler);

        await Assert.ThrowsAsync<OAuthException>(() => client.Learners.GetAsync(LearnerId));

        Assert.Single(handler.RequestsTo(TokenRoute));
    }

    [Fact]
    public async Task RaisesTheMintFailureOnceItsRetriesRunOutWithoutRetryingTheRequestOnTop()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler().Script(TokenRoute, NetworkFailure, NetworkFailure, NetworkFailure);
        var client = NewClient(handler, time: time);

        await Assert.ThrowsAsync<ApiConnectionException>(() => time.RunAsync(client.Learners.GetAsync(LearnerId)));

        Assert.Equal(3, handler.RequestsTo(TokenRoute).Count);
        Assert.Empty(handler.RequestsTo(LearnerRoute));
    }
}
