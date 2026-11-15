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
    /// The IR's <c>retrySafe</c>: GETs, operations that take an
    /// Idempotency-Key, and those idempotent by meaning. PATCH and DELETE never are.
    /// </summary>
    public bool RetrySafe { get; init; }

    /// <summary>
    /// The IR's <c>idempotencyKey: required</c>. Each call then gets one
    /// generated key, reused by every retry, unless the caller passes their own.
    /// </summary>
    public bool IdempotencyKeyRequired { get; init; }

    /// <summary>Query parameters; null values are left out.</summary>
    public IEnumerable<KeyValuePair<string, string?>>? Query { get; init; }

    /// <summary>The request body, or null for none.</summary>
    public RequestBody? Body { get; init; }
}

/// <summary>A serialized request body and its media type.</summary>
internal sealed class RequestBody
{
    private RequestBody(byte[] content, string contentType)
    {
        Content = content;
        ContentType = contentType;
    }

    public byte[] Content { get; }

    public string ContentType { get; }

    public static RequestBody Json<T>(T value, JsonTypeInfo<T> typeInfo) =>
        new(JsonSerializer.SerializeToUtf8Bytes(value, typeInfo), "application/json");

    public static RequestBody MergePatch<T>(T value, JsonTypeInfo<T> typeInfo) =>
        new(JsonSerializer.SerializeToUtf8Bytes(value, typeInfo), "application/merge-patch+json");
}
