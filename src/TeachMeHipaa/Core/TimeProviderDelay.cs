namespace TeachMeHipaa;

/// <summary>Waits on the client's <see cref="TimeProvider"/>, so tests can run backoffs and polls instantly.</summary>
internal static class TimeProviderDelay
{
    /// <summary>Completes after <paramref name="delay"/> by <paramref name="timeProvider"/>'s clock, or is cancelled.</summary>
    public static Task Delay(TimeSpan delay, TimeProvider timeProvider, CancellationToken cancellationToken) =>
#if NET8_0_OR_GREATER
        Task.Delay(delay, timeProvider, cancellationToken);
#else
        timeProvider.Delay(delay, cancellationToken);
#endif
}
