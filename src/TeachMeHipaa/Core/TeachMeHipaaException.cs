using System.Net;

namespace TeachMeHipaa;

/// <summary>One invalid field from a <c>validation_failed</c> problem.</summary>
/// <param name="Path">The dot-joined field path, or <c>Idempotency-Key</c> for a missing header.</param>
/// <param name="Message">What is wrong with the field.</param>
public sealed record FieldError(string Path, string Message);

/// <summary>
/// The base of every error the library raises about an API call. Catch a
/// subclass to handle one kind of failure.
/// </summary>
public class TeachMeHipaaException : Exception
{
    // Backing fields rather than auto-properties so the library can fill in
    // whichever subclass a status maps to, while callers still get init-only
    // properties for building their own in tests.
    private HttpStatusCode? _statusCode;
    private string? _code;
    private string? _title;
    private string? _detail;
    private IReadOnlyList<FieldError> _errors = [];
    private string? _requestId;
    private IReadOnlyDictionary<string, string> _headers =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private string? _rawBody;

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
    public HttpStatusCode? StatusCode { get => _statusCode; init => _statusCode = value; }

    /// <summary>
    /// The machine-readable reason, such as <c>seat_unavailable</c>, or the
    /// OAuth <c>error</c> from the token endpoint. Branch on this; new codes
    /// may appear at any time.
    /// </summary>
    public string? Code { get => _code; init => _code = value; }

    /// <summary>The problem's short summary.</summary>
    public string? Title { get => _title; init => _title = value; }

    /// <summary>The problem's explanation of this occurrence.</summary>
    public string? Detail { get => _detail; init => _detail = value; }

    /// <summary>Each invalid field, for a validation problem.</summary>
    public IReadOnlyList<FieldError> Errors { get => _errors; init => _errors = value; }

    /// <summary>The <c>x-request-id</c> header. Quote it to TeachMeHIPAA support.</summary>
    public string? RequestId { get => _requestId; init => _requestId = value; }

    /// <summary>The response headers, looked up without regard to case.</summary>
    public IReadOnlyDictionary<string, string> Headers { get => _headers; init => _headers = value; }

    /// <summary>The response body as text, or null when no response arrived.</summary>
    public string? RawBody { get => _rawBody; init => _rawBody = value; }

    internal TeachMeHipaaException Describe(
        TeachMeHipaaResponse response,
        string rawBody,
        string? code,
        string? title,
        string? detail,
        IReadOnlyList<FieldError> errors)
    {
        _statusCode = response.StatusCode;
        _headers = response.Headers;
        _requestId = response.RequestId;
        _rawBody = rawBody;
        _code = code;
        _title = title;
        _detail = detail;
        _errors = errors;
        return this;
    }
}

/// <summary>401: the Access token was missing, expired or revoked.</summary>
public class AuthenticationException : TeachMeHipaaException
{
    /// <summary>Creates an error with a default message.</summary>
    public AuthenticationException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public AuthenticationException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public AuthenticationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>403: the Access token lacks a Scope, or the Organization may not do this.</summary>
public class PermissionDeniedException : TeachMeHipaaException
{
    /// <summary>Creates an error with a default message.</summary>
    public PermissionDeniedException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public PermissionDeniedException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public PermissionDeniedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>404: no such object in this Organization.</summary>
public class NotFoundException : TeachMeHipaaException
{
    /// <summary>Creates an error with a default message.</summary>
    public NotFoundException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>409: the object's state forbids this, such as <c>seat_unavailable</c>.</summary>
public class ConflictException : TeachMeHipaaException
{
    /// <summary>Creates an error with a default message.</summary>
    public ConflictException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public ConflictException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>412 for a stale <c>If-Match</c>, 428 for a missing one.</summary>
public class PreconditionFailedException : TeachMeHipaaException
{
    /// <summary>Creates an error with a default message.</summary>
    public PreconditionFailedException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public PreconditionFailedException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public PreconditionFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>400, 405, 406, 415 or 422: the request itself is wrong.</summary>
public class InvalidRequestException : TeachMeHipaaException
{
    /// <summary>Creates an error with a default message.</summary>
    public InvalidRequestException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public InvalidRequestException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public InvalidRequestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>422 <c>idempotency_key_reuse</c>: the Idempotency-Key was already used with another body.</summary>
public class IdempotencyException : InvalidRequestException
{
    /// <summary>Creates an error with a default message.</summary>
    public IdempotencyException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public IdempotencyException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public IdempotencyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>429: too many requests.</summary>
public class RateLimitException : TeachMeHipaaException
{
    /// <summary>Creates an error with a default message.</summary>
    public RateLimitException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public RateLimitException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public RateLimitException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>How long the API asked you to wait, from <c>Retry-After</c>; null when it didn't say.</summary>
    public TimeSpan? RetryAfter { get; init; }
}

/// <summary>A 5xx, a status the library doesn't expect, or a response body it could not understand.</summary>
public class ApiException : TeachMeHipaaException
{
    /// <summary>Creates an error with a default message.</summary>
    public ApiException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public ApiException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public ApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>The token endpoint refused to issue an Access token, such as <c>invalid_client</c>.</summary>
public class OAuthException : TeachMeHipaaException
{
    /// <summary>Creates an error with a default message.</summary>
    public OAuthException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public OAuthException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public OAuthException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// No response arrived: the network failed or the body was cut off. The
/// <see cref="Exception.InnerException"/> is the underlying failure.
/// </summary>
public class ApiConnectionException : TeachMeHipaaException
{
    /// <summary>Creates an error with a default message.</summary>
    public ApiConnectionException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public ApiConnectionException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public ApiConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>No response arrived within the timeout.</summary>
public class ApiTimeoutException : ApiConnectionException
{
    /// <summary>Creates an error with a default message.</summary>
    public ApiTimeoutException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public ApiTimeoutException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public ApiTimeoutException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
