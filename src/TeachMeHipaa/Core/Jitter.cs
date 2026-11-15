namespace TeachMeHipaa;

/// <summary>
/// Spreads out retries and polls so that many clients don't send in step.
/// It is not security, so a shared non-cryptographic generator is enough.
/// </summary>
internal static class Jitter
{
#if !NET6_0_OR_GREATER
    private static readonly Random Random = new();
#endif

    /// <summary>A number from 0 up to, not including, 1.</summary>
#if NET6_0_OR_GREATER
    public static double Next() => Random.Shared.NextDouble();
#else
    public static double Next()
    {
        lock (Random)
        {
            return Random.NextDouble();
        }
    }
#endif
}
