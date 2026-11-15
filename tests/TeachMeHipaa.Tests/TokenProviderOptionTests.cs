using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class TokenProviderOptionTests
{
    private const string Unauthorized = """{"title":"Unauthorized","status":401,"code":"unauthorized"}""";

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
    public void WithoutATokenProviderBothCredentialsAreStillRequired()
    {
        var error = Assert.Throws<ArgumentException>(() => new TeachMeHipaaClient(new TeachMeHipaaClientOptions { ClientId = ClientId }));

        Assert.Equal(nameof(TeachMeHipaaClientOptions.ClientSecret), error.ParamName);
    }
}
