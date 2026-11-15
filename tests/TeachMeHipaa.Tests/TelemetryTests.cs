using Microsoft.Extensions.Logging;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class TelemetryTests
{
    [Fact]
    public async Task IdentifiesTheLibraryAndTheCallersAppInTheUserAgent()
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
            AppInfo = new AppInfo("AcmeHR", "4.2.0", "https://acme.example"),
        });

        await client.Learners.GetAsync(LearnerId);

        Assert.All(handler.Requests, request =>
        {
            Assert.Matches(@"^teachmehipaa-dotnet/0\.1\.0 \(\.NET [^)]+\) AcmeHR/4\.2\.0 \(\+https://acme\.example\)$", request.Header("User-Agent"));
        });
    }

    [Fact]
    public async Task SendsNoClientTelemetryHeaders()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, LearnerReply());
        var client = NewClient(handler);

        await client.Learners.GetAsync(LearnerId);

        Assert.Equal(
            ["Accept", "Authorization", "User-Agent"],
            Assert.Single(handler.RequestsTo(LearnerRoute)).Headers.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task LogsMethodPathStatusLatencyAndRequestIdButNoCredentialsOrBodies()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply("access-token-secret-token"))
            .Script(LearnerRoute, LearnerReply(new Dictionary<string, string> { ["x-request-id"] = "req-7" }));
        var logger = new RecordingLogger();
        var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            OrganizationId = OrgId,
            HttpMessageHandler = handler,
            Logger = logger,
        });

        await client.Learners.GetAsync(LearnerId, new LearnerGetOptions { CourseId = "ana@example.com" });

        Assert.Equal(2, logger.Entries.Count);
        var entry = logger.Entries[1];
        Assert.Equal(LogLevel.Debug, entry.Level);
        Assert.Equal("GET", entry.State["Method"]);
        Assert.Equal($"/v1/organizations/{OrgId}/learners/{LearnerId}", entry.State["Path"]);
        Assert.Equal(200, entry.State["StatusCode"]);
        Assert.IsType<long>(entry.State["LatencyMs"]);
        Assert.Equal("req-7", entry.State["RequestId"]);
        Assert.All(logger.Entries, logged =>
        {
            Assert.DoesNotContain("access-token-secret-token", logged.Message, StringComparison.Ordinal);
            Assert.DoesNotContain(ClientSecret, logged.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("ana@example.com", logged.Message, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task LogsATimedOutRequestWithTheRequestIdItSentBeforeRaising()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply("access-token-secret"))
            .Script(LearnerRoute, ScriptedReply.NeverAnswers);
        var logger = new RecordingLogger();
        var client = LoggingClient(handler, logger, time);

        await Assert.ThrowsAsync<ApiTimeoutException>(() => time.RunAsync(client.Learners.GetAsync(
            LearnerId,
            new LearnerGetOptions { CourseId = "ana@example.com" },
            new RequestOptions { RequestId = "acme-sync-42", Timeout = TimeSpan.FromSeconds(3) })));

        var entry = logger.Entries[^1];
        Assert.Equal(LogLevel.Debug, entry.Level);
        Assert.Equal("GET", entry.State["Method"]);
        Assert.Equal($"/v1/organizations/{OrgId}/learners/{LearnerId}", entry.State["Path"]);
        Assert.Equal("timeout", entry.State["ErrorKind"]);
        Assert.Equal(3000L, entry.State["LatencyMs"]);
        Assert.Equal("acme-sync-42", entry.State["RequestId"]);
        Assert.False(entry.State.ContainsKey("StatusCode"));
        Assert.All(logger.Entries, logged =>
        {
            Assert.DoesNotContain("access-token-secret", logged.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("ana@example.com", logged.Message, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task LogsARequestThatCouldNotConnectBeforeRaising()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, new ScriptedReply { Throws = new HttpRequestException("Connection refused") });
        var logger = new RecordingLogger();
        var client = LoggingClient(handler, logger, time);

        await Assert.ThrowsAsync<ApiConnectionException>(() => time.RunAsync(client.Learners.GetAsync(LearnerId)));

        var entry = logger.Entries[^1];
        Assert.Equal("GET", entry.State["Method"]);
        Assert.Equal($"/v1/organizations/{OrgId}/learners/{LearnerId}", entry.State["Path"]);
        Assert.Equal("connection", entry.State["ErrorKind"]);
        Assert.IsType<long>(entry.State["LatencyMs"]);
        Assert.Null(entry.State["RequestId"]);
    }

    private static TeachMeHipaaClient LoggingClient(FakeHttpMessageHandler handler, ILogger logger, TestTime time) =>
        new(new TeachMeHipaaClientOptions
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            OrganizationId = OrgId,
            HttpMessageHandler = handler,
            Logger = logger,
            TimeProvider = time,
            MaxRetries = 0,
        });

    private sealed class RecordingLogger : ILogger
    {
        public List<(LogLevel Level, string Message, IReadOnlyDictionary<string, object?> State)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var fields = state as IEnumerable<KeyValuePair<string, object?>> ?? [];
            Entries.Add((logLevel, formatter(state, exception), fields.ToDictionary(field => field.Key, field => field.Value)));
        }
    }
}
