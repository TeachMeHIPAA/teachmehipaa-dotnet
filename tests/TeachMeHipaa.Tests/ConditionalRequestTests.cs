using System.Net;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

/// <summary>Conditional writes (<c>If-Match</c>) and reads (<c>If-None-Match</c>), through the public client.</summary>
public class ConditionalRequestTests
{
    private const string LearnerPatchRoute = $"PATCH /v1/organizations/{OrgId}/learners/{LearnerId}";
    private const string OffboardRoute = $"POST /v1/organizations/{OrgId}/learners/{LearnerId}:offboard";
    private const string SubscriptionId = "whs_01JD8D";
    private const string SubscriptionGetRoute = $"GET /v1/organizations/{OrgId}/webhook-subscriptions/{SubscriptionId}";
    private const string SubscriptionPatchRoute = $"PATCH /v1/organizations/{OrgId}/webhook-subscriptions/{SubscriptionId}";
    private const string CourseId = "hipaa-2026";
    private const string CourseRoute = $"GET /v1/courses/{CourseId}";

    // Learners carry a quoted SHA-256 hex strong tag (api-sdk-surface.md §5).
    private const string LearnerETag = "\"9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08\"";

    // Rev-based resources carry a quoted revision.
    private const string CourseETag = "\"7\"";

    /// <summary>A <c>dev#/components/schemas/CourseDto</c>.</summary>
    private const string CourseJson = """
        {"courseId":"hipaa-2026","title":"HIPAA Training 2026","courseVersion":"2026.1","passingThreshold":80}
        """;

    private const string SubscriptionJson = $$"""
        {"subscriptionId":"{{SubscriptionId}}","endpointUrl":"https://hr.example.com/hooks/tmh","eventTypes":["learner.certified"],"enabled":false,"description":null,"seatsLowThreshold":5,"secretKeyId":"whk_1","createdAt":"2026-11-01T09:00:00Z","rev":4}
        """;

    // As conditional.core.ts sends them.
    private static readonly ScriptedReply PreconditionFailed = ProblemReply(412, """
        {"type":"https://developers.teachmehipaa.com/errors/precondition_failed","title":"Precondition failed","status":412,"code":"precondition_failed","detail":"The resource has changed since you read it."}
        """);

    private static readonly ScriptedReply PreconditionRequired = ProblemReply(428, """
        {"type":"https://developers.teachmehipaa.com/errors/precondition_required","title":"Precondition required","status":428,"code":"precondition_required","detail":"Send If-Match with the resource's current ETag."}
        """);

    private static FakeHttpMessageHandler Api() => new FakeHttpMessageHandler().Script(TokenRoute, TokenReply());

    private static Dictionary<string, string> ETagHeader(string etag) => new() { ["ETag"] = etag };

    [Fact]
    public async Task SendsTheETagAsIfMatchOnALearnerUpdate()
    {
        var handler = Api().Script(LearnerPatchRoute, LearnerReply(ETagHeader("\"next\"")));
        var client = NewClient(handler);

        var learner = await client.Learners.UpdateAsync(LearnerId, new LearnerUpdateOptions(LearnerETag) { Name = "Ana Example" });

        Assert.Equal("\"next\"", learner.LastResponse?.ETag);
        Assert.Equal(LearnerETag, Assert.Single(handler.RequestsTo(LearnerPatchRoute)).Header("If-Match"));
    }

    [Fact]
    public async Task TakesIfMatchFromTheETagOfALearnerItReturned()
    {
        var handler = Api()
            .Script(LearnerRoute, LearnerReply(ETagHeader(LearnerETag)))
            .Script(LearnerPatchRoute, LearnerReply());
        var client = NewClient(handler);

        var learner = await client.Learners.GetAsync(LearnerId);
        await client.Learners.UpdateAsync(LearnerId, new LearnerUpdateOptions(learner) { ExternalId = null });

        Assert.Equal(LearnerETag, Assert.Single(handler.RequestsTo(LearnerPatchRoute)).Header("If-Match"));
    }

    [Fact]
    public async Task RefusesAnObjectWithoutAnETagBeforeSending()
    {
        var handler = Api().Script(LearnerRoute, LearnerReply());
        var client = NewClient(handler);
        var learner = await client.Learners.GetAsync(LearnerId);

        var error = await Assert.ThrowsAsync<ArgumentException>(
            () => client.Learners.OffboardAsync(LearnerId, new LearnerOffboardOptions { IfMatch = learner }));

        Assert.Contains("ETag", error.Message, StringComparison.Ordinal);
        Assert.Empty(handler.RequestsTo(OffboardRoute));
    }

    [Fact]
    public async Task SendsAnOptionalIfMatchOnlyWhenGiven()
    {
        var handler = Api()
            .Script(OffboardRoute, LearnerReply(), LearnerReply());
        var client = NewClient(handler);

        await client.Learners.OffboardAsync(LearnerId);
        await client.Learners.OffboardAsync(LearnerId, new LearnerOffboardOptions { IfMatch = LearnerETag });

        var requests = handler.RequestsTo(OffboardRoute);
        Assert.Null(requests[0].Header("If-Match"));
        Assert.Equal(LearnerETag, requests[1].Header("If-Match"));
    }

    [Fact]
    public async Task RaisesPreconditionFailedForAStaleETagWithoutRetrying()
    {
        var handler = Api().Script(LearnerPatchRoute, PreconditionFailed);
        var client = NewClient(handler);

        var error = await Assert.ThrowsAsync<PreconditionFailedException>(
            () => client.Learners.UpdateAsync(LearnerId, new LearnerUpdateOptions(LearnerETag) { Name = "Ana Example" }));

        Assert.Equal(HttpStatusCode.PreconditionFailed, error.StatusCode);
        Assert.Equal("precondition_failed", error.Code);
        Assert.Single(handler.RequestsTo(LearnerPatchRoute));
    }

    [Fact]
    public async Task RaisesPreconditionFailedWhenTheApiRequiresAPrecondition()
    {
        var handler = Api().Script(SubscriptionPatchRoute, PreconditionRequired);
        var client = NewClient(handler);

        var error = await Assert.ThrowsAsync<PreconditionFailedException>(
            () => client.WebhookSubscriptions.UpdateAsync(SubscriptionId, new WebhookSubscriptionUpdateOptions("*") { Enabled = false }));

        Assert.Equal((HttpStatusCode)428, error.StatusCode);
        Assert.Equal("precondition_required", error.Code);
    }

    [Fact]
    public async Task TakesIfMatchFromTheETagOfAWebhookSubscriptionItReturned()
    {
        var handler = Api()
            .Script(SubscriptionGetRoute, new ScriptedReply { Json = SubscriptionJson, Headers = ETagHeader("\"4\"") })
            .Script(SubscriptionPatchRoute, new ScriptedReply { Json = SubscriptionJson });
        var client = NewClient(handler);

        var subscription = await client.WebhookSubscriptions.GetAsync(SubscriptionId);
        await client.WebhookSubscriptions.UpdateAsync(SubscriptionId, new WebhookSubscriptionUpdateOptions(subscription) { Enabled = false });

        Assert.Equal("\"4\"", Assert.Single(handler.RequestsTo(SubscriptionPatchRoute)).Header("If-Match"));
    }

    [Fact]
    public async Task ReturnsNotModifiedWhenTheHeldCopyIsCurrent()
    {
        var handler = new FakeHttpMessageHandler().Script(CourseRoute, new ScriptedReply
        {
            Status = HttpStatusCode.NotModified,
            Headers = new Dictionary<string, string> { ["ETag"] = CourseETag, ["x-request-id"] = "req_304" },
        });
        var client = NewClient(handler);

        var result = await client.Courses.GetIfModifiedAsync(CourseId, CourseETag);

        Assert.True(result.IsNotModified);
        Assert.Null(result.Value);
        Assert.Equal(HttpStatusCode.NotModified, result.LastResponse.StatusCode);
        Assert.Equal(CourseETag, result.LastResponse.ETag);
        Assert.Equal("req_304", result.LastResponse.RequestId);
        Assert.Equal(CourseETag, Assert.Single(handler.RequestsTo(CourseRoute)).Header("If-None-Match"));
    }

    [Fact]
    public async Task ReturnsAChangedObjectUsingTheHeldCopysETag()
    {
        var handler = new FakeHttpMessageHandler().Script(
            CourseRoute,
            new ScriptedReply { Json = CourseJson, Headers = ETagHeader(CourseETag) },
            new ScriptedReply { Json = CourseJson.Replace("2026.1", "2026.2", StringComparison.Ordinal), Headers = ETagHeader("\"8\"") });
        var client = NewClient(handler);
        var cached = await client.Courses.GetAsync(CourseId);

        var result = await client.Courses.GetIfModifiedAsync(CourseId, cached);
        var current = result.Value ?? cached;

        Assert.False(result.IsNotModified);
        Assert.Equal("2026.2", current.CourseVersion);
        Assert.Equal("\"8\"", result.LastResponse.ETag);
        Assert.Same(result.LastResponse, current.LastResponse);
        var requests = handler.RequestsTo(CourseRoute);
        Assert.Null(requests[0].Header("If-None-Match"));
        Assert.Equal(CourseETag, requests[1].Header("If-None-Match"));
    }

    [Fact]
    public async Task ReturnsNotModifiedForAWebhookSubscription()
    {
        var handler = Api().Script(SubscriptionGetRoute, new ScriptedReply
        {
            Status = HttpStatusCode.NotModified,
            Headers = ETagHeader("\"4\""),
        });
        var client = NewClient(handler);

        var result = await client.WebhookSubscriptions.GetIfModifiedAsync(SubscriptionId, "\"4\"");

        Assert.True(result.IsNotModified);
        Assert.Equal("\"4\"", result.LastResponse.ETag);
    }

    [Fact]
    public async Task RefusesAnIfNoneMatchObjectWithoutAnETagBeforeSending()
    {
        var handler = new FakeHttpMessageHandler().Script(CourseRoute, new ScriptedReply { Json = CourseJson });
        var client = NewClient(handler);
        var course = await client.Courses.GetAsync(CourseId);

        await Assert.ThrowsAsync<ArgumentException>(() => client.Courses.GetIfModifiedAsync(CourseId, course));

        Assert.Single(handler.RequestsTo(CourseRoute));
    }
}
