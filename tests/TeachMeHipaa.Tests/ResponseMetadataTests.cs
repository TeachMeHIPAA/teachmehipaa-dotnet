using System.Net;
using System.Text.Json;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class ResponseMetadataTests
{
    private const string ETag = "\"5d41402abc4b2a76b9719d911017c592ae0f3c1b6e4a3b0f9d8e7c6a5b4c3d2e\"";
    private const string RequestId = "1f0c6a2e-7b5d-4c3a-9e8f-0a1b2c3d4e5f";

    [Fact]
    public async Task ExposesStatusHeadersRequestIdAndETagOnTheReturnedLearner()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, LearnerReply(new Dictionary<string, string>
            {
                ["ETag"] = ETag,
                ["x-request-id"] = RequestId,
            }));
        var client = NewClient(handler);

        var learner = await client.Learners.GetAsync(LearnerId);

        var response = Assert.IsType<TeachMeHipaaResponse>(learner.LastResponse);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(RequestId, response.RequestId);
        Assert.Equal(ETag, response.ETag);
        Assert.Equal(ETag, response.Headers["etag"]);
        Assert.Equal(RequestId, response.Headers["X-Request-Id"]);
        Assert.Equal("application/json", response.Headers["content-type"]);
    }

    [Fact]
    public async Task LeavesTheETagNullWhenTheResponseHasNone()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, LearnerReply());
        var client = NewClient(handler);

        var learner = await client.Learners.GetAsync(LearnerId);

        Assert.Null(learner.LastResponse?.ETag);
        Assert.Null(learner.LastResponse?.RequestId);
    }

    [Fact]
    public async Task KeepsTheResponseOutOfTheLearnersSerializedFields()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, LearnerReply(new Dictionary<string, string> { ["x-request-id"] = RequestId }));
        var client = NewClient(handler);

        var learner = await client.Learners.GetAsync(LearnerId);

        using var serialized = JsonDocument.Parse(JsonSerializer.Serialize(learner, LearnerJsonOptions));
        using var sent = JsonDocument.Parse(LearnerJson);
        Assert.Equal(
            sent.RootElement.EnumerateObject().Select(property => property.Name),
            serialized.RootElement.EnumerateObject().Select(property => property.Name));
    }

    private static readonly JsonSerializerOptions LearnerJsonOptions = new()
    {
        TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver(),
    };
}
