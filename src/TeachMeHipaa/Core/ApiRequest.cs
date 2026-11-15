using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace TeachMeHipaa;

/// <summary>Describes one API operation call. The generated service layer builds these.</summary>
/// <param name="Method">The HTTP method.</param>
/// <param name="Path">The path below the base URL, with path parameters already escaped.</param>
internal sealed record ApiRequest(HttpMethod Method, string Path)
{
    /// <summary>Whether to prefix the path with <c>/organizations/{orgId}</c>.</summary>
    public bool OrganizationScoped { get; init; }

    /// <summary>
    /// False for the IR's public operations (<c>security: none</c>), which
    /// are sent without an Access token, so none is ever minted for them.
    /// </summary>
    public bool Authenticated { get; init; } = true;

    /// <summary>
    /// The IR's <c>retrySafe</c>: GETs, operations that take an
    /// Idempotency-Key, and those idempotent by meaning. PATCH and DELETE never are.
    /// </summary>
    public bool RetrySafe { get; init; }

    /// <summary>
    /// The IR's <c>idempotencyKey: required</c>. Each call then gets one
    /// generated key, reused by every retry, unless the caller passes their own.
    /// </summary>
    public bool IdempotencyKeyRequired { get; init; }

    /// <summary>The media type of a success response; JSON when null.</summary>
    public string? Accept { get; init; }

    /// <summary>Query parameters; null values are left out.</summary>
    public IEnumerable<KeyValuePair<string, string?>>? Query { get; init; }

    /// <summary>The request body, or null for none.</summary>
    public RequestBody? Body { get; init; }
}

/// <summary>A request body, created fresh for the one message that sends it.</summary>
internal sealed class RequestBody
{
    private readonly Func<HttpContent> _createContent;

    private RequestBody(Func<HttpContent> createContent) => _createContent = createContent;

    public HttpContent CreateContent() => _createContent();

    public static RequestBody Json<T>(T value, JsonTypeInfo<T> typeInfo) =>
        Serialized(value, typeInfo, "application/json");

    public static RequestBody MergePatch<T>(T value, JsonTypeInfo<T> typeInfo) =>
        Serialized(value, typeInfo, "application/merge-patch+json");

    /// <summary>A <c>multipart/form-data</c> body; null parts (optional fields left unset) are left out.</summary>
    public static RequestBody Multipart(params MultipartPart?[] parts) =>
        new(() =>
        {
            var content = new MultipartFormDataContent();
            foreach (var part in parts)
            {
                if (part is null)
                {
                    continue;
                }

                if (part.FileName is null)
                {
                    content.Add(part.CreateContent(), part.Name);
                }
                else
                {
                    content.Add(part.CreateContent(), part.Name, part.FileName);
                }
            }

            return content;
        });

    private static RequestBody Serialized<T>(T value, JsonTypeInfo<T> typeInfo, string mediaType)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, typeInfo);
        return new(() =>
        {
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            return content;
        });
    }
}

/// <summary>
/// One part of a multipart body: the IR's binary parts are files with their
/// content type, every other field is a JSON part.
/// </summary>
internal sealed class MultipartPart
{
    private readonly Func<HttpContent> _createContent;

    private MultipartPart(string name, string? fileName, Func<HttpContent> createContent)
    {
        Name = name;
        FileName = fileName;
        _createContent = createContent;
    }

    public string Name { get; }

    public string? FileName { get; }

    public HttpContent CreateContent() => _createContent();

    public static MultipartPart File(string name, Stream data, string contentType) =>
        new(name, name, () =>
        {
            var content = new BorrowedStreamContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return content;
        });

    public static MultipartPart Json<T>(string name, T value, JsonTypeInfo<T> typeInfo)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, typeInfo);
        return new(name, null, () =>
        {
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        });
    }

    // StreamContent would dispose the caller's stream along with the request.
    private sealed class BorrowedStreamContent(Stream stream) : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream target, TransportContext? context) =>
            stream.CopyToAsync(target);

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
