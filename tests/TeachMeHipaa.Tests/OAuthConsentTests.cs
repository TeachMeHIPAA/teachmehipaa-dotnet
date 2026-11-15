using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class OAuthConsentTests
{
    private const string RedirectUri = "https://partner.example.com/oauth/callback?tenant=7";
    private const string Challenge = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM";
    private const string Verifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";
    private const string Code = "SplxlOBeZQQYbYS6WxSbIA";

    private static readonly Regex Unreserved43To128 = new("^[A-Za-z0-9._~-]{43,128}$");
    private static readonly Regex Base64Url43 = new("^[A-Za-z0-9_-]{43}$");

    private static AuthorizeUrlOptions Consent(
        string state = "af0ifjsldkj",
        TeachMeHipaaEnvironment environment = TeachMeHipaaEnvironment.Live,
        Uri? authorizationUrl = null) => new()
        {
            ClientId = AppId,
            RedirectUri = new Uri(RedirectUri),
            Scopes = ["learners:read", "learners:write"],
            State = state,
            CodeChallenge = Challenge,
            Environment = environment,
            AuthorizationUrl = authorizationUrl,
        };

    private static ExchangeCodeOptions Exchange(TeachMeHipaaEnvironment environment = TeachMeHipaaEnvironment.Live) => new()
    {
        ClientId = AppId,
        ClientSecret = AppSecret,
        Code = Code,
        RedirectUri = new Uri(RedirectUri),
        CodeVerifier = Verifier,
        Environment = environment,
    };

    /// <summary>RFC 7636 §4.2, computed here rather than by the SDK.</summary>
    private static string S256(string codeVerifier)
    {
        var digest = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Convert.ToBase64String(digest).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static Dictionary<string, string> QueryOf(Uri url) =>
        url.Query.TrimStart('?').Split('&')
            .Select(pair => pair.Split('=', 2))
            .ToDictionary(pair => pair[0], pair => Uri.UnescapeDataString(pair[1]));

    [Fact]
    public void TheIndependentS256MatchesRfc7636AppendixB()
    {
        Assert.Equal(Challenge, S256(Verifier));
    }

    [Fact]
    public void TheCodeChallengeIsTheS256HashOfTheCodeVerifier()
    {
        var pair = TeachMeHipaaOAuth.CreatePkcePair();

        Assert.Equal("S256", pair.Method);
        Assert.Matches(Unreserved43To128, pair.CodeVerifier);
        Assert.Matches(Base64Url43, pair.CodeChallenge);
        Assert.Equal(S256(pair.CodeVerifier), pair.CodeChallenge);
    }

    [Fact]
    public void EveryPkcePairHasAFreshCodeVerifier()
    {
        Assert.NotEqual(TeachMeHipaaOAuth.CreatePkcePair().CodeVerifier, TeachMeHipaaOAuth.CreatePkcePair().CodeVerifier);
    }

    [Fact]
    public void StatesAreHardToGuess()
    {
        var state = TeachMeHipaaOAuth.CreateState();

        Assert.Matches(Base64Url43, state);
        Assert.NotEqual(state, TeachMeHipaaOAuth.CreateState());
    }

    [Fact]
    public void TheConsentUrlCarriesClientRedirectScopesStateAndS256()
    {
        var url = new Uri(TeachMeHipaaOAuth.AuthorizeUrl(Consent()));

        Assert.Equal("https://console.teachmehipaa.com/oauth/authorize", url.GetLeftPart(UriPartial.Path));
        Assert.Equal(
            new Dictionary<string, string>
            {
                ["response_type"] = "code",
                ["client_id"] = AppId,
                ["redirect_uri"] = RedirectUri,
                ["scope"] = "learners:read learners:write",
                ["state"] = "af0ifjsldkj",
                ["code_challenge"] = Challenge,
                ["code_challenge_method"] = "S256",
            },
            QueryOf(url));
    }

    [Fact]
    public void TheConsentUrlEncodesSpacesInTheScopeAsPercent20()
    {
        Assert.Contains("scope=learners%3Aread%20learners%3Awrite", TeachMeHipaaOAuth.AuthorizeUrl(Consent()), StringComparison.Ordinal);
    }

    [Fact]
    public void AConsentUrlNeedsAState()
    {
        var error = Assert.Throws<ArgumentException>(() => TeachMeHipaaOAuth.AuthorizeUrl(Consent(state: "")));

        Assert.Equal(nameof(AuthorizeUrlOptions.State), error.ParamName);
    }

    [Fact]
    public void SandboxConsentNeedsAnExplicitAuthorizationUrl()
    {
        var error = Assert.Throws<ArgumentException>(
            () => TeachMeHipaaOAuth.AuthorizeUrl(Consent(environment: TeachMeHipaaEnvironment.Sandbox)));
        Assert.Equal(nameof(AuthorizeUrlOptions.AuthorizationUrl), error.ParamName);

        var url = new Uri(TeachMeHipaaOAuth.AuthorizeUrl(Consent(
            environment: TeachMeHipaaEnvironment.Sandbox,
            authorizationUrl: new Uri("https://console.example.test/oauth/authorize"))));

        Assert.Equal("https://console.example.test/oauth/authorize", url.GetLeftPart(UriPartial.Path));
        Assert.Equal("S256", QueryOf(url)["code_challenge_method"]);
    }

    [Fact]
    public async Task TheCodeExchangeReturnsTheGrantTokens()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, GrantTokenReply("access-token-org", RefreshToken1, expiresIn: 900));
        using var httpClient = new HttpClient(handler);

        var grant = await TeachMeHipaaOAuth.ExchangeCodeAsync(Exchange(), httpClient);

        Assert.Equal("access-token-org", grant.AccessToken);
        Assert.Equal(RefreshToken1, grant.RefreshToken);
        Assert.Equal(TimeSpan.FromSeconds(900), grant.ExpiresIn);
        Assert.Equal(["learners:read", "learners:write"], grant.Scopes);
        var exchange = Assert.Single(handler.Requests);
        Assert.Equal("https://api.tme.so/v1/auth/token", exchange.Uri.AbsoluteUri);
        Assert.Equal($"Basic {AppBasicCredentials}", exchange.Header("Authorization"));
        Assert.Equal("application/x-www-form-urlencoded", exchange.Header("Content-Type"));
        Assert.Equal(
            "grant_type=authorization_code&code=SplxlOBeZQQYbYS6WxSbIA"
                + "&redirect_uri=https%3A%2F%2Fpartner.example.com%2Foauth%2Fcallback%3Ftenant%3D7"
                + $"&code_verifier={Verifier}",
            exchange.Body);
    }

    [Fact]
    public void GrantTokensKeepTheirSecretsOutOfToString()
    {
        var grant = new GrantTokens
        {
            AccessToken = "access-token-org",
            RefreshToken = RefreshToken1,
            ExpiresIn = TimeSpan.FromSeconds(900),
            Scopes = [],
        };

        Assert.DoesNotContain("access-token-org", grant.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(RefreshToken1, grant.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheCodeExchangeGoesToTheSandboxTokenEndpointWhenAsked()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, GrantTokenReply("access-token-org", RefreshToken1));
        using var httpClient = new HttpClient(handler);

        await TeachMeHipaaOAuth.ExchangeCodeAsync(Exchange(TeachMeHipaaEnvironment.Sandbox), httpClient);

        Assert.Equal("https://api-teachmehipaa-dev.web.app/v1/auth/token", Assert.Single(handler.Requests).Uri.AbsoluteUri);
    }

    [Fact]
    public async Task ARefusedCodeRaisesAnOAuthException()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, OAuthErrorReply("invalid_grant", "The code has expired or was used"));
        using var httpClient = new HttpClient(handler);

        var error = await Assert.ThrowsAsync<OAuthException>(() => TeachMeHipaaOAuth.ExchangeCodeAsync(Exchange(), httpClient));

        Assert.Equal("invalid_grant", error.Code);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, error.StatusCode);
        Assert.Contains("The code has expired or was used", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ACodeExchangeThatLosesTheNetworkIsNotRetriedAndSaysToRerunConsent()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, new ScriptedReply { Throws = new HttpRequestException("connection reset") });
        using var httpClient = new HttpClient(handler);

        var error = await Assert.ThrowsAsync<ApiConnectionException>(() => TeachMeHipaaOAuth.ExchangeCodeAsync(Exchange(), httpClient));

        Assert.Single(handler.Requests);
        Assert.Contains("may have been consumed", error.Message, StringComparison.Ordinal);
        Assert.Contains("Re-run consent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ACodeExchangeAnsweredWithA503IsNotRetried()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, new ScriptedReply { Status = System.Net.HttpStatusCode.ServiceUnavailable, Json = "{}" });
        using var httpClient = new HttpClient(handler);

        await Assert.ThrowsAsync<ApiException>(() => TeachMeHipaaOAuth.ExchangeCodeAsync(Exchange(), httpClient));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task ACodeExchangeNeedsTheCodeVerifier()
    {
        var options = new ExchangeCodeOptions
        {
            ClientId = AppId,
            ClientSecret = AppSecret,
            Code = Code,
            RedirectUri = new Uri(RedirectUri),
        };

        var error = await Assert.ThrowsAsync<ArgumentException>(() => TeachMeHipaaOAuth.ExchangeCodeAsync(options));

        Assert.Equal(nameof(ExchangeCodeOptions.CodeVerifier), error.ParamName);
    }
}
