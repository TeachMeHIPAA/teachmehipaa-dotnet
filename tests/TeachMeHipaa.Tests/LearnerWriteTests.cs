using System.Net;
using System.Text.Json.Nodes;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

/// <summary>
/// Updates as merge patches, and the write results of Learner create and
/// upsert, through the public client.
/// </summary>
public class LearnerWriteTests
{
    private const string LearnerPatchRoute = $"PATCH /v1/organizations/{OrgId}/learners/{LearnerId}";
    private const string CreateRoute = $"POST /v1/organizations/{OrgId}/learners";
    private const string UpsertRoute = $"PUT /v1/organizations/{OrgId}/learners/by-external-id/hr-123";
    private const string SubscriptionId = "whs_01JD8D";
    private const string SubscriptionPatchRoute = $"PATCH /v1/organizations/{OrgId}/webhook-subscriptions/{SubscriptionId}";
    private const string ETag = "\"9f86d081\"";

    /// <summary>A <c>dev#/components/schemas/LearnerCreated</c> with <c>delivery: "none"</c>.</summary>
    private const string CreatedJson = $$"""
        {
          "learnerId": "{{LearnerId}}", "email": "ana@example.com", "name": "Ana Example", "externalId": "hr-123",
          "status": "not_started", "course": { "courseId": "hipaa-2026", "title": "HIPAA Training 2026" },
          "invitedAt": "2026-11-01T09:00:00.000Z", "startedAt": null, "completedAt": null,
          "dueAt": "2026-12-01T00:00:00.000Z", "certificate": null,
          "delivery": "none", "joinUrl": "https://tme.so/j/7QX2"
        }
        """;

    private const string SubscriptionJson = $$"""
        {"subscriptionId":"{{SubscriptionId}}","endpointUrl":"https://hr.example.com/hooks/tmh","eventTypes":["learner.certified"],"enabled":true,"description":null,"seatsLowThreshold":5,"secretKeyId":"whk_1","createdAt":"2026-11-01T09:00:00Z","rev":4}
        """;

    private static FakeHttpMessageHandler Api() => new FakeHttpMessageHandler().Script(TokenRoute, TokenReply());

    private static JsonNode? Sent(RecordedRequest request) => JsonNode.Parse(request.Body!);

    [Fact]
    public async Task LeavesUnsetFieldsOutOfALearnerUpdateAndSendsNullToClear()
    {
        var handler = Api().Script(LearnerPatchRoute, new ScriptedReply { Json = LearnerJson.Replace("\"hr-123\"", "null", StringComparison.Ordinal) });
        var client = NewClient(handler);

        var learner = await client.Learners.UpdateAsync(LearnerId, new LearnerUpdateOptions(ETag) { ExternalId = null });

        Assert.Null(learner.ExternalId);
        var request = Assert.Single(handler.RequestsTo(LearnerPatchRoute));
        Assert.Equal("application/merge-patch+json", request.Header("Content-Type"));
        Assert.Equal("""{"externalId":null}""", request.Body);
    }

    [Fact]
    public async Task KeepsTheCourseQueryOutOfALearnerPatch()
    {
        var handler = Api().Script(LearnerPatchRoute, LearnerReply());
        var client = NewClient(handler);

        await client.Learners.UpdateAsync(
            LearnerId,
            new LearnerUpdateOptions(ETag) { Name = "Ana B. Example", CourseId = "hipaa-2026" });

        var request = Assert.Single(handler.RequestsTo(LearnerPatchRoute));
        Assert.Equal("?courseId=hipaa-2026", request.Uri.Query);
        Assert.Equal("""{"name":"Ana B. Example"}""", request.Body);
    }

    [Fact]
    public async Task SendsOnlyTheWebhookSubscriptionFieldsThatWereSet()
    {
        var handler = Api().Script(SubscriptionPatchRoute, new ScriptedReply { Json = SubscriptionJson });
        var client = NewClient(handler);

        await client.WebhookSubscriptions.UpdateAsync(
            SubscriptionId,
            new WebhookSubscriptionUpdateOptions("\"4\"") { Description = null, EventTypes = new[] { EventType.LearnerCertified }, SeatsLowThreshold = 0 });

        var request = Assert.Single(handler.RequestsTo(SubscriptionPatchRoute));
        Assert.Equal("application/merge-patch+json", request.Header("Content-Type"));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("""{"description":null,"eventTypes":["learner.certified"],"seatsLowThreshold":0}"""),
            Sent(request)));
    }

    [Fact]
    public async Task SendsAnEmptyPatchWhenNothingWasSet()
    {
        var handler = Api().Script(SubscriptionPatchRoute, new ScriptedReply { Json = SubscriptionJson });
        var client = NewClient(handler);

        await client.WebhookSubscriptions.UpdateAsync(SubscriptionId, new WebhookSubscriptionUpdateOptions("\"4\""));

        Assert.Equal("{}", Assert.Single(handler.RequestsTo(SubscriptionPatchRoute)).Body);
    }

    [Fact]
    public async Task ReportsALearnerItCreatedWithItsDeliveryDetails()
    {
        var handler = Api().Script(CreateRoute, new ScriptedReply
        {
            Status = HttpStatusCode.Created,
            Json = CreatedJson,
            Headers = new Dictionary<string, string> { ["ETag"] = ETag },
        });
        var client = NewClient(handler);

        var result = await client.Learners.CreateAsync(new LearnerCreateOptions { Email = "ana@example.com", Delivery = LearnerCreateRequestDelivery.None });

        Assert.True(result.Created);
        Assert.Equal(LearnerCreatedDelivery.None, result.Delivery);
        Assert.Equal("https://tme.so/j/7QX2", result.JoinUrl);
        Assert.Equal(ETag, result.LastResponse?.ETag);
        var request = Assert.Single(handler.RequestsTo(CreateRoute));
        Assert.Equal("""{"email":"ana@example.com","delivery":"none"}""", request.Body);
        Assert.NotNull(request.Header("Idempotency-Key"));
    }

    [Fact]
    public async Task ReportsALearnerThatWasAlreadyThereAsNotCreatedWithoutDeliveryDetails()
    {
        var handler = Api().Script(CreateRoute, LearnerReply());
        var client = NewClient(handler);

        var result = await client.Learners.CreateAsync(new LearnerCreateOptions { Email = "ana@example.com" });

        Assert.False(result.Created);
        Assert.Equal(LearnerId, result.LearnerId);
        Assert.Null(result.Delivery);
        Assert.Null(result.JoinUrl);
    }

    [Fact]
    public async Task TellsAReplayedUpsertApartFromTheFirst()
    {
        var handler = Api().Script(
            UpsertRoute,
            new ScriptedReply { Status = HttpStatusCode.Created, Json = CreatedJson },
            LearnerReply());
        var client = NewClient(handler);
        var options = new LearnerUpsertByExternalIdOptions { Email = "ana@example.com", Delivery = LearnerUpsertRequestDelivery.None };

        var first = await client.Learners.UpsertByExternalIdAsync("hr-123", options);
        var replay = await client.Learners.UpsertByExternalIdAsync("hr-123", options);

        Assert.True(first.Created);
        Assert.Equal("https://tme.so/j/7QX2", first.JoinUrl);
        Assert.False(replay.Created);
        Assert.Null(replay.JoinUrl);
    }
}
