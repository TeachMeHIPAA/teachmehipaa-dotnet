using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class RequestOptionsTests
{
    [Fact]
    public async Task AsksForTheLearnersStatusInTheGivenCourse()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, LearnerReply());
        var client = NewClient(handler);

        await client.Learners.GetAsync(LearnerId, new LearnerGetOptions { CourseId = "hipaa 2026" });

        Assert.Equal("?courseId=hipaa%202026", Assert.Single(handler.RequestsTo(LearnerRoute)).Uri.Query);
    }

    [Fact]
    public async Task SendsTheCallersRequestId()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, LearnerReply());
        var client = NewClient(handler);

        await client.Learners.GetAsync(LearnerId, requestOptions: new RequestOptions { RequestId = "sync-job-42" });

        Assert.Equal("sync-job-42", Assert.Single(handler.RequestsTo(LearnerRoute)).Header("x-request-id"));
    }

    [Fact]
    public async Task EscapesTheLearnerIdInThePath()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script($"GET /v1/organizations/{OrgId}/learners/a%2Fb", LearnerReply());
        var client = NewClient(handler);

        await client.Learners.GetAsync("a/b");

        Assert.Single(handler.RequestsTo($"GET /v1/organizations/{OrgId}/learners/a%2Fb"));
    }

    [Fact]
    public async Task PassesTheCallersCancellationThrough()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, async (_, cancellationToken) =>
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return LearnerReply();
            });
        var client = NewClient(handler);
        using var cancellation = new CancellationTokenSource();

        var call = client.Learners.GetAsync(LearnerId, cancellationToken: cancellation.Token);
        while (handler.RequestsTo(LearnerRoute).Count == 0)
        {
            await Task.Yield();
        }

        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => call);
    }
}
