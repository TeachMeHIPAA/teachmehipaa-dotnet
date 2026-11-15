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
}
