// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>OAuthErrorCode</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(OAuthErrorCodeJsonConverter))]
public readonly record struct OAuthErrorCode
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public OAuthErrorCode(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>invalid_request</c>.</summary>
    public static OAuthErrorCode InvalidRequest { get; } = new("invalid_request");

    /// <summary><c>invalid_client</c>.</summary>
    public static OAuthErrorCode InvalidClient { get; } = new("invalid_client");

    /// <summary><c>invalid_grant</c>.</summary>
    public static OAuthErrorCode InvalidGrant { get; } = new("invalid_grant");

    /// <summary><c>invalid_scope</c>.</summary>
    public static OAuthErrorCode InvalidScope { get; } = new("invalid_scope");

    /// <summary><c>unsupported_grant_type</c>.</summary>
    public static OAuthErrorCode UnsupportedGrantType { get; } = new("unsupported_grant_type");

    /// <summary><c>slow_down</c>.</summary>
    public static OAuthErrorCode SlowDown { get; } = new("slow_down");

    /// <summary><c>temporarily_unavailable</c>.</summary>
    public static OAuthErrorCode TemporarilyUnavailable { get; } = new("temporarily_unavailable");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class OAuthErrorCodeJsonConverter : JsonConverter<OAuthErrorCode>
{
    public override OAuthErrorCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A OAuthErrorCode must be a string."));

    public override void Write(Utf8JsonWriter writer, OAuthErrorCode value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
