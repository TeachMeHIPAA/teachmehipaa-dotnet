using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class ClientConfigurationTests
{
    private static async Task<IReadOnlyList<string>> UrlsSentBy(TeachMeHipaaClientOptions options)
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, LearnerReply());
        var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            OrganizationId = OrgId,
            HttpMessageHandler = handler,
            Environment = options.Environment,
            BaseUrl = options.BaseUrl,
        });
        await client.Learners.GetAsync(LearnerId);
        return handler.Requests.Select(request => request.Uri.AbsoluteUri).ToArray();
    }

    [Fact]
    public async Task TalksToTheLiveEnvironmentByDefault()
    {
        Assert.Equal(
            ["https://api.tme.so/v1/auth/token", $"https://api.tme.so/v1/organizations/{OrgId}/learners/{LearnerId}"],
            await UrlsSentBy(new TeachMeHipaaClientOptions()));
    }

    [Fact]
    public async Task TalksToTheSandboxEnvironmentWhenAsked()
    {
        Assert.Equal(
            [
                "https://sandbox.api.tme.so/v1/auth/token",
                $"https://sandbox.api.tme.so/v1/organizations/{OrgId}/learners/{LearnerId}",
            ],
            await UrlsSentBy(new TeachMeHipaaClientOptions { Environment = TeachMeHipaaEnvironment.Sandbox }));
    }

    [Fact]
    public async Task PrefersAnExplicitBaseUrlOverTheEnvironment()
    {
        Assert.Equal(
            ["http://localhost:4010/v1/auth/token", $"http://localhost:4010/v1/organizations/{OrgId}/learners/{LearnerId}"],
            await UrlsSentBy(new TeachMeHipaaClientOptions
            {
                Environment = TeachMeHipaaEnvironment.Sandbox,
                BaseUrl = new Uri("http://localhost:4010/v1/"),
            }));
    }

    [Fact]
    public async Task IgnoresChangesToTheScopesListAfterConstruction()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, LearnerReply());
        var scopes = new List<string> { "learners:read" };
        var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            OrganizationId = OrgId,
            Scopes = scopes,
            HttpMessageHandler = handler,
        });

        scopes.Add("learners:write");
        await client.Learners.GetAsync(LearnerId);

        Assert.Equal(
            "grant_type=client_credentials&scope=learners%3Aread",
            Assert.Single(handler.RequestsTo(TokenRoute)).Body);
    }

    [Fact]
    public async Task SendsRequestsThroughACallersHttpClient()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, LearnerReply());
        using var httpClient = new HttpClient(handler);
        var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            OrganizationId = OrgId,
            HttpClient = httpClient,
        });

        var learner = await client.Learners.GetAsync(LearnerId);

        Assert.Equal(LearnerId, learner.LearnerId);
        Assert.Equal([TokenRoute, LearnerRoute], handler.Requests.Select(request => request.Route));
    }

    public static TheoryData<string, TeachMeHipaaClientOptions> InvalidOptions => new()
    {
        { "ClientId", new TeachMeHipaaClientOptions { ClientSecret = ClientSecret } },
        { "ClientSecret", new TeachMeHipaaClientOptions { ClientId = ClientId, ClientSecret = " " } },
        { "Environment", new TeachMeHipaaClientOptions { ClientId = ClientId, ClientSecret = ClientSecret, Environment = (TeachMeHipaaEnvironment)7 } },
        { "BaseUrl", new TeachMeHipaaClientOptions { ClientId = ClientId, ClientSecret = ClientSecret, BaseUrl = new Uri("api.tme.so", UriKind.Relative) } },
        { "OrganizationId", new TeachMeHipaaClientOptions { ClientId = ClientId, ClientSecret = ClientSecret, OrganizationId = "" } },
        { "Scopes", new TeachMeHipaaClientOptions { ClientId = ClientId, ClientSecret = ClientSecret, Scopes = ["learners:read", ""] } },
        { "MaxRetries", new TeachMeHipaaClientOptions { ClientId = ClientId, ClientSecret = ClientSecret, MaxRetries = -1 } },
        { "Timeout", new TeachMeHipaaClientOptions { ClientId = ClientId, ClientSecret = ClientSecret, Timeout = TimeSpan.Zero } },
        { "AppInfo", new TeachMeHipaaClientOptions { ClientId = ClientId, ClientSecret = ClientSecret, AppInfo = new AppInfo("") } },
        { "HttpClient", new TeachMeHipaaClientOptions { ClientId = ClientId, ClientSecret = ClientSecret, HttpClient = new HttpClient(), HttpMessageHandler = new FakeHttpMessageHandler() } },
    };

    [Theory]
    [MemberData(nameof(InvalidOptions))]
    public void RejectsAnInvalidOptionWhenConstructed(string option, TeachMeHipaaClientOptions options)
    {
        var error = Assert.ThrowsAny<ArgumentException>(() => new TeachMeHipaaClient(options));

        Assert.Equal(option, error.ParamName);
    }
}
