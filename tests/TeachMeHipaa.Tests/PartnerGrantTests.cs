using System.Collections.Concurrent;
using Microsoft.Extensions.Time.Testing;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class PartnerGrantTests
{
    /// <summary>
    /// A token store that records each save, and how many requests the fake
    /// had seen at that moment, so a test can check the order.
    /// </summary>
    private sealed class RecordingTokenStore(FakeHttpMessageHandler handler, string? stored = null) : ITokenStore
    {
        public ConcurrentQueue<(string RefreshToken, int RequestsSoFar)> Saves { get; } = new();

        public Exception? SaveFailure { get; set; }

        public int Loads { get; private set; }

        public ValueTask<string> LoadAsync(CancellationToken cancellationToken)
        {
            Loads++;
            return new ValueTask<string>(stored ?? throw new InvalidOperationException("nothing stored"));
        }

        public ValueTask SaveAsync(string refreshToken, CancellationToken cancellationToken)
        {
            Saves.Enqueue((refreshToken, handler.Requests.Count));
            return SaveFailure is null ? default : throw SaveFailure;
        }
    }

    private static PartnerGrantTokenProvider NewProvider(
        FakeHttpMessageHandler handler,
        ITokenStore store,
        string? refreshToken = RefreshToken1,
        TimeProvider? time = null,
        IReadOnlyList<string>? scopes = null) =>
        new(new PartnerGrantTokenProviderOptions
        {
            ClientId = AppId,
            ClientSecret = AppSecret,
            TokenStore = store,
            RefreshToken = refreshToken,
            Scopes = scopes,
            HttpMessageHandler = handler,
            TimeProvider = time,
        });

    private static TeachMeHipaaClient NewClient(FakeHttpMessageHandler handler, ITokenProvider tokenProvider, TimeProvider? time = null) =>
        new(new TeachMeHipaaClientOptions
        {
            TokenProvider = tokenProvider,
            OrganizationId = OrgId,
            HttpMessageHandler = handler,
            TimeProvider = time,
        });

    [Fact]
    public async Task ExchangesTheRefreshTokenAndCallsTheApiWithTheNewAccessToken()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, GrantTokenReply("access-1", RefreshToken2))
            .Script(LearnerRoute, LearnerReply());
        var client = NewClient(handler, NewProvider(handler, new RecordingTokenStore(handler)));

        await client.Learners.GetAsync(LearnerId);

        var exchange = Assert.Single(handler.RequestsTo(TokenRoute));
        Assert.Equal($"Basic {AppBasicCredentials}", exchange.Header("Authorization"));
        Assert.Equal(
            "grant_type=refresh_token&refresh_token=tmh_rt_grt_01JD81QW3E4R5T6Y7U8I9O0PAS.first",
            exchange.Body);
        Assert.Equal("Bearer access-1", Assert.Single(handler.RequestsTo(LearnerRoute)).Header("Authorization"));
    }

    [Fact]
    public async Task NarrowsTheAccessTokenToTheGivenScopes()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, GrantTokenReply("access-1", RefreshToken2))
            .Script(LearnerRoute, LearnerReply());
        var provider = NewProvider(handler, new RecordingTokenStore(handler), scopes: ["learners:read"]);

        await NewClient(handler, provider).Learners.GetAsync(LearnerId);

        Assert.EndsWith("&scope=learners%3Aread", Assert.Single(handler.RequestsTo(TokenRoute)).Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheTokenStoreReceivesTheRotatedRefreshTokenBeforeTheApiRequestIsSent()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, GrantTokenReply("access-1", RefreshToken2))
            .Script(LearnerRoute, LearnerReply());
        var store = new RecordingTokenStore(handler);

        await NewClient(handler, NewProvider(handler, store)).Learners.GetAsync(LearnerId);

        // Saved after the exchange (1 request) and before the API call (2).
        Assert.Equal([(RefreshToken2, 1)], store.Saves);
        Assert.Equal([TokenRoute, LearnerRoute], handler.Requests.Select(request => request.Route));
    }

    [Fact]
    public async Task LoadsTheFirstRefreshTokenFromTheTokenStoreWhenNoneIsGiven()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, GrantTokenReply("access-1", RefreshToken2), GrantTokenReply("access-2", RefreshToken3))
            .Script(LearnerRoute, LearnerReply(), ProblemReply(401, """{"title":"Unauthorized","status":401,"code":"unauthorized"}"""), LearnerReply());
        var store = new RecordingTokenStore(handler, stored: RefreshToken1);
        var client = NewClient(handler, NewProvider(handler, store, refreshToken: null));

        await client.Learners.GetAsync(LearnerId);
        await client.Learners.GetAsync(LearnerId);

        Assert.Equal(1, store.Loads);
        Assert.Equal(
            [$"refresh_token={RefreshToken1}", $"refresh_token={RefreshToken2}"],
            handler.RequestsTo(TokenRoute).Select(request => request.Body!.Split('&')[1]));
    }

    [Fact]
    public async Task AFailedSaveAbortsTheCallAndKeepsTheRotatedRefreshTokenInMemory()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, GrantTokenReply("access-1", RefreshToken2), GrantTokenReply("access-2", RefreshToken3))
            .Script(LearnerRoute, LearnerReply());
        var storeDown = new InvalidOperationException("database unavailable");
        var store = new RecordingTokenStore(handler) { SaveFailure = storeDown };
        var client = NewClient(handler, NewProvider(handler, store));

        var error = await Assert.ThrowsAsync<TokenStoreException>(() => client.Learners.GetAsync(LearnerId));

        Assert.Same(storeDown, error.InnerException);
        Assert.DoesNotContain(RefreshToken2, error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("access-1", error.Message, StringComparison.Ordinal);
        Assert.Empty(handler.RequestsTo(LearnerRoute));

        store.SaveFailure = null;
        await client.Learners.GetAsync(LearnerId);

        Assert.Contains(
            "refresh_token=tmh_rt_grt_01JD81QW3E4R5T6Y7U8I9O0PAS.second",
            handler.RequestsTo(TokenRoute)[1].Body,
            StringComparison.Ordinal);
        Assert.Equal([RefreshToken2, RefreshToken3], store.Saves.Select(save => save.RefreshToken));
    }

    [Fact]
    public async Task ARefreshThatLosesTheNetworkRaisesAfterExactlyOneAttempt()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, new ScriptedReply { Throws = new HttpRequestException("connection reset") });
        var store = new RecordingTokenStore(handler);
        var client = NewClient(handler, NewProvider(handler, store));

        var error = await Assert.ThrowsAsync<ApiConnectionException>(() => client.Learners.GetAsync(LearnerId));

        Assert.Single(handler.Requests);
        Assert.Empty(store.Saves);
        Assert.Contains("was not retried", error.Message, StringComparison.Ordinal);
        Assert.Contains("re-run consent", error.Message, StringComparison.Ordinal);
        Assert.IsType<HttpRequestException>(error.InnerException);
    }

    [Fact]
    public async Task ARefreshAnsweredWithA503IsNotRetried()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, new ScriptedReply { Status = System.Net.HttpStatusCode.ServiceUnavailable, Json = "{}" });
        var client = NewClient(handler, NewProvider(handler, new RecordingTokenStore(handler)));

        await Assert.ThrowsAsync<ApiException>(() => client.Learners.GetAsync(LearnerId));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task InvalidGrantSaysTheGrantWasRevokedOrReusedAndToRerunConsent()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, OAuthErrorReply("invalid_grant", "The refresh token is invalid"));
        var client = NewClient(handler, NewProvider(handler, new RecordingTokenStore(handler)));

        var error = await Assert.ThrowsAsync<OAuthException>(() => client.Learners.GetAsync(LearnerId));

        Assert.Equal("invalid_grant", error.Code);
        Assert.Contains("The refresh token is invalid", error.Message, StringComparison.Ordinal);
        Assert.Contains("revoked", error.Message, StringComparison.Ordinal);
        Assert.Contains("reused", error.Message, StringComparison.Ordinal);
        Assert.Contains("Re-run consent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ConcurrentCallsShareOneExchange()
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
        var store = new RecordingTokenStore(handler);
        var client = NewClient(handler, NewProvider(handler, store));

        var first = client.Learners.GetAsync(LearnerId);
        var second = client.Learners.GetAsync(LearnerId);
        await tokenRequested.Task;
        releaseToken.SetResult(GrantTokenReply("access-1", RefreshToken2));
        await Task.WhenAll(first, second);

        Assert.Single(handler.RequestsTo(TokenRoute));
        Assert.Single(store.Saves);
    }

    [Fact]
    public async Task ExchangesAgain60SecondsBeforeTheAccessTokenExpires()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 11, 15, 12, 0, 0, TimeSpan.Zero));
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, GrantTokenReply("access-1", RefreshToken2, expiresIn: 900), GrantTokenReply("access-2", RefreshToken3, expiresIn: 900))
            .Script(LearnerRoute, LearnerReply(), LearnerReply(), LearnerReply());
        var client = NewClient(handler, NewProvider(handler, new RecordingTokenStore(handler), time: time), time);

        await client.Learners.GetAsync(LearnerId);
        time.Advance(TimeSpan.FromSeconds(839));
        await client.Learners.GetAsync(LearnerId);
        time.Advance(TimeSpan.FromSeconds(1));
        await client.Learners.GetAsync(LearnerId);

        Assert.Equal(2, handler.RequestsTo(TokenRoute).Count);
        Assert.Equal(
            ["Bearer access-1", "Bearer access-1", "Bearer access-2"],
            handler.RequestsTo(LearnerRoute).Select(request => request.Header("Authorization")));
    }

    [Fact]
    public async Task A401ExchangesAgainAndReplaysTheRequestOnce()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, GrantTokenReply("access-1", RefreshToken2), GrantTokenReply("access-2", RefreshToken3))
            .Script(LearnerRoute, ProblemReply(401, """{"title":"Unauthorized","status":401,"code":"unauthorized"}"""), LearnerReply());
        var client = NewClient(handler, NewProvider(handler, new RecordingTokenStore(handler)));

        await client.Learners.GetAsync(LearnerId);

        Assert.Equal(
            ["Bearer access-1", "Bearer access-2"],
            handler.RequestsTo(LearnerRoute).Select(request => request.Header("Authorization")));
    }

    [Fact]
    public void ExposesTheBaseUrlItExchangesAt()
    {
        var handler = new FakeHttpMessageHandler();
        var provider = new PartnerGrantTokenProvider(new PartnerGrantTokenProviderOptions
        {
            ClientId = AppId,
            ClientSecret = AppSecret,
            TokenStore = new RecordingTokenStore(handler),
            Environment = TeachMeHipaaEnvironment.Sandbox,
        });

        Assert.Equal(new Uri("https://api-teachmehipaa-dev.web.app/v1"), provider.BaseUrl);
    }

    [Fact]
    public void NeedsATokenStore()
    {
        var error = Assert.Throws<ArgumentException>(() => new PartnerGrantTokenProvider(new PartnerGrantTokenProviderOptions
        {
            ClientId = AppId,
            ClientSecret = AppSecret,
        }));

        Assert.Equal(nameof(PartnerGrantTokenProviderOptions.TokenStore), error.ParamName);
    }
}
