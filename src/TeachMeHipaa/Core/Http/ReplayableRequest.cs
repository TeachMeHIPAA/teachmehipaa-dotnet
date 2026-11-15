using System.Net.Http.Headers;

namespace TeachMeHipaa;

/// <summary>
/// A snapshot of a request that can be sent again. A request message can be
/// sent only once and its content may be a one-shot stream (or be disposed by
/// an <see cref="HttpClient"/> after sending), so the body is read into memory
/// once and every send gets a fresh copy.
/// </summary>
internal sealed class ReplayableRequest
{
    private readonly HttpMethod _method;
    private readonly Uri? _uri;
    private readonly Version _version;
    private readonly KeyValuePair<string, IEnumerable<string>>[] _headers;
    private readonly KeyValuePair<string, object?>[] _properties;
    private readonly byte[]? _content;
    private readonly KeyValuePair<string, IEnumerable<string>>[] _contentHeaders;

    private ReplayableRequest(HttpRequestMessage request, byte[]? content)
    {
        _method = request.Method;
        _uri = request.RequestUri;
        _version = request.Version;
        _headers = [.. request.Headers];
#if NET5_0_OR_GREATER
        _properties = [.. request.Options];
#else
        _properties = [.. request.Properties];
#endif
        _content = content;
        _contentHeaders = request.Content is null ? [] : [.. request.Content.Headers];
    }

    public static async Task<ReplayableRequest> CaptureAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is null)
        {
            return new ReplayableRequest(request, null);
        }
#if NET5_0_OR_GREATER
        var content = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
        var content = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
        return new ReplayableRequest(request, content);
    }

    /// <summary>A new, unsent copy of the request.</summary>
    /// <param name="authorization">Replaces the request's Authorization header when given.</param>
    public HttpRequestMessage Create(AuthenticationHeaderValue? authorization = null)
    {
        var copy = new HttpRequestMessage(_method, _uri) { Version = _version };
        foreach (var header in _headers)
        {
            copy.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (authorization is not null)
        {
            copy.Headers.Authorization = authorization;
        }

        foreach (var property in _properties)
        {
#if NET5_0_OR_GREATER
            ((IDictionary<string, object?>)copy.Options)[property.Key] = property.Value;
#else
            copy.Properties[property.Key] = property.Value;
#endif
        }

        if (_content is not null)
        {
            copy.Content = new ByteArrayContent(_content);
            foreach (var header in _contentHeaders)
            {
                copy.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return copy;
    }
}
