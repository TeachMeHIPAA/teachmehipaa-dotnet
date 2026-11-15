using System.Net;

namespace TeachMeHipaa;

/// <summary>One invalid field from a <c>validation_failed</c> problem.</summary>
/// <param name="Path">The dot-joined field path, or <c>Idempotency-Key</c> for a missing header.</param>
/// <param name="Message">What is wrong with the field.</param>
public sealed record FieldError(string Path, string Message);

/// <summary>The base of every error the library raises about an API call.</summary>
public class TeachMeHipaaException : Exception
{
    /// <summary>Creates an error with a default message.</summary>
    public TeachMeHipaaException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public TeachMeHipaaException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public TeachMeHipaaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>The HTTP status, or null when no response arrived.</summary>
    public HttpStatusCode? StatusCode { get; init; }

    /// <summary>
    /// The machine-readable reason, such as <c>seat_unavailable</c>, or the
    /// OAuth <c>error</c> from the token endpoint. Branch on this; new codes
    /// may appear at any time.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>The problem's short summary.</summary>
    public string? Title { get; init; }

    /// <summary>The problem's explanation of this occurrence.</summary>
    public string? Detail { get; init; }

    /// <summary>Each invalid field, for a validation problem.</summary>
    public IReadOnlyList<FieldError> Errors { get; init; } = [];

    /// <summary>The <c>x-request-id</c> header. Quote it to TeachMeHIPAA support.</summary>
    public string? RequestId { get; init; }

    /// <summary>The response headers, looked up without regard to case.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>The response body as text, or null when no response arrived.</summary>
    public string? RawBody { get; init; }
}
