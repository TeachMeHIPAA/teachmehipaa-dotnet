using System.Net;
using System.Text.Json.Nodes;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

/// <summary>Representative generated methods, through the public client and the fake transport.</summary>
public class GeneratedServiceTests
{
    private const string EnrollmentId = "enr_01JD9";

    /// <summary>A <c>dev#/components/schemas/Enrollment</c>.</summary>
    private const string EnrollmentJson = $$"""
        {
          "enrollmentId": "{{EnrollmentId}}", "subjectId": "{{LearnerId}}", "courseId": "hipaa-2026", "orgId": "{{OrgId}}",
          "status": "CANCELLED", "effectiveStatus": "CANCELLED", "archived": false, "cycle": 1, "rev": 4,
          "invitedAt": "2026-11-01T09:00:00.000Z", "startedAt": null, "completedAt": null,
          "certifiedAt": null, "expiresAt": null, "certificateId": null
        }
        """;

    [Fact]
    public async Task CancelsAnEnrollmentWithACustomMethodAndAGeneratedIdempotencyKey()
    {
        var route = $"POST /v1/organizations/{OrgId}/enrollments/{EnrollmentId}:cancel";
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(route, new ScriptedReply { Json = EnrollmentJson });
        var client = NewClient(handler);

        var cancelled = await client.Enrollments.CancelAsync(EnrollmentId, new EnrollmentCancelOptions { Refund = false });

        var enrollment = Assert.IsType<Enrollment>(cancelled);
        Assert.Equal(EnrollmentStatus.Cancelled, enrollment.Status);
        Assert.Equal(HttpStatusCode.OK, enrollment.LastResponse?.StatusCode);
        var request = Assert.Single(handler.RequestsTo(route));
        Assert.Equal(36, request.Header("Idempotency-Key")?.Length);
        Assert.Equal("""{"refund":false}""", request.Body);
    }

    [Fact]
    public async Task ReadsARefundedCancellationByItsRefundField()
    {
        var route = $"POST /v1/organizations/{OrgId}/enrollments/{EnrollmentId}:cancel";
        var json = EnrollmentJson.TrimEnd()[..^1] + """, "refund": {"refundId": "re_1", "amountCents": 4900, "currency": "usd"}}""";
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(route, new ScriptedReply { Json = json });
        var client = NewClient(handler);

        var cancelled = await client.Enrollments.CancelAsync(EnrollmentId);

        var refunded = Assert.IsType<EnrollmentCancelRefunded>(cancelled);
        Assert.Equal(4900, refunded.Refund.AmountCents);
        Assert.Null(Assert.Single(handler.RequestsTo(route)).Body);
    }

    [Fact]
    public async Task ListsAPartnersOrganizationsWithoutLookingUpAnOrganization()
    {
        const string route = "GET /v1/partners/ptn_01JD9/organizations";
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(route, new ScriptedReply
            {
                Json = """
                    {"items":[{"orgId":"org_beta","name":"Beta Dental","createdAt":"2026-11-02T10:00:00.000Z","grantActive":true}],"nextCursor":"cur_2"}
                    """,
            });
        var client = NewClient(handler, organizationId: null);

        var page = await client.Partners.Organizations.ListAsync("ptn_01JD9", new PartnerOrganizationListOptions { Limit = 1 });

        var organization = Assert.Single(page.Items);
        Assert.Equal("Beta Dental", organization.Name);
        Assert.Equal(new DateTimeOffset(2026, 11, 2, 10, 0, 0, TimeSpan.Zero), organization.CreatedAt);
        Assert.Equal("cur_2", page.NextCursor);
        Assert.Equal(HttpStatusCode.OK, page.LastResponse?.StatusCode);
        Assert.Equal("?limit=1", Assert.Single(handler.RequestsTo(route)).Uri.Query);
        Assert.Empty(handler.RequestsTo(IntrospectionRoute));
    }

    [Fact]
    public async Task UpdatesAWebhookSubscriptionWithAMergePatchAndIfMatch()
    {
        var route = $"PATCH /v1/organizations/{OrgId}/webhook-subscriptions/whs_01JD9";
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(route, new ScriptedReply
            {
                Json = """
                    {"subscriptionId":"whs_01JD9","endpointUrl":"https://hooks.example.com/tmh","eventTypes":["learner.invited"],"enabled":false,"description":null,"seatsLowThreshold":5,"secretKeyId":"whk_1","createdAt":"2026-11-01T09:00:00.000Z","rev":3}
                    """,
            });
        var client = NewClient(handler);

        var subscription = await client.WebhookSubscriptions.UpdateAsync(
            "whs_01JD9",
            new WebhookSubscriptionUpdateOptions("\"rev-2\"") { Enabled = false });

        Assert.False(subscription.Enabled);
        Assert.Equal([EventType.LearnerInvited], subscription.EventTypes);
        var request = Assert.Single(handler.RequestsTo(route));
        Assert.Equal("application/merge-patch+json", request.Header("Content-Type"));
        Assert.Equal("\"rev-2\"", request.Header("If-Match"));
        Assert.Null(request.Header("Idempotency-Key"));
        Assert.Equal("""{"enabled":false}""", request.Body);
    }

    [Fact]
    public async Task RemovesThePaymentMethodWithA204()
    {
        var route = $"DELETE /v1/organizations/{OrgId}/billing/payment-method";
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(route, new ScriptedReply { Status = HttpStatusCode.NoContent });
        var client = NewClient(handler);

        var response = await client.PaymentMethod.DeleteAsync();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    public static TheoryData<HttpStatusCode, bool> WriteResults => new()
    {
        { HttpStatusCode.Created, true },
        { HttpStatusCode.OK, false },
    };

    [Theory]
    [MemberData(nameof(WriteResults))]
    public async Task UpsertsALearnerByExternalIdAndSaysWhetherItWasCreated(HttpStatusCode status, bool created)
    {
        var route = $"PUT /v1/organizations/{OrgId}/learners/by-external-id/hr%20123";
        var json = LearnerJson.TrimEnd()[..^1] + """, "delivery": "none", "joinUrl": "https://training.teachmehipaa.com/join/j_1"}""";
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(route, new ScriptedReply { Status = status, Json = json });
        var client = NewClient(handler);

        var learner = await client.Learners.UpsertByExternalIdAsync("hr 123", new() { Email = "ana@example.com" });

        Assert.Equal(created, learner.Created);
        Assert.Equal(status, learner.LastResponse?.StatusCode);
        Assert.Equal(LearnerId, learner.LearnerId);
        Assert.Equal("https://training.teachmehipaa.com/join/j_1", learner.JoinUrl);
        var request = Assert.Single(handler.RequestsTo(route));
        Assert.Equal("""{"email":"ana@example.com"}""", request.Body);
        Assert.NotNull(request.Header("Idempotency-Key"));
    }

    [Fact]
    public async Task KeepsUnknownFieldsAndAcceptsUnknownValuesOnAGeneratedModel()
    {
        var route = $"GET /v1/organizations/{OrgId}/enrollments/{EnrollmentId}";
        var json = EnrollmentJson.Replace("\"status\": \"CANCELLED\"", "\"status\": \"PAUSED\", \"pausedBy\": \"hr\"", StringComparison.Ordinal);
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(route, new ScriptedReply { Json = json });
        var client = NewClient(handler);

        var enrollment = await client.Enrollments.GetAsync(EnrollmentId);

        Assert.Equal(new EnrollmentStatus("PAUSED"), enrollment.Status);
        Assert.Equal("hr", enrollment.ExtensionData["pausedBy"].GetString());
    }

    [Fact]
    public async Task ReadsAuditEventsOfKindsTheLibraryDoesNotKnowAsUnknownVariants()
    {
        var route = $"GET /v1/organizations/{OrgId}/audit-events";
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(route, new ScriptedReply
            {
                Json = $$"""
                    {"items":[
                      {"eventId":"aud_1","at":"2026-11-15T10:00:00.000Z","type":"learner.updated","actorSubjectId":"sub_admin","actor":{"type":"user","subjectId":"sub_admin"},"subjectId":"{{LearnerId}}","fields":["name"]},
                      {"eventId":"aud_2","at":"2026-11-15T11:00:00.000Z","type":"widget.spun","actor":{"type":"robot","serial":"R2"},"speed":3}
                    ],"nextCursor":null}
                    """,
            });
        var client = NewClient(handler);

        var page = await client.AuditEvents.ListAsync(new AuditEventListOptions
        {
            Limit = 2,
            Type = ListAuditEventsType.LearnerUpdated,
            Since = new DateTimeOffset(2026, 11, 15, 5, 0, 0, TimeSpan.FromHours(-5)),
        });

        var updated = Assert.IsType<AuditEventLearnerUpdated>(page.Items[0]);
        Assert.Equal("learner.updated", updated.Type);
        Assert.Equal("sub_admin", Assert.IsType<AuditEventActorUser>(updated.Actor).SubjectId);
        var unknown = Assert.IsType<UnknownAuditEvent>(page.Items[1]);
        Assert.Equal("widget.spun", unknown.Type);
        Assert.Equal(3, unknown.ExtensionData["speed"].GetInt32());
        Assert.Null(page.NextCursor);
        Assert.Equal(
            "?limit=2&type=learner.updated&since=2026-11-15T10%3A00%3A00.000Z",
            Assert.Single(handler.RequestsTo(route)).Uri.Query);
    }

    [Fact]
    public async Task ReadsAnEventOfATypeTheLibraryDoesNotKnowAsAnUnknownEvent()
    {
        var route = $"GET /v1/organizations/{OrgId}/events";
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(route, new ScriptedReply
            {
                Json = $$$"""
                    {"items":[
                      {"eventId":"evt_1","eventType":"learner.invited","orgId":"{{{OrgId}}}","occurredAt":"2026-11-15T10:00:00.000Z","data":{"learnerId":"{{{LearnerId}}}","email":"ana@example.com","externalId":null,"courseId":"hipaa-2026","status":"not_started","occurredAt":"2026-11-15T10:00:00.000Z"}},
                      {"eventId":"evt_2","eventType":"course.retired","orgId":"{{{OrgId}}}","occurredAt":"2026-11-15T11:00:00.000Z","data":{"courseId":"hipaa-2025"}}
                    ],"nextCursor":null}
                    """,
            });
        var client = NewClient(handler);

        var page = await client.Events.ListAsync();

        var invited = Assert.IsType<EventLearnerInvited>(page.Items[0]);
        Assert.Equal(LearnerStatus.NotStarted, invited.Data.Status);
        var unknown = Assert.IsType<UnknownEvent>(page.Items[1]);
        Assert.Equal("course.retired", unknown.EventType);
        Assert.Equal("hipaa-2025", unknown.Data["courseId"].GetString());
    }

    [Fact]
    public async Task ListsCoursesWithoutAnAccessToken()
    {
        const string route = "GET /v1/courses";
        var handler = new FakeHttpMessageHandler()
            .Script(route, new ScriptedReply
            {
                Json = """
                    {"items":[{"courseId":"hipaa-2026","title":"HIPAA Training 2026","courseVersion":"2026.1","passingThreshold":0.8}],"nextCursor":null}
                    """,
            });
        var client = NewClient(handler, organizationId: null);

        var page = await client.Courses.ListAsync();

        Assert.Equal(0.8, Assert.Single(page.Items).PassingThreshold);
        Assert.Null(Assert.Single(handler.RequestsTo(route)).Header("Authorization"));
        Assert.Empty(handler.RequestsTo(TokenRoute));
        Assert.Empty(handler.RequestsTo(IntrospectionRoute));
    }

    [Fact]
    public async Task VerifiesACertificateWithoutAnAccessToken()
    {
        const string route = "GET /v1/certificate-verifications/Ab3dE";
        var handler = new FakeHttpMessageHandler()
            .Script(route, new ScriptedReply
            {
                Json = """
                    {"status":"valid","learnerName":"Ana Example","courseTitle":"HIPAA Training 2026","issuedAt":"2026-11-03T08:00:00.000Z","expiresAt":"2027-11-03T08:00:00.000Z","issuer":"TeachMeHIPAA"}
                    """,
            });
        var client = NewClient(handler);

        var verification = await client.CertificateVerifications.GetAsync("Ab3dE");

        Assert.Equal(CertificateVerificationStatus.Valid, verification.Status);
        Assert.Null(Assert.Single(handler.RequestsTo(route)).Header("Authorization"));
        Assert.Empty(handler.RequestsTo(TokenRoute));
    }

    [Fact]
    public async Task BatchesEnrollmentsChosenByIdOrByFilter()
    {
        var route = $"POST /v1/organizations/{OrgId}/enrollments:batch";
        var results = new ScriptedReply { Json = """{"operation":"remind","results":[{"enrollmentId":"enr_01JD9","outcome":"succeeded"}]}""" };
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(route, results, new ScriptedReply { Json = """{"name":"op_01JDA","done":false}""" });
        var client = NewClient(handler);

        var batch = await client.Enrollments.BatchAsync(new EnrollmentBatchOptions
        {
            Operation = EnrollmentsBatchRequestOperation.Remind,
            Select = new EnrollmentsBatchRequestSelectEnrollmentIds { EnrollmentIds = [EnrollmentId] },
        });
        await client.Enrollments.BatchByFilterAsync(new EnrollmentBatchByFilterOptions
        {
            Operation = EnrollmentsBatchRequestOperation.Archive,
            Select = new EnrollmentsBatchRequestSelectFilter { Filter = new() { CourseId = "hipaa-2025" } },
        });

        Assert.Equal(EnrollmentsBatchResultsResultsItemOutcome.Succeeded, Assert.Single(batch.Results).Outcome);
        var requests = handler.RequestsTo(route);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse($$$"""{"operation":"remind","select":{"enrollmentIds":["{{{EnrollmentId}}}"]}}"""),
            JsonNode.Parse(requests[0].Body!)));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("""{"operation":"archive","select":{"filter":{"courseId":"hipaa-2025"}}}"""),
            JsonNode.Parse(requests[1].Body!)));
    }

    [Fact]
    public async Task OrdersSeatsWithAPaymentMethodVariantAndReadsTheNextAction()
    {
        var route = $"POST /v1/organizations/{OrgId}/seat-orders";
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(route, new ScriptedReply
            {
                Status = HttpStatusCode.Created,
                Json = """
                    {"seatOrderId":"so_01JD9","status":"invoiced","nextAction":{"type":"invoice","hostedInvoiceUrl":"https://pay.example.com/i/1"},"failure":null}
                    """,
            });
        var client = NewClient(handler);

        var order = await client.SeatOrders.CreateAsync(new SeatOrderCreateOptions
        {
            Quantity = 5,
            Payment = new SeatOrderCreateRequestPaymentInvoice { DaysUntilDue = 30 },
        });

        Assert.Equal(SeatOrderCreatedStatus.Invoiced, order.Status);
        Assert.IsType<SeatOrderCreatedNextActionInvoice>(order.NextAction);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("""{"quantity":5,"payment":{"method":"invoice","daysUntilDue":30}}"""),
            JsonNode.Parse(Assert.Single(handler.RequestsTo(route)).Body!)));
    }
}
