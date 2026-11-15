using System.Diagnostics.CodeAnalysis;

namespace TeachMeHipaa;

/// <summary>
/// <see cref="WebhookUtility"/> reached from a client, for code that already
/// holds one. Needs only the Signing secret and never makes a request.
/// </summary>
[SuppressMessage(
    "Performance",
    "CA1822:Mark members as static",
    Justification = "Instance members, so client.Webhooks reads like the client's services.")]
public sealed class TeachMeHipaaWebhooks
{
    private TeachMeHipaaWebhooks()
    {
    }

    internal static TeachMeHipaaWebhooks Instance { get; } = new();

    /// <inheritdoc cref="WebhookUtility.ConstructEvent(string, IEnumerable{KeyValuePair{string, string}}, string, string, TimeSpan?, DateTimeOffset?)"/>
    public WebhookDelivery ConstructEvent(
        string payload,
        IEnumerable<KeyValuePair<string, string>> headers,
        string secret,
        string path,
        TimeSpan? tolerance = null,
        DateTimeOffset? receivedAt = null) =>
        WebhookUtility.ConstructEvent(payload, headers, secret, path, tolerance, receivedAt);

    /// <inheritdoc cref="WebhookUtility.ConstructEvent(string, IEnumerable{KeyValuePair{string, IEnumerable{string}}}, string, string, TimeSpan?, DateTimeOffset?)"/>
    public WebhookDelivery ConstructEvent(
        string payload,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
        string secret,
        string path,
        TimeSpan? tolerance = null,
        DateTimeOffset? receivedAt = null) =>
        WebhookUtility.ConstructEvent(payload, headers, secret, path, tolerance, receivedAt);

    /// <inheritdoc cref="WebhookUtility.ConstructEvent(string, Func{string, string}, string, string, TimeSpan?, DateTimeOffset?)"/>
    public WebhookDelivery ConstructEvent(
        string payload,
        Func<string, string?> getHeader,
        string secret,
        string path,
        TimeSpan? tolerance = null,
        DateTimeOffset? receivedAt = null) =>
        WebhookUtility.ConstructEvent(payload, getHeader, secret, path, tolerance, receivedAt);

    /// <inheritdoc cref="WebhookUtility.ConstructEvent(string, IEnumerable{KeyValuePair{string, string}}, string, string, TimeSpan?, DateTimeOffset?)"/>
    public WebhookDelivery ConstructEvent(
        ReadOnlySpan<byte> payload,
        IEnumerable<KeyValuePair<string, string>> headers,
        string secret,
        string path,
        TimeSpan? tolerance = null,
        DateTimeOffset? receivedAt = null) =>
        WebhookUtility.ConstructEvent(payload, headers, secret, path, tolerance, receivedAt);

    /// <inheritdoc cref="WebhookUtility.ConstructEvent(string, IEnumerable{KeyValuePair{string, IEnumerable{string}}}, string, string, TimeSpan?, DateTimeOffset?)"/>
    public WebhookDelivery ConstructEvent(
        ReadOnlySpan<byte> payload,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
        string secret,
        string path,
        TimeSpan? tolerance = null,
        DateTimeOffset? receivedAt = null) =>
        WebhookUtility.ConstructEvent(payload, headers, secret, path, tolerance, receivedAt);

    /// <inheritdoc cref="WebhookUtility.ConstructEvent(string, Func{string, string}, string, string, TimeSpan?, DateTimeOffset?)"/>
    public WebhookDelivery ConstructEvent(
        ReadOnlySpan<byte> payload,
        Func<string, string?> getHeader,
        string secret,
        string path,
        TimeSpan? tolerance = null,
        DateTimeOffset? receivedAt = null) =>
        WebhookUtility.ConstructEvent(payload, getHeader, secret, path, tolerance, receivedAt);

    /// <inheritdoc cref="WebhookUtility.GenerateTestHeaders(string, string, string, DateTimeOffset?, string, string, string)"/>
    public IReadOnlyDictionary<string, string> GenerateTestHeaders(
        string payload,
        string secret,
        string path,
        DateTimeOffset? timestamp = null,
        string? eventType = null,
        string? eventId = null,
        string? keyId = null) =>
        WebhookUtility.GenerateTestHeaders(payload, secret, path, timestamp, eventType, eventId, keyId);

    /// <inheritdoc cref="WebhookUtility.GenerateTestHeaders(string, string, string, DateTimeOffset?, string, string, string)"/>
    public IReadOnlyDictionary<string, string> GenerateTestHeaders(
        ReadOnlySpan<byte> payload,
        string secret,
        string path,
        DateTimeOffset? timestamp = null,
        string? eventType = null,
        string? eventId = null,
        string? keyId = null) =>
        WebhookUtility.GenerateTestHeaders(payload, secret, path, timestamp, eventType, eventId, keyId);
}
