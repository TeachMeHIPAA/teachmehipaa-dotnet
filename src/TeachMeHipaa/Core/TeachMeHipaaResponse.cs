using System.Net;

namespace TeachMeHipaa;

/// <summary>What the API said about the response an object came from.</summary>
public sealed class TeachMeHipaaResponse
{
    /// <summary>The HTTP status.</summary>
    public HttpStatusCode StatusCode { get; init; }

    /// <summary>Every response header, looked up without regard to case. Repeated headers are joined with <c>", "</c>.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>The <c>x-request-id</c> header. Quote it to TeachMeHIPAA support.</summary>
    public string? RequestId { get; init; }

    /// <summary>The <c>ETag</c> header, for a later conditional update.</summary>
    public string? ETag { get; init; }

    internal static TeachMeHipaaResponse From(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var all = response.Content is null
            ? response.Headers
            : response.Headers.Concat(response.Content.Headers);
        foreach (var header in all)
        {
            headers[header.Key] = string.Join(", ", header.Value);
        }

        return new TeachMeHipaaResponse
        {
            StatusCode = response.StatusCode,
            Headers = headers,
            RequestId = headers.TryGetValue("x-request-id", out var requestId) ? requestId : null,
            ETag = headers.TryGetValue("ETag", out var etag) ? etag : null,
        };
    }
}
