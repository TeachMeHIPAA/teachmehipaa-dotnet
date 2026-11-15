using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class LearnerGetTests
{
    [Fact]
    public async Task RetrievesALearnerWithAClientCredentialsAccessToken()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply("access-token-1"))
            .Script(LearnerRoute, LearnerReply());
        var client = NewClient(handler);

        var learner = await client.Learners.GetAsync(LearnerId);

        Assert.Equal(LearnerId, learner.LearnerId);
        Assert.Equal("ana@example.com", learner.Email);
        Assert.Equal("Ana Example", learner.Name);
        Assert.Equal("hr-123", learner.ExternalId);
        Assert.Equal(LearnerStatus.InProgress, learner.Status);
        Assert.Equal("hipaa-2026", learner.Course.CourseId);
        Assert.Equal("HIPAA Training 2026", learner.Course.Title);
        Assert.Equal(new DateTimeOffset(2026, 11, 1, 9, 0, 0, TimeSpan.Zero), learner.InvitedAt);
        Assert.Equal(new DateTimeOffset(2026, 11, 2, 10, 30, 0, TimeSpan.Zero), learner.StartedAt);
        Assert.Null(learner.CompletedAt);
        Assert.Equal(new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.Zero), learner.DueAt);
        Assert.Null(learner.Certificate);

        var token = Assert.Single(handler.RequestsTo(TokenRoute));
        Assert.Equal(
            "Basic Y2xpXzAxSkQ3WjNNMVY0UThUMks1TjlSNlcwWFlaOnRtaF9jc19jMlZqY21WMExYTmxZM0psZEMxelpXTnlaWFF0YzJWamNtVjBMWE0=",
            token.Header("Authorization"));
        Assert.StartsWith("application/x-www-form-urlencoded", token.Header("Content-Type"));
        Assert.Equal("grant_type=client_credentials", token.Body);

        var get = Assert.Single(handler.RequestsTo(LearnerRoute));
        Assert.Equal("Bearer access-token-1", get.Header("Authorization"));
        Assert.Equal("application/json", get.Header("Accept"));
    }
}
