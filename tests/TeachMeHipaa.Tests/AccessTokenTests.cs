using System.Net;
using Microsoft.Extensions.Time.Testing;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class AccessTokenTests
{
    [Fact]
    public async Task MintsExactlyOneAccessTokenForTwoConcurrentCalls()
    {
        var tokenRequested = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseToken = new TaskCompletionSource<ScriptedReply>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, (_, _) =>
            {
                tokenRequested.SetResult(true);
                return releaseToken.Task;
            })
            .Script(LearnerRoute, LearnerReply(), LearnerReply());
        var client = NewClient(handler);

        var first = client.Learners.GetAsync(LearnerId);
        var second = client.Learners.GetAsync(LearnerId);
        await tokenRequested.Task;
        releaseToken.SetResult(TokenReply("jwt-1"));
        await Task.WhenAll(first, second);

        Assert.Single(handler.RequestsTo(TokenRoute));
        Assert.Equal(
            ["Bearer jwt-1", "Bearer jwt-1"],
            handler.RequestsTo(LearnerRoute).Select(request => request.Header("Authorization")));
    }

    [Fact]
    public async Task ReusesTheAccessTokenUntil60SecondsBeforeItExpires()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 11, 15, 12, 0, 0, TimeSpan.Zero));
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply("jwt-1", expiresIn: 900), TokenReply("jwt-2", expiresIn: 900))
            .Script(LearnerRoute, LearnerReply(), LearnerReply(), LearnerReply());
        var client = NewClient(handler, time: time);

        await client.Learners.GetAsync(LearnerId);
        time.Advance(TimeSpan.FromSeconds(839));
        await client.Learners.GetAsync(LearnerId);
        time.Advance(TimeSpan.FromSeconds(1));
        await client.Learners.GetAsync(LearnerId);

        Assert.Equal(2, handler.RequestsTo(TokenRoute).Count);
        Assert.Equal(
            ["Bearer jwt-1", "Bearer jwt-1", "Bearer jwt-2"],
            handler.RequestsTo(LearnerRoute).Select(request => request.Header("Authorization")));
    }

    [Fact]
    public async Task RaisesTheTokenEndpointsOAuthErrorAndMintsAfreshOnTheNextCall()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(
                TokenRoute,
                new ScriptedReply
                {
                    Status = HttpStatusCode.Unauthorized,
                    Headers = new Dictionary<string, string>
                    {
                        ["WWW-Authenticate"] = "Basic realm=\"TeachMeHIPAA API\"",
                        ["x-request-id"] = "req-token-1",
                    },
                    Json = """{"error":"invalid_client","error_description":"Client authentication failed."}""",
                },
                TokenReply("jwt-2"))
            .Script(LearnerRoute, LearnerReply());
        var client = NewClient(handler);

        var error = await Assert.ThrowsAsync<OAuthException>(() => client.Learners.GetAsync(LearnerId));
        var learner = await client.Learners.GetAsync(LearnerId);

        Assert.Equal(HttpStatusCode.Unauthorized, error.StatusCode);
        Assert.Equal("invalid_client", error.Code);
        Assert.Equal("Client authentication failed.", error.Detail);
        Assert.Equal("req-token-1", error.RequestId);
        Assert.Equal(HttpStatusCode.OK, learner.LastResponse?.StatusCode);
        Assert.Equal("Bearer jwt-2", Assert.Single(handler.RequestsTo(LearnerRoute)).Header("Authorization"));
    }

    [Fact]
    public async Task RequestsOnlyTheConfiguredScopes()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, LearnerReply());
        var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            OrganizationId = OrgId,
            Scopes = ["learners:write", "compliance:read"],
            HttpMessageHandler = handler,
        });

        await client.Learners.GetAsync(LearnerId);

        Assert.Equal(
            "grant_type=client_credentials&scope=learners%3Awrite+compliance%3Aread",
            Assert.Single(handler.RequestsTo(TokenRoute)).Body);
    }
}
