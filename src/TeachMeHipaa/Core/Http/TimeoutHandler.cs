using System.Globalization;

namespace TeachMeHipaa;

/// <summary>A per-request override of the client's timeout, carried on the request itself.</summary>
internal static class RequestTimeout
{
#if NET5_0_OR_GREATER
    private static readonly HttpRequestOptionsKey<TimeSpan> Key = new("TeachMeHipaa.Timeout");

    public static void Set(HttpRequestMessage request, TimeSpan timeout) => request.Options.Set(Key, timeout);

    public static TimeSpan? Get(HttpRequestMessage request) =>
        request.Options.TryGetValue(Key, out var timeout) ? timeout : null;
#else
    private const string Key = "TeachMeHipaa.Timeout";

    public static void Set(HttpRequestMessage request, TimeSpan timeout) => request.Properties[Key] = timeout;

    public static TimeSpan? Get(HttpRequestMessage request) =>
        request.Properties.TryGetValue(Key, out var timeout) ? (TimeSpan?)timeout : null;
#endif
}

/// <summary>
/// Bounds each HTTP attempt, token mints included, and reports a timeout as a
/// <see cref="TeachMeHipaaException"/> rather than a bare cancellation.
/// </summary>
internal sealed class TimeoutHandler(TimeSpan defaultTimeout, TimeProvider timeProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var timeout = RequestTimeout.Get(request) ?? defaultTimeout;
#if NET8_0_OR_GREATER
        using var timeoutSource = new CancellationTokenSource(timeout, timeProvider);
#else
        using var timeoutSource = timeProvider.CreateCancellationTokenSource(timeout);
#endif
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
        try
        {
            return await base.SendAsync(request, linked.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException exception) when (timeoutSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            var seconds = timeout.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture);
            throw new TeachMeHipaaException(
                $"No response from TeachMeHIPAA within {seconds} s ({request.Method} {request.RequestUri?.AbsolutePath}).",
                new TimeoutException(exception.Message, exception));
        }
    }
}
