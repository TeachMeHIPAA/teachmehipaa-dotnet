using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

/// <summary>Operations: waiting for completion and reading export CSVs, through the public client and the fake transport.</summary>
public class OperationTests
{
    private const string OperationId = "0b7d4a8e-5c1f-4e2a-9d3b-6f8a1c2e4b7d";

    // The Developer API names an Operation organizations/{orgId}/operations/{id}.
    private const string OperationName = $"organizations/{OrgId}/operations/{OperationId}";
    private const string SubscriptionId = "whs_01JD8K2M4N6P8R0T2V4X6Z8B0D";
    private const string PollRoute = $"GET /v1/organizations/{OrgId}/operations/{OperationId}";
    private const string TestDeliveryRoute = $"POST /v1/organizations/{OrgId}/webhook-subscriptions/{SubscriptionId}:test";
    private const string EnrollmentExportRoute = $"POST /v1/organizations/{OrgId}/enrollments:export";

    // An export's 202: a done Operation whose response is the monorepo's
    // ExportArtifact (api-contracts bulk.schemas.ts), the CSV in base64.
    private const string ExportOperationJson = $$"""
        {
          "name": "{{OperationName}}",
          "done": true,
          "metadata": { "kind": "enrollments:export" },
          "response": {
            "format": "csv",
            "contentType": "text/csv; charset=utf-8",
            "filename": "enrollments-{{OrgId}}.csv",
            "rowCount": 2,
            "dataBase64": "bGVhcm5lcklkLGVtYWlsLG5hbWUNCnN1Yl8wMUpEODBBQjJDM0Q0RTVGNkc3SDhKOUswTSxhbmFAZXhhbXBsZS5jb20sIkV4YW1wbGUsIEFuYSINCnN1Yl8wMUpEODBBQjJDM0Q0RTVGNkc3SDhKOUswTixqb3NlQGV4YW1wbGUuY29tLEpvc8OpIE7DusOxZXoNCg=="
          }
        }
        """;

    // What ExportOperationJson's dataBase64 decodes to: RFC 4180, CRLF, UTF-8.
    private const string ExportedCsv =
        "learnerId,email,name\r\n"
        + "sub_01JD80AB2C3D4E5F6G7H8J9K0M,ana@example.com,\"Example, Ana\"\r\n"
        + "sub_01JD80AB2C3D4E5F6G7H8J9K0N,jose@example.com,José Núñez\r\n";

    // A Test delivery's 202: pending until the webhook worker settles it.
    private const string PendingOperationJson = $$"""{"name":"{{OperationName}}","done":false}""";

    // The worker's settle on a 2xx from the endpoint (§8: response._tag).
    private const string SentOperationJson = $$"""{"name":"{{OperationName}}","done":true,"response":{"_tag":"sent","responseStatus":200} }""";

    // dev#/components/schemas/Operation: done, with a problem.
    private const string FailedOperationJson = $$"""
        {
          "name": "{{OperationName}}",
          "done": true,
          "error": {
            "title": "Temporarily unavailable",
            "status": 503,
            "code": "temporarily_unavailable",
            "detail": "Webhook delivery is paused. Try again later.",
            "errors": [{ "path": "url", "message": "unreachable" }]
          }
        }
        """;

    private static readonly JsonSerializerOptions OperationJsonOptions = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    [Fact]
    public async Task ADoneOperationReturnsFromWaitWithoutARequest()
    {
        var time = new TestTime();
        var handler = Scripted((EnrollmentExportRoute, Accepted(ExportOperationJson)));
        var client = NewClient(handler, time: time, maxRetries: 0);

        var operation = await client.Enrollments.ExportAsync(new EnrollmentExportOptions { Format = EnrollmentsExportRequestFormat.Csv });
        var finished = await operation.WaitAsync();

        Assert.True(finished.Done);
        Assert.Equal(OperationName, finished.Name);
        Assert.Equal(HttpStatusCode.Accepted, finished.LastResponse!.StatusCode);
        Assert.Equal([TokenRoute, EnrollmentExportRoute], handler.Requests.Select(request => request.Route));
        Assert.Empty(time.Fired);
    }

    [Fact]
    public async Task APendingOperationPollsUntilItIsDone()
    {
        var time = new TestTime();
        var handler = PendingThen(
            Reply(PendingOperationJson),
            Reply(PendingOperationJson),
            Reply(SentOperationJson) with { Headers = new Dictionary<string, string> { ["x-request-id"] = "req_poll_3" } });
        var client = NewClient(handler, time: time, maxRetries: 0);

        var operation = await client.WebhookSubscriptions.TestAsync(SubscriptionId);
        var finished = await time.RunAsync(operation.WaitAsync());

        Assert.False(operation.Done);
        Assert.True(finished.Done);
        Assert.Equal("req_poll_3", finished.LastResponse!.RequestId);
        Assert.Equal(3, handler.RequestsTo(PollRoute).Count);
        Assert.Equal(3, time.Fired.Count);
    }

    [Fact]
    public async Task PollsBackOffFromOneSecondDoublingWithJitterToATenSecondCap()
    {
        var time = new TestTime();
        var handler = PendingThen([.. Enumerable.Repeat(Reply(PendingOperationJson), 7), Reply(SentOperationJson)]);
        var client = NewClient(handler, time: time, maxRetries: 0);

        var operation = await client.WebhookSubscriptions.TestAsync(SubscriptionId);
        await time.RunAsync(operation.WaitAsync());

        // Ceilings 1, 2, 4, 8, 10, 10, 10, 10 s; jitter keeps each within
        // 50-100 % of its ceiling, and never below the 1 s interval.
        (double Low, double High)[] ranges = [(1, 1), (1, 2), (2, 4), (4, 8), (5, 10), (5, 10), (5, 10), (5, 10)];
        Assert.Equal(ranges.Length, time.Fired.Count);
        Assert.All(
            time.Fired.Zip(ranges),
            pair => Assert.InRange(pair.First.TotalSeconds, pair.Second.Low, pair.Second.High));
    }

    [Fact]
    public async Task PollsStartFromTheIntervalGiven()
    {
        var time = new TestTime();
        var handler = PendingThen(Reply(PendingOperationJson), Reply(SentOperationJson));
        var client = NewClient(handler, time: time, maxRetries: 0);

        var operation = await client.WebhookSubscriptions.TestAsync(SubscriptionId);
        await time.RunAsync(operation.WaitAsync(interval: TimeSpan.FromMilliseconds(250)));

        Assert.Equal(TimeSpan.FromMilliseconds(250), time.Fired[0]);
        Assert.InRange(time.Fired[1].TotalMilliseconds, 250, 500);
    }

    [Fact]
    public async Task WaitThrowsOperationTimeoutExceptionAfterALastPollAtTheDeadline()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(TestDeliveryRoute, Accepted(PendingOperationJson))
            .Script(PollRoute, [.. Enumerable.Repeat(Reply(PendingOperationJson), 10)]);
        var client = NewClient(handler, time: time, maxRetries: 0);
        var operation = await client.WebhookSubscriptions.TestAsync(SubscriptionId);

        var error = await Assert.ThrowsAsync<OperationTimeoutException>(
            () => time.RunAsync(operation.WaitAsync(timeout: TimeSpan.FromSeconds(5))));

        Assert.IsAssignableFrom<TeachMeHipaaException>(error);
        Assert.Equal($"Operation {OperationName} was not done after 5 s.", error.Message);
        Assert.False(error.Operation.Done);
        Assert.Equal(OperationName, error.Operation.Name);
        Assert.Equal(TimeSpan.FromSeconds(5), time.Fired.Aggregate(TimeSpan.Zero, (sum, wait) => sum + wait));
        Assert.Equal(time.Fired.Count, handler.RequestsTo(PollRoute).Count);
    }

    [Fact]
    public async Task WaitThrowsOperationFailedExceptionWithTheProblem()
    {
        var time = new TestTime();
        var handler = PendingThen(Reply(FailedOperationJson) with { Headers = new Dictionary<string, string> { ["x-request-id"] = "req_poll_1" } });
        var client = NewClient(handler, time: time, maxRetries: 0);
        var operation = await client.WebhookSubscriptions.TestAsync(SubscriptionId);

        var error = await Assert.ThrowsAsync<OperationFailedException>(() => time.RunAsync(operation.WaitAsync()));

        Assert.IsAssignableFrom<TeachMeHipaaException>(error);
        Assert.Equal("Webhook delivery is paused. Try again later.", error.Message);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, error.StatusCode);
        Assert.Equal("temporarily_unavailable", error.Code);
        Assert.Equal("Temporarily unavailable", error.Title);
        Assert.Equal("Webhook delivery is paused. Try again later.", error.Detail);
        Assert.Equal([new FieldError("url", "unreachable")], error.Errors);
        Assert.Equal("req_poll_1", error.RequestId);
        Assert.True(error.Operation.Done);
    }

    [Fact]
    public async Task AnOperationAlreadyDoneWithAnErrorThrowsWithoutARequest()
    {
        var handler = Scripted((TestDeliveryRoute, Accepted(FailedOperationJson)));
        var client = NewClient(handler, maxRetries: 0);
        var operation = await client.WebhookSubscriptions.TestAsync(SubscriptionId);

        await Assert.ThrowsAsync<OperationFailedException>(() => operation.WaitAsync());

        Assert.Empty(handler.RequestsTo(PollRoute));
    }

    [Fact]
    public async Task CancellingAWaitStopsItMidDelayWithoutAPoll()
    {
        var time = new DelayWatchingTime();
        var handler = PendingThen(Reply(SentOperationJson));
        var client = NewClient(handler, time: time, maxRetries: 0);
        var operation = await client.WebhookSubscriptions.TestAsync(SubscriptionId);
        using var cancellation = new CancellationTokenSource();

        var waiting = operation.WaitAsync(cancellationToken: cancellation.Token);
        Assert.Equal(TimeSpan.FromSeconds(1), await time.DelayScheduled);
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting);
        Assert.Empty(handler.RequestsTo(PollRoute));
    }

    [Fact]
    public async Task GetExportResultReturnsTheCsvTextFileNameAndRowCount()
    {
        var handler = Scripted((EnrollmentExportRoute, Accepted(ExportOperationJson)));
        var client = NewClient(handler, maxRetries: 0);

        var operation = await client.Enrollments.ExportAsync(new EnrollmentExportOptions { Format = EnrollmentsExportRequestFormat.Csv });

        Assert.Equal(new ExportResult(ExportedCsv, $"enrollments-{OrgId}.csv", 2), operation.GetExportResult());
    }

    [Fact]
    public async Task GetExportResultReadsACertificateExport()
    {
        var handler = Scripted(($"POST /v1/organizations/{OrgId}/certificates:export", Accepted(ExportOperationJson)));
        var client = NewClient(handler, maxRetries: 0);

        var operation = await client.Certificates.ExportAsync(new CertificateExportOptions { Format = CertificatesExportRequestFormat.Csv });

        Assert.Equal(ExportedCsv, operation.GetExportResult().Csv);
    }

    [Fact]
    public async Task GetExportResultReadsALearnerExport()
    {
        var handler = Scripted(($"POST /v1/organizations/{OrgId}/learners:export", Accepted(ExportOperationJson)));
        var client = NewClient(handler, maxRetries: 0);

        var operation = await client.Learners.ExportAsync(new LearnerExportOptions { Format = LearnersExportRequestFormat.Csv, CourseId = "hipaa-2026" });

        Assert.Equal(ExportedCsv, operation.GetExportResult().Csv);
    }

    [Fact]
    public async Task GetExportResultReadsAnExportRetrievedById()
    {
        var handler = Scripted((PollRoute, Reply(ExportOperationJson)));
        var client = NewClient(handler, maxRetries: 0);

        var operation = await client.Operations.GetAsync(OperationId);

        Assert.Equal(ExportedCsv, operation.GetExportResult().Csv);
    }

    [Fact]
    public async Task AnExportResultKeepsTheCsvOutOfItsText()
    {
        var handler = Scripted((PollRoute, Reply(ExportOperationJson)));
        var client = NewClient(handler, maxRetries: 0);

        var result = (await client.Operations.GetAsync(OperationId)).GetExportResult();

        Assert.DoesNotContain("ana@example.com", result.ToString(), StringComparison.Ordinal);
        Assert.Contains("RowCount = 2", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetExportResultRefusesAnOperationThatIsNotDone()
    {
        var handler = Scripted((PollRoute, Reply(PendingOperationJson)));
        var client = NewClient(handler, maxRetries: 0);
        var operation = await client.Operations.GetAsync(OperationId);

        var error = Assert.Throws<InvalidOperationException>(operation.GetExportResult);

        Assert.Equal($"Operation {OperationName} is not done; call WaitAsync first.", error.Message);
    }

    [Fact]
    public async Task GetExportResultRefusesAnOperationThatIsNotAnExport()
    {
        var handler = Scripted((PollRoute, Reply(SentOperationJson)));
        var client = NewClient(handler, maxRetries: 0);
        var operation = await client.Operations.GetAsync(OperationId);

        var error = Assert.Throws<InvalidOperationException>(operation.GetExportResult);

        Assert.Equal($"Operation {OperationName} did not produce a CSV export.", error.Message);
    }

    [Fact]
    public async Task GetExportResultThrowsOperationFailedExceptionForAFailedExport()
    {
        var handler = Scripted((PollRoute, Reply(FailedOperationJson)));
        var client = NewClient(handler, maxRetries: 0);
        var operation = await client.Operations.GetAsync(OperationId);

        var error = Assert.Throws<OperationFailedException>(operation.GetExportResult);

        Assert.Equal("temporarily_unavailable", error.Code);
    }

    [Fact]
    public async Task ATestDeliveryReadsAsASentDeliveryResult()
    {
        var handler = PendingThen(Reply(SentOperationJson));
        var time = new TestTime();
        var client = NewClient(handler, time: time, maxRetries: 0);
        var operation = await client.WebhookSubscriptions.TestAsync(SubscriptionId);

        var finished = await time.RunAsync(operation.WaitAsync());
        var sent = Assert.IsType<WebhookDeliverySent>(finished.GetDeliveryResult());

        Assert.Equal("sent", sent.Tag);
        Assert.Equal(200, sent.ResponseStatus);
    }

    [Fact]
    public async Task ARedriveThatFailedReadsAsAFailedDeliveryResult()
    {
        var failed = Assert.IsType<WebhookDeliveryFailed>(await RedriveResultAsync("""{"_tag":"failed","reason":"endpoint_gone","responseStatus":410}"""));

        Assert.Equal("failed", failed.Tag);
        Assert.Equal("endpoint_gone", failed.Reason);
        Assert.Equal(410, failed.ResponseStatus);
    }

    [Fact]
    public async Task AFailedDeliveryResultWhoseEndpointNeverAnsweredHasNoStatus()
    {
        var failed = Assert.IsType<WebhookDeliveryFailed>(await RedriveResultAsync("""{"_tag":"failed","reason":"connection_refused"}"""));

        Assert.Equal("connection_refused", failed.Reason);
        Assert.Null(failed.ResponseStatus);
    }

    [Fact]
    public async Task ARedriveOfAnEndedDeliveryReadsAsASettledDeliveryResult()
    {
        var settled = Assert.IsType<WebhookDeliverySettled>(await RedriveResultAsync("""{"_tag":"settled","status":"delivered"}"""));

        Assert.Equal("settled", settled.Tag);
        Assert.Equal("delivered", settled.Status);
    }

    [Fact]
    public async Task ADeliveryResultWithAnUnknownTagIsKeptNotRejected()
    {
        var unknown = Assert.IsType<UnknownWebhookDeliveryResult>(await RedriveResultAsync("""{"_tag":"skipped","why":"nothing to deliver"}"""));

        Assert.Equal("skipped", unknown.Tag);
        Assert.Equal("nothing to deliver", unknown.ExtensionData["why"].GetString());
    }

    [Fact]
    public async Task GetDeliveryResultRefusesAnOperationThatIsNotDone()
    {
        var handler = Scripted((TestDeliveryRoute, Accepted(PendingOperationJson)));
        var client = NewClient(handler, maxRetries: 0);
        var operation = await client.WebhookSubscriptions.TestAsync(SubscriptionId);

        var error = Assert.Throws<InvalidOperationException>(operation.GetDeliveryResult);

        Assert.Equal($"Operation {OperationName} is not done; call WaitAsync first.", error.Message);
    }

    [Fact]
    public async Task GetDeliveryResultRefusesAnOperationThatIsNotADelivery()
    {
        var handler = Scripted((PollRoute, Reply(ExportOperationJson)));
        var client = NewClient(handler, maxRetries: 0);
        var operation = await client.Operations.GetAsync(OperationId);

        var error = Assert.Throws<InvalidOperationException>(operation.GetDeliveryResult);

        Assert.Equal($"Operation {OperationName} did not produce a Delivery result.", error.Message);
    }

    [Fact]
    public async Task GetDeliveryResultThrowsOperationFailedExceptionForAFailedOperation()
    {
        var handler = Scripted((TestDeliveryRoute, Accepted(FailedOperationJson)));
        var client = NewClient(handler, maxRetries: 0);
        var operation = await client.WebhookSubscriptions.TestAsync(SubscriptionId);

        var error = Assert.Throws<OperationFailedException>(operation.GetDeliveryResult);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, error.StatusCode);
    }

    [Fact]
    public void OperationIdFromNameReadsTheIdThatOperationsGetTakes()
    {
        Assert.Equal(
            "0b7d4a8e-5c1f-4e2a-9d3b-6f8a1c2e4b7d",
            Operation.OperationIdFromName("organizations/org_acme/operations/0b7d4a8e-5c1f-4e2a-9d3b-6f8a1c2e4b7d"));
    }

    [Theory]
    [InlineData("0b7d4a8e-5c1f-4e2a-9d3b-6f8a1c2e4b7d")]
    [InlineData("organizations/org_acme/operations/")]
    [InlineData("organizations//operations/op_1")]
    [InlineData("organizations/org_acme/learners/sub_01JD80AB2C3D4E5F6G7H8J9K0M")]
    [InlineData("organizations/org_acme/operations/op_1/extra")]
    [InlineData("/organizations/org_acme/operations/op_1")]
    public void OperationIdFromNameRejectsAnythingButAnOperationName(string name)
    {
        var error = Assert.Throws<ArgumentException>(() => Operation.OperationIdFromName(name));

        Assert.Contains("is not an Operation name", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnOperationReadThroughTheClientSerializesAsTheApiSentIt()
    {
        var handler = Scripted((TestDeliveryRoute, Accepted(SentOperationJson)));
        var client = NewClient(handler, maxRetries: 0);
        var operation = await client.WebhookSubscriptions.TestAsync(SubscriptionId);

        var json = JsonSerializer.Serialize(operation, OperationJsonOptions);

        Assert.Equal($$$"""{"name":"{{{OperationName}}}","done":true,"response":{"_tag":"sent","responseStatus":200}}""", json);
    }

    private static FakeHttpMessageHandler Scripted(params (string Route, ScriptedReply Reply)[] routes)
    {
        var handler = new FakeHttpMessageHandler().Script(TokenRoute, TokenReply());
        foreach (var (route, reply) in routes)
        {
            handler.Script(route, reply);
        }

        return handler;
    }

    /// <summary>A Test delivery that starts pending, then polls through <paramref name="polls"/>.</summary>
    private static FakeHttpMessageHandler PendingThen(params ScriptedReply[] polls) =>
        Scripted((TestDeliveryRoute, Accepted(PendingOperationJson))).Script(PollRoute, polls);

    /// <summary>How a redrive that the API answered with a done Operation ended.</summary>
    private static async Task<WebhookDeliveryResult> RedriveResultAsync(string response)
    {
        var handler = Scripted((
            $"POST /v1/organizations/{OrgId}/webhook-subscriptions/{SubscriptionId}:redrive",
            Accepted($$"""{"name":"{{OperationName}}","done":true,"response":{{response}} }""")));
        var client = NewClient(handler, maxRetries: 0);

        var operation = await client.WebhookSubscriptions.RedriveAsync(SubscriptionId);

        return operation.GetDeliveryResult();
    }

    private static ScriptedReply Accepted(string json) => new() { Status = HttpStatusCode.Accepted, Json = json };

    private static ScriptedReply Reply(string json) => new() { Json = json };

    /// <summary>Fake time that reports the first delay anyone waits on, and never moves on its own.</summary>
    private sealed class DelayWatchingTime : Microsoft.Extensions.Time.Testing.FakeTimeProvider
    {
        private readonly TaskCompletionSource<TimeSpan> _delayScheduled = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<TimeSpan> DelayScheduled => _delayScheduled.Task;

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            // The pipeline's request timeouts are 80 s; a poll delay is far shorter.
            if (dueTime < TimeSpan.FromSeconds(60))
            {
                _delayScheduled.TrySetResult(dueTime);
            }

            return base.CreateTimer(callback, state, dueTime, period);
        }
    }
}
