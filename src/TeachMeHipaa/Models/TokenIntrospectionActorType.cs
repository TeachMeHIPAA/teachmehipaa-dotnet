// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary><c>client</c> for an API client, <c>user</c> for a person.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(TokenIntrospectionActorTypeJsonConverter))]
public readonly record struct TokenIntrospectionActorType
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public TokenIntrospectionActorType(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>user</c>.</summary>
    public static TokenIntrospectionActorType User { get; } = new("user");

    /// <summary><c>client</c>.</summary>
    public static TokenIntrospectionActorType Client { get; } = new("client");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class TokenIntrospectionActorTypeJsonConverter : JsonConverter<TokenIntrospectionActorType>
{
    public override TokenIntrospectionActorType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A TokenIntrospectionActorType must be a string."));

    public override void Write(Utf8JsonWriter writer, TokenIntrospectionActorType value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
