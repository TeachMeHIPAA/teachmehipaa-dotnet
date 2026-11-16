using Microsoft.Extensions.DependencyInjection;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class DependencyInjectionTests
{
    private sealed record Settings(string OrganizationId);

    /// <summary>Counts the requests that pass through the IHttpClientFactory pipeline.</summary>
    private sealed class CountingHandler : DelegatingHandler
    {
        public int Count { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Count++;
            return base.SendAsync(request, cancellationToken);
        }
    }

    private static TeachMeHipaaClientOptions Credentials(string organizationId = OrgId) => new()
    {
        ClientId = ClientId,
        ClientSecret = ClientSecret,
        OrganizationId = organizationId,
    };

    [Fact]
    public async Task RegistersOneSharedClientThatSendsThroughTheFactorysHttpClient()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, LearnerReply(), LearnerReply());
        var services = new ServiceCollection();
        services.AddTeachMeHipaa(_ => Credentials()).ConfigurePrimaryHttpMessageHandler(() => handler);
        using var provider = services.BuildServiceProvider();

        var first = provider.GetRequiredService<TeachMeHipaaClient>();
        var second = provider.GetRequiredService<TeachMeHipaaClient>();
        await first.Learners.GetAsync(LearnerId);
        await second.Learners.GetAsync(LearnerId);

        Assert.Same(first, second);
        // One client, so one token for both calls.
        Assert.Equal([TokenRoute, LearnerRoute, LearnerRoute], handler.Requests.Select(request => request.Route));
    }

    [Fact]
    public async Task BuildsTheOptionsFromTheServiceProvider()
    {
        var route = $"GET /v1/organizations/org_from_settings/learners/{LearnerId}";
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(route, LearnerReply());
        var services = new ServiceCollection();
        services.AddSingleton(new Settings("org_from_settings"));
        services.AddTeachMeHipaa(provider => Credentials(provider.GetRequiredService<Settings>().OrganizationId))
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        using var provider = services.BuildServiceProvider();

        await provider.GetRequiredService<TeachMeHipaaClient>().Learners.GetAsync(LearnerId);

        Assert.Single(handler.RequestsTo(route));
    }

    [Fact]
    public async Task SendsEveryRequestThroughHandlersAddedToTheReturnedBuilder()
    {
        var counting = new CountingHandler();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, LearnerReply());
        var services = new ServiceCollection();
        services.AddTeachMeHipaa(_ => Credentials())
            .AddHttpMessageHandler(() => counting)
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        using var provider = services.BuildServiceProvider();

        await provider.GetRequiredService<TeachMeHipaaClient>().Learners.GetAsync(LearnerId);

        Assert.Equal(2, counting.Count);
    }

    [Fact]
    public void RefusesOptionsThatBringTheirOwnTransport()
    {
        var services = new ServiceCollection();
        services.AddTeachMeHipaa(_ => new TeachMeHipaaClientOptions
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            HttpMessageHandler = new FakeHttpMessageHandler(),
        });
        using var provider = services.BuildServiceProvider();

        var error = Assert.Throws<ArgumentException>(() => provider.GetRequiredService<TeachMeHipaaClient>());

        Assert.Equal(nameof(TeachMeHipaaClientOptions.HttpMessageHandler), error.ParamName);
    }
}
