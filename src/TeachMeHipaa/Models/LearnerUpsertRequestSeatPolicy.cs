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
[JsonConverter(typeof(LearnerUpsertRequestSeatPolicyJsonConverter))]
public readonly record struct LearnerUpsertRequestSeatPolicy
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public LearnerUpsertRequestSeatPolicy(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>require_available</c>.</summary>
    public static LearnerUpsertRequestSeatPolicy RequireAvailable { get; } = new("require_available");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class LearnerUpsertRequestSeatPolicyJsonConverter : JsonConverter<LearnerUpsertRequestSeatPolicy>
{
    public override LearnerUpsertRequestSeatPolicy Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A LearnerUpsertRequestSeatPolicy must be a string."));

    public override void Write(Utf8JsonWriter writer, LearnerUpsertRequestSeatPolicy value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
