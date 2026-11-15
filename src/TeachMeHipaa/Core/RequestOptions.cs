namespace TeachMeHipaa;

/// <summary>Per-call settings, passed after a method's own options.</summary>
public sealed class RequestOptions
{
    /// <summary>
    /// Sent as <c>x-request-id</c> so you can correlate your logs with
    /// TeachMeHIPAA support. Must match <c>^[A-Za-z0-9._-]{1,128}$</c> or the
    /// API replaces it.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>How long to wait for each response; overrides the client's <see cref="TeachMeHipaaClientOptions.Timeout"/>.</summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Retries for this call if it fails transiently; overrides the client's
    /// <see cref="TeachMeHipaaClientOptions.MaxRetries"/>. Only requests that
    /// are safe to replay are ever retried.
    /// </summary>
    public int? MaxRetries { get; set; }

    /// <summary>
    /// Replaces the generated <c>Idempotency-Key</c>. Reuse one key to make a
    /// call you repeat yourself, such as a sync job's replay, run at most once.
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Sent as <c>If-Match</c>: the <see cref="TeachMeHipaaResponse.ETag"/> an
    /// object was read with, so an update fails rather than overwrite a newer change.
    /// </summary>
    public string? IfMatch { get; set; }
}
