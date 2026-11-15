namespace TeachMeHipaa;

/// <summary>
/// Exponential backoff with jitter, for retries and Operation polls, so that
/// many clients don't send in step.
/// </summary>
internal static class Backoff
{
#if !NET6_0_OR_GREATER
    // Jitter is not security, so a shared non-cryptographic generator is enough.
    private static readonly Random Random = new();
#endif

    /// <summary>
    /// How long to wait before <paramref name="attempt"/>: the ceiling doubles
    /// from <paramref name="initial"/> up to <paramref name="cap"/> (an initial
    /// delay longer than the cap stays as it is), jitter picks 50–100 % of it,
    /// and nothing waits less than <paramref name="initial"/>. Stripe's schedule.
    /// </summary>
    /// <param name="initial">The first and shortest delay.</param>
    /// <param name="cap">The longest ceiling.</param>
    /// <param name="attempt">1 for the first wait, 2 for the second, and so on.</param>
    public static TimeSpan Delay(TimeSpan initial, TimeSpan cap, int attempt)
    {
        var ceiling = Math.Min(initial.Ticks * Math.Pow(2, attempt - 1), Math.Max(cap.Ticks, initial.Ticks));
        return TimeSpan.FromTicks(Math.Max(initial.Ticks, (long)(ceiling * (0.5 + (NextJitter() * 0.5)))));
    }

    /// <summary>A number from 0 up to, not including, 1.</summary>
#if NET6_0_OR_GREATER
    private static double NextJitter() => Random.Shared.NextDouble();
#else
    private static double NextJitter()
    {
        lock (Random)
        {
            return Random.NextDouble();
        }
    }
#endif
}
