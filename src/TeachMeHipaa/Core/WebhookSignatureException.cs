namespace TeachMeHipaa;

/// <summary>
/// A Delivery failed verification: respond with a 4xx and don't act on it.
/// <see cref="TeachMeHipaaException.Code"/> says why. The error never holds
/// the body or headers, which may carry PHI.
/// </summary>
public class WebhookSignatureException : TeachMeHipaaException
{
    /// <summary>The <c>x-tmh-signature</c> or <c>x-tmh-timestamp</c> header is missing or empty.</summary>
    public const string MissingHeader = "missing_header";

    /// <summary>The <c>x-tmh-timestamp</c> header is not an ISO 8601 date-time with an offset.</summary>
    public const string InvalidTimestamp = "invalid_timestamp";

    /// <summary>The Delivery was signed more than the tolerance before or after it arrived.</summary>
    public const string TimestampOutsideTolerance = "timestamp_outside_tolerance";

    /// <summary>The <c>x-tmh-body-hash</c> header does not match the body.</summary>
    public const string BodyHashMismatch = "body_hash_mismatch";

    /// <summary>The signature does not match the body, path and Signing secret.</summary>
    public const string SignatureMismatch = "signature_mismatch";

    /// <summary>Creates an error with a default message.</summary>
    public WebhookSignatureException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public WebhookSignatureException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public WebhookSignatureException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
