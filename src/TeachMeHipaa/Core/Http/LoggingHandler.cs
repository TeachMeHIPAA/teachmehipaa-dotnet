using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace TeachMeHipaa;

/// <summary>
/// Logs one debug entry per HTTP attempt with only the fields ADR-0007
/// allows: a response, or a connection error or timeout before it is raised.
/// It sits below authentication so token mints are logged too, and so
/// nothing above it can leak headers or bodies into a log; and above
/// <see cref="TimeoutHandler"/>, which turns every failure to get a response
/// into an <see cref="ApiConnectionException"/>.
/// </summary>
internal sealed partial class LoggingHandler(ILogger logger, TimeProvider timeProvider) : DelegatingHandler
{
    private const string RequestIdHeader = "x-request-id";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var startedAt = timeProvider.GetTimestamp();
        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiConnectionException exception) when (logger.IsEnabled(LogLevel.Debug))
        {
            var path = PathOf(request);
            var errorKind = exception is ApiTimeoutException ? "timeout" : "connection";
            var latencyMs = LatencyMs(startedAt);
            // No response, so the only request id is the one the caller sent.
            var requestId = FirstValue(request.Headers, RequestIdHeader);
            LogFailure(logger, request.Method.Method, path, errorKind, latencyMs, requestId);
            throw;
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            var path = PathOf(request);
            var latencyMs = LatencyMs(startedAt);
            var requestId = FirstValue(response.Headers, RequestIdHeader);
            LogResponse(logger, request.Method.Method, path, (int)response.StatusCode, latencyMs, requestId);
        }

        return response;
    }

    // The query is left out because it can carry Learner emails.
    private static string PathOf(HttpRequestMessage request) => request.RequestUri?.AbsolutePath ?? string.Empty;

    private static string? FirstValue(HttpHeaders headers, string name) =>
        headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;

    private long LatencyMs(long startedAt) => (long)timeProvider.GetElapsedTime(startedAt).TotalMilliseconds;

    [LoggerMessage(
        EventId = 1,
        EventName = "TeachMeHipaaRequest",
        Level = LogLevel.Debug,
        Message = "teachmehipaa request {Method} {Path} {StatusCode} in {LatencyMs} ms, request id {RequestId}")]
    private static partial void LogResponse(
        ILogger logger, string method, string path, int statusCode, long latencyMs, string? requestId);

    [LoggerMessage(
        EventId = 2,
        EventName = "TeachMeHipaaRequestFailed",
        Level = LogLevel.Debug,
        Message = "teachmehipaa request {Method} {Path} failed ({ErrorKind}) after {LatencyMs} ms, request id {RequestId}")]
    private static partial void LogFailure(
        ILogger logger, string method, string path, string errorKind, long latencyMs, string? requestId);
}
