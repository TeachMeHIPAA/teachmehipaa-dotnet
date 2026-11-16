// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>TokenIntrospectionMode</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(TokenIntrospectionModeJsonConverter))]
public readonly record struct TokenIntrospectionMode
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public TokenIntrospectionMode(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>test</c>.</summary>
    public static TokenIntrospectionMode Test { get; } = new("test");

    /// <summary><c>live</c>.</summary>
    public static TokenIntrospectionMode Live { get; } = new("live");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class TokenIntrospectionModeJsonConverter : JsonConverter<TokenIntrospectionMode>
{
    public override TokenIntrospectionMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A TokenIntrospectionMode must be a string."));

    public override void Write(Utf8JsonWriter writer, TokenIntrospectionMode value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
