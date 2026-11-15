using System.Net;

namespace TeachMeHipaa;

/// <summary>Which responses a <see cref="RetryHandler"/> treats as transient.</summary>
internal enum RetryPolicy
{
    /// <summary>409 <c>conflict</c> (the same Idempotency-Key is in flight), 429 and 5xx.</summary>
    Api,

    /// <summary>
    /// 5xx only: a client-credentials mint mints a fresh token each time, but
    /// an OAuth error such as <c>invalid_client</c> won't change on a retry.
    /// </summary>
    TokenEndpoint,
}

/// <summary>
/// Retries a retry-safe request after a network error, a timeout or a
/// transient response, backing off 0.5 s doubling to 5 s with jitter, or as
/// long as <c>Retry-After</c> asks up to 60 s. It sits above
/// <see cref="TimeoutHandler"/> so each attempt is timed on its own, and
/// below authentication so a 401 replay doesn't use up a retry.
/// </summary>
internal sealed class RetryHandler(RetryPolicy policy, int maxRetries, TimeProvider timeProvider) : DelegatingHandler
{
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(0.5);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan MaxRetryAfter = TimeSpan.FromSeconds(60);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var retries = RequestProperties.RetrySafe.Get(request) == true
            ? RequestProperties.MaxRetries.Get(request) ?? maxRetries
            : 0;
        if (retries <= 0)
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var replay = await ReplayableRequest.CaptureAsync(request, cancellationToken).ConfigureAwait(false);
        for (var retry = 1; ; retry++)
        {
            TimeSpan delay;
            try
            {
                var response = await base.SendAsync(replay.Create(), cancellationToken).ConfigureAwait(false);
                if (retry > retries || !await IsTransientAsync(response, cancellationToken).ConfigureAwait(false))
                {
                    return response;
                }

                delay = ServerDelay(response, retry);
                response.Dispose();
            }
            catch (ApiConnectionException) when (retry <= retries)
            {
                delay = Backoff(retry);
            }

#if NET8_0_OR_GREATER
            await Task.Delay(delay, timeProvider, cancellationToken).ConfigureAwait(false);
#else
            await timeProvider.Delay(delay, cancellationToken).ConfigureAwait(false);
#endif
        }
    }

    private async Task<bool> IsTransientAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var status = (int)response.StatusCode;
        return status >= 500
            || (policy == RetryPolicy.Api
                && (response.StatusCode == (HttpStatusCode)429
                    || (response.StatusCode == HttpStatusCode.Conflict
                        && await ResponseReader.ReadProblemCodeAsync(response, cancellationToken).ConfigureAwait(false) == "conflict")));
    }

    private TimeSpan ServerDelay(HttpResponseMessage response, int retry) =>
        ResponseReader.RetryAfter(response, timeProvider.GetUtcNow()) is { } retryAfter
            ? (retryAfter < MaxRetryAfter ? retryAfter : MaxRetryAfter)
            : Backoff(retry);

    // Stripe's schedule: the ceiling doubles from 0.5 s to 5 s, jitter picks
    // 50–100 % of it, and nothing waits less than 0.5 s.
    private static TimeSpan Backoff(int retry)
    {
        var ceiling = Math.Min(InitialDelay.TotalSeconds * Math.Pow(2, retry - 1), MaxDelay.TotalSeconds);
        var seconds = Math.Max(InitialDelay.TotalSeconds, ceiling * (0.5 + (Jitter.Next() * 0.5)));
        return TimeSpan.FromSeconds(seconds);
    }
}
