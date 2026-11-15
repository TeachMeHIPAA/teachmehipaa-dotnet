using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace TeachMeHipaa;

internal static class ResponseReader
{
    public static async Task<string> ReadTextAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return string.Empty;
        }
#if NET5_0_OR_GREATER
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
    }

    public static async Task<byte[]> ReadBytesAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return [];
        }
#if NET5_0_OR_GREATER
        return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
    }

    public static async Task<Stream> ReadStreamAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return Stream.Null;
        }
#if NET5_0_OR_GREATER
        return await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif
    }

    /// <summary>The file name from <c>Content-Disposition</c>, preferring RFC 6266's UTF-8 <c>filename*</c>.</summary>
    public static string? FileName(HttpResponseMessage response)
    {
        var disposition = response.Content?.Headers.ContentDisposition;
        return (disposition?.FileNameStar ?? disposition?.FileName)?.Trim('"');
    }

    // Unknown paths answer with a 200 HTML page, so a body is parsed only when
    // it claims to be JSON.
    public static bool IsJson(HttpResponseMessage response)
    {
        var mediaType = response.Content?.Headers.ContentType?.MediaType;
        return mediaType is not null
            && (string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase)
                || (mediaType.StartsWith("application/", StringComparison.OrdinalIgnoreCase)
                    && mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)));
    }

    public static T? Parse<T>(HttpResponseMessage response, string text, JsonTypeInfo<T> typeInfo)
        where T : class
    {
        if (!IsJson(response))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize(text, typeInfo);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Maps a response the caller can't use to the exception for its status.</summary>
    public static TeachMeHipaaException ApiError(HttpResponseMessage response, string text, DateTimeOffset now)
    {
        var problem = Parse(response, text, CoreJsonContext.Default.ProblemDocument);
        var summary = string.Join(": ", new[] { problem?.Title, problem?.Detail }.Where(part => part is not null));
        var message = summary.Length == 0 ? FallbackMessage(response) : summary;
        var exception = ExceptionFor(response, problem?.Code, message, now);
        return exception.Describe(
            TeachMeHipaaResponse.From(response),
            text,
            problem?.Code,
            problem?.Title,
            problem?.Detail,
            (problem?.Errors ?? [])
                .Where(error => error.Path is not null && error.Message is not null)
                .Select(error => new FieldError(error.Path!, error.Message!))
                .ToArray());
    }

    /// <summary>
    /// The token endpoint answers with RFC 6749 <c>{error, error_description}</c>;
    /// anything else from it (an HTML page, a bare 502) maps like any response.
    /// </summary>
    public static TeachMeHipaaException OAuthError(HttpResponseMessage response, string text, DateTimeOffset now)
    {
        var oauth = Parse(response, text, CoreJsonContext.Default.TokenEndpointError);
        if (oauth?.Error is null)
        {
            return ApiError(response, text, now);
        }

        var summary = string.Join(": ", new[] { oauth.Error, oauth.ErrorDescription }.Where(part => part is not null));
        return new OAuthException(summary).Describe(
            TeachMeHipaaResponse.From(response),
            text,
            oauth.Error,
            title: null,
            oauth.ErrorDescription,
            []);
    }

    /// <summary>The problem <c>code</c>, for a response that carries one.</summary>
    public static async Task<string?> ReadProblemCodeAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        Parse(response, await ReadTextAsync(response, cancellationToken).ConfigureAwait(false), CoreJsonContext.Default.ProblemDocument)?.Code;

    /// <summary>
    /// <c>Retry-After</c> as a delay from <paramref name="now"/>. It may be
    /// delay-seconds or an HTTP-date (RFC 9110 §10.2.3); anything else counts
    /// as absent.
    /// </summary>
    public static TimeSpan? RetryAfter(HttpResponseMessage response, DateTimeOffset now) =>
        response.Headers.RetryAfter switch
        {
            { Delta: { } delta } => delta,
            { Date: { } date } => date > now ? date - now : TimeSpan.Zero,
            _ => null,
        };

    private static TeachMeHipaaException ExceptionFor(HttpResponseMessage response, string? code, string message, DateTimeOffset now)
    {
        // A success status only gets here with a body the library can't use.
        if (response.IsSuccessStatusCode)
        {
            return new ApiException(message);
        }

        return (int)response.StatusCode switch
        {
            401 => new AuthenticationException(message),
            403 => new PermissionDeniedException(message),
            404 => new NotFoundException(message),
            409 => new ConflictException(message),
            412 or 428 => new PreconditionFailedException(message),
            422 when code == "idempotency_key_reuse" => new IdempotencyException(message),
            400 or 405 or 406 or 415 or 422 => new InvalidRequestException(message),
            429 => new RateLimitException(message) { RetryAfter = RetryAfter(response, now) },
            _ => new ApiException(message),
        };
    }

    private static string FallbackMessage(HttpResponseMessage response)
    {
        var status = ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture);
        return IsJson(response)
            ? $"TeachMeHIPAA API responded with HTTP {status}."
            : $"Expected a JSON response but got {response.Content?.Headers.ContentType?.MediaType ?? "no content type"} with HTTP {status}; check the base URL.";
    }
}
