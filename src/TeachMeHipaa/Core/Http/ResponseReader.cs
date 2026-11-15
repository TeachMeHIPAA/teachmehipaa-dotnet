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

    public static TeachMeHipaaException ApiError(HttpResponseMessage response, string text)
    {
        var problem = Parse(response, text, CoreJsonContext.Default.ProblemDocument);
        var summary = string.Join(": ", new[] { problem?.Title, problem?.Detail }.Where(part => part is not null));
        var metadata = TeachMeHipaaResponse.From(response);
        return new TeachMeHipaaException(summary.Length == 0 ? FallbackMessage(response) : summary)
        {
            StatusCode = response.StatusCode,
            Code = problem?.Code,
            Title = problem?.Title,
            Detail = problem?.Detail,
            Errors = (problem?.Errors ?? [])
                .Where(error => error.Path is not null && error.Message is not null)
                .Select(error => new FieldError(error.Path!, error.Message!))
                .ToArray(),
            RequestId = metadata.RequestId,
            Headers = metadata.Headers,
            RawBody = text,
        };
    }

    /// <summary>The token endpoint answers with RFC 6749 <c>{error, error_description}</c>, not a problem.</summary>
    public static TeachMeHipaaException OAuthError(HttpResponseMessage response, string text)
    {
        var oauth = Parse(response, text, CoreJsonContext.Default.OAuthTokenError);
        var summary = string.Join(": ", new[] { oauth?.Error, oauth?.ErrorDescription }.Where(part => part is not null));
        var metadata = TeachMeHipaaResponse.From(response);
        return new TeachMeHipaaException(summary.Length == 0 ? FallbackMessage(response) : summary)
        {
            StatusCode = response.StatusCode,
            Code = oauth?.Error,
            Detail = oauth?.ErrorDescription,
            RequestId = metadata.RequestId,
            Headers = metadata.Headers,
            RawBody = text,
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
