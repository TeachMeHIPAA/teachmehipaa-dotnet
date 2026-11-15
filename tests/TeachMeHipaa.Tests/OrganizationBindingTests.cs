using System.Net;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class OrganizationBindingTests
{
    [Fact]
    public async Task ResolvesTheOrganizationOnceAndReusesIt()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply("jwt-1"))
            .Script(IntrospectionRoute, IntrospectionReply())
            .Script(LearnerRoute, LearnerReply(), LearnerReply());
        var client = NewClient(handler, organizationId: null);

        await client.Learners.GetAsync(LearnerId);
        await client.Learners.GetAsync(LearnerId);

        Assert.Equal("Bearer jwt-1", Assert.Single(handler.RequestsTo(IntrospectionRoute)).Header("Authorization"));
        Assert.Equal(2, handler.RequestsTo(LearnerRoute).Count);
    }

    [Fact]
    public async Task SharesOneIntrospectionBetweenConcurrentFirstCalls()
    {
        var releaseIntrospection = new TaskCompletionSource<ScriptedReply>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(IntrospectionRoute, (_, _) => releaseIntrospection.Task)
            .Script(LearnerRoute, LearnerReply(), LearnerReply());
        var client = NewClient(handler, organizationId: null);

        var first = client.Learners.GetAsync(LearnerId);
        var second = client.Learners.GetAsync(LearnerId);
        releaseIntrospection.SetResult(IntrospectionReply());
        await Task.WhenAll(first, second);

        Assert.Single(handler.RequestsTo(IntrospectionRoute));
        Assert.Equal(2, handler.RequestsTo(LearnerRoute).Count);
    }

    [Fact]
    public async Task UsesAnExplicitOrganizationIdWithoutIntrospection()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script($"GET /v1/organizations/org_given/learners/{LearnerId}", LearnerReply());
        var client = NewClient(handler, organizationId: "org_given");

        await client.Learners.GetAsync(LearnerId);

        Assert.Empty(handler.RequestsTo(IntrospectionRoute));
    }

    [Fact]
    public async Task LooksTheOrganizationUpAgainAfterAFailedIntrospection()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(
                IntrospectionRoute,
                ProblemReply(503, """{"title":"Service Unavailable","status":503,"code":"temporarily_unavailable"}"""),
                IntrospectionReply())
            .Script(LearnerRoute, LearnerReply());
        var client = NewClient(handler, organizationId: null, maxRetries: 0);

        var error = await Assert.ThrowsAsync<ApiException>(() => client.Learners.GetAsync(LearnerId));
        var learner = await client.Learners.GetAsync(LearnerId);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, error.StatusCode);
        Assert.Equal("temporarily_unavailable", error.Code);
        Assert.Equal(LearnerId, learner.LearnerId);
        Assert.Equal(2, handler.RequestsTo(IntrospectionRoute).Count);
    }

    [Fact]
    public async Task ExplainsThatOrganizationIdIsNeededWhenTheAccessTokenHasNoOrganization()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(IntrospectionRoute, IntrospectionReply(orgId: null));
        var client = NewClient(handler, organizationId: null);

        var error = await Assert.ThrowsAsync<TeachMeHipaaException>(() => client.Learners.GetAsync(LearnerId));

        Assert.Contains("OrganizationId", error.Message, StringComparison.Ordinal);
        Assert.Empty(handler.RequestsTo(LearnerRoute));
    }
}
