// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AccessTokenResponseTokenType</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(AccessTokenResponseTokenTypeJsonConverter))]
public readonly record struct AccessTokenResponseTokenType
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public AccessTokenResponseTokenType(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>Bearer</c>.</summary>
    public static AccessTokenResponseTokenType Bearer { get; } = new("Bearer");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class AccessTokenResponseTokenTypeJsonConverter : JsonConverter<AccessTokenResponseTokenType>
{
    public override AccessTokenResponseTokenType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A AccessTokenResponseTokenType must be a string."));

    public override void Write(Utf8JsonWriter writer, AccessTokenResponseTokenType value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
