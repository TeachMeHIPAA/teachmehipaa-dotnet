namespace TeachMeHipaa;

/// <summary>
/// Per-call settings carried on the request itself, so each handler in the
/// pipeline reads the ones it acts on.
/// </summary>
internal static class RequestProperties
{
    /// <summary>The caller's <see cref="RequestOptions.Timeout"/>, read by <see cref="TimeoutHandler"/>.</summary>
    public static readonly RequestProperty<TimeSpan> Timeout = new("TeachMeHipaa.Timeout");

    /// <summary>The caller's <see cref="RequestOptions.MaxRetries"/>, read by <see cref="RetryHandler"/>.</summary>
    public static readonly RequestProperty<int> MaxRetries = new("TeachMeHipaa.MaxRetries");

    /// <summary>
    /// The IR's <c>retrySafe</c>: GETs, operations that carry an
    /// Idempotency-Key, and those idempotent by meaning. Requests without it
    /// are never retried.
    /// </summary>
    public static readonly RequestProperty<bool> RetrySafe = new("TeachMeHipaa.RetrySafe");
}

internal sealed class RequestProperty<T>(string name)
    where T : struct
{
#if NET5_0_OR_GREATER
    private readonly HttpRequestOptionsKey<T> _key = new(name);

    public void Set(HttpRequestMessage request, T value) => request.Options.Set(_key, value);

    public T? Get(HttpRequestMessage request) =>
        request.Options.TryGetValue(_key, out var value) ? value : null;
#else
    public void Set(HttpRequestMessage request, T value) => request.Properties[name] = value;

    public T? Get(HttpRequestMessage request) =>
        request.Properties.TryGetValue(name, out var value) ? (T?)value : null;
#endif
}
