using Microsoft.Extensions.Logging;

namespace TeachMeHipaa;

/// <summary>
/// Logs one debug entry per HTTP response with only the fields ADR-0007
/// allows. It sits below authentication so token mints are logged too, and so
/// nothing above it can leak headers or bodies into a log.
/// </summary>
internal sealed partial class LoggingHandler(ILogger logger, TimeProvider timeProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var startedAt = timeProvider.GetTimestamp();
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (logger.IsEnabled(LogLevel.Debug))
        {
            var latencyMs = (long)timeProvider.GetElapsedTime(startedAt).TotalMilliseconds;
            // The query is left out because it can carry Learner emails.
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            var requestId = response.Headers.TryGetValues("x-request-id", out var values) ? values.FirstOrDefault() : null;
            LogResponse(logger, request.Method.Method, path, (int)response.StatusCode, latencyMs, requestId);
        }

        return response;
    }

    [LoggerMessage(
        EventId = 1,
        EventName = "TeachMeHipaaRequest",
        Level = LogLevel.Debug,
        Message = "teachmehipaa request {Method} {Path} {StatusCode} in {LatencyMs} ms, request id {RequestId}")]
    private static partial void LogResponse(
        ILogger logger, string method, string path, int statusCode, long latencyMs, string? requestId);
}
