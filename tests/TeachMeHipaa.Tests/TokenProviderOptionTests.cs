using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class TokenProviderOptionTests
{
    private const string PartnerId = "ptn_01JD82ZX4C5V6B7N8M9Q0W1E2R";
    private const string Unauthorized = """{"title":"Unauthorized","status":401,"code":"unauthorized"}""";

    private sealed class EmptyTokenStore : ITokenStore
    {
        public ValueTask<string> LoadAsync(CancellationToken cancellationToken) => new(string.Empty);

        public ValueTask SaveAsync(string refreshToken, CancellationToken cancellationToken) => default;
    }

    private static PartnerGrantTokenProvider PartnerGrant(TeachMeHipaaEnvironment environment = TeachMeHipaaEnvironment.Live, Uri? baseUrl = null) =>
        new(new PartnerGrantTokenProviderOptions
        {
            ClientId = AppId,
            ClientSecret = AppSecret,
            TokenStore = new EmptyTokenStore(),
            Environment = environment,
            BaseUrl = baseUrl,
        });

    [Fact]
    public async Task AStaticTokenIsSentAsIsAndTheTokenEndpointIsNeverCalled()
    {
        var handler = new FakeHttpMessageHandler().Script(LearnerRoute, LearnerReply(), LearnerReply());
        var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            TokenProvider = new StaticTokenProvider("my-own-token"),
            OrganizationId = OrgId,
            HttpMessageHandler = handler,
        });

        await client.Learners.GetAsync(LearnerId);
        await client.Learners.GetAsync(LearnerId);

        Assert.Empty(handler.RequestsTo(TokenRoute));
        Assert.Equal(
            ["Bearer my-own-token", "Bearer my-own-token"],
            handler.Requests.Select(request => request.Header("Authorization")));
    }

    [Fact]
    public async Task AStaticTokenThatIsRefusedRaisesTheFirst401WithoutResending()
    {
        var handler = new FakeHttpMessageHandler().Script(LearnerRoute, ProblemReply(401, Unauthorized));
        var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            TokenProvider = new StaticTokenProvider("expired-token"),
            OrganizationId = OrgId,
            HttpMessageHandler = handler,
        });

        var error = await Assert.ThrowsAsync<AuthenticationException>(() => client.Learners.GetAsync(LearnerId));

        Assert.Equal("unauthorized", error.Code);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public void AStaticTokenMustNotBeEmpty()
    {
        Assert.Throws<ArgumentException>(() => new StaticTokenProvider(" "));
    }

    [Fact]
    public async Task APartnerLevelTokenCallsPartnersWithoutLookingUpAnOrganization()
    {
        var handler = new FakeHttpMessageHandler().Script(
            $"GET /v1/partners/{PartnerId}",
            new ScriptedReply
            {
                Json = $$"""{"partnerId":"{{PartnerId}}","name":"Acme Resellers","appId":"{{AppId}}","status":"active","maxChildOrgs":50,"childOrgCount":3,"createdAt":"2026-11-01T09:00:00.000Z"}""",
            });
        var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            TokenProvider = new StaticTokenProvider("partner-level-token"),
            HttpMessageHandler = handler,
        });

        await client.Partners.GetAsync(PartnerId);

        Assert.Equal([$"GET /v1/partners/{PartnerId}"], handler.Requests.Select(request => request.Route));
    }

    [Theory]
    [InlineData(nameof(TeachMeHipaaClientOptions.ClientId))]
    [InlineData(nameof(TeachMeHipaaClientOptions.ClientSecret))]
    [InlineData(nameof(TeachMeHipaaClientOptions.Scopes))]
    public void ATokenProviderCannotBeCombinedWithCredentialsOrScopes(string conflicting)
    {
        var options = new TeachMeHipaaClientOptions
        {
            TokenProvider = new StaticTokenProvider("my-own-token"),
            ClientId = conflicting == nameof(TeachMeHipaaClientOptions.ClientId) ? ClientId : string.Empty,
            ClientSecret = conflicting == nameof(TeachMeHipaaClientOptions.ClientSecret) ? ClientSecret : string.Empty,
            Scopes = conflicting == nameof(TeachMeHipaaClientOptions.Scopes) ? ["learners:read"] : null,
        };

        var error = Assert.Throws<ArgumentException>(() => new TeachMeHipaaClient(options));

        Assert.Equal(conflicting, error.ParamName);
        Assert.Contains("TokenProvider", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WithoutATokenProviderTheCredentialsAreStillRequired()
    {
        var error = Assert.Throws<ArgumentException>(() => new TeachMeHipaaClient(new TeachMeHipaaClientOptions()));

        Assert.Equal(nameof(TeachMeHipaaClientOptions.ClientId), error.ParamName);
    }

    [Fact]
    public void APartnerGrantFromAnotherEnvironmentIsRefused()
    {
        var error = Assert.Throws<ArgumentException>(() => new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            TokenProvider = PartnerGrant(TeachMeHipaaEnvironment.Sandbox),
        }));

        Assert.Equal(nameof(TeachMeHipaaClientOptions.TokenProvider), error.ParamName);
        Assert.Contains("https://api-teachmehipaa-dev.web.app/v1", error.Message, StringComparison.Ordinal);
        Assert.Contains("https://api.tme.so/v1", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APartnerGrantOnTheSameBaseUrlIsAcceptedWhateverTheTrailingSlash()
    {
        var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            TokenProvider = PartnerGrant(baseUrl: new Uri("http://localhost:4010/v1/")),
            BaseUrl = new Uri("http://localhost:4010/v1"),
        });

        Assert.NotNull(client);
    }
}
