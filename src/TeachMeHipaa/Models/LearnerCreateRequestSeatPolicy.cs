// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary><c>require_available</c> fails with <c>409</c> when no seat is free.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(LearnerCreateRequestSeatPolicyJsonConverter))]
public readonly record struct LearnerCreateRequestSeatPolicy
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public LearnerCreateRequestSeatPolicy(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>require_available</c>.</summary>
    public static LearnerCreateRequestSeatPolicy RequireAvailable { get; } = new("require_available");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class LearnerCreateRequestSeatPolicyJsonConverter : JsonConverter<LearnerCreateRequestSeatPolicy>
{
    public override LearnerCreateRequestSeatPolicy Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A LearnerCreateRequestSeatPolicy must be a string."));

    public override void Write(Utf8JsonWriter writer, LearnerCreateRequestSeatPolicy value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
