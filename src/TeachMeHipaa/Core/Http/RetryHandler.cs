using System.Net;

namespace TeachMeHipaa;

/// <summary>Which responses a <see cref="RetryHandler"/> treats as transient.</summary>
internal enum RetryPolicy
{
    /// <summary>409 <c>conflict</c> (the same Idempotency-Key is in flight), 429 and 5xx.</summary>
    Api,

    /// <summary>
    /// 429 and 5xx: a client-credentials mint mints a fresh token each time,
    /// but an OAuth error such as <c>invalid_client</c> won't change on a retry.
    /// </summary>
    TokenEndpoint,
}

/// <summary>
/// Retries a retry-safe request after a network error, a timeout or a
/// transient response, backing off 0.5 s doubling to 5 s with jitter, or as
/// long as <c>Retry-After</c> asks up to 60 s. It sits above
/// <see cref="TimeoutHandler"/> so each attempt is timed on its own, and
/// below authentication, whose 401 replay doesn't use up a retry but does
/// share the call's <see cref="RetryBudget"/>.
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

        // A call's 401 replay comes back through here with the same budget,
        // so the call as a whole never retries more than its limit.
        var budget = RequestProperties.RetryBudget.Get(request) ?? new RetryBudget();
        var replay = await ReplayableRequest.CaptureAsync(request, cancellationToken).ConfigureAwait(false);
        while (true)
        {
            TimeSpan delay;
            try
            {
                var response = await base.SendAsync(replay.Create(), cancellationToken).ConfigureAwait(false);
                if (budget.Used >= retries || !await IsTransientAsync(response, cancellationToken).ConfigureAwait(false))
                {
                    return response;
                }

                budget.Used++;
                delay = ServerDelay(response, budget.Used);
                response.Dispose();
            }
            catch (ApiConnectionException) when (budget.Used < retries)
            {
                budget.Used++;
                delay = Backoff.Delay(InitialDelay, MaxDelay, budget.Used);
            }

            await TimeProviderDelay.Delay(delay, timeProvider, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<bool> IsTransientAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        return (int)response.StatusCode >= 500
            || response.StatusCode == (HttpStatusCode)429
            || (policy == RetryPolicy.Api
                && response.StatusCode == HttpStatusCode.Conflict
                && await ResponseReader.ReadProblemCodeAsync(response, cancellationToken).ConfigureAwait(false) == "conflict");
    }

    private TimeSpan ServerDelay(HttpResponseMessage response, int retry) =>
        ResponseReader.RetryAfter(response, timeProvider.GetUtcNow()) is { } retryAfter
            ? (retryAfter < MaxRetryAfter ? retryAfter : MaxRetryAfter)
            : Backoff.Delay(InitialDelay, MaxDelay, retry);
}

/// <summary>How many retries one call has used, across its 401 replay too.</summary>
internal sealed class RetryBudget
{
    /// <summary>Read and written by one attempt at a time, since a call's attempts run in turn.</summary>
    public int Used { get; set; }
}
