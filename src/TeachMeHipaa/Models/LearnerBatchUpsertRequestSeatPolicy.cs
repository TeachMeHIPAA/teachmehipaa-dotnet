// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Applies to every item. <c>require_available</c> fails an item when no seat is free.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(LearnerBatchUpsertRequestSeatPolicyJsonConverter))]
public readonly record struct LearnerBatchUpsertRequestSeatPolicy
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public LearnerBatchUpsertRequestSeatPolicy(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>require_available</c>.</summary>
    public static LearnerBatchUpsertRequestSeatPolicy RequireAvailable { get; } = new("require_available");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class LearnerBatchUpsertRequestSeatPolicyJsonConverter : JsonConverter<LearnerBatchUpsertRequestSeatPolicy>
{
    public override LearnerBatchUpsertRequestSeatPolicy Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A LearnerBatchUpsertRequestSeatPolicy must be a string."));

    public override void Write(Utf8JsonWriter writer, LearnerBatchUpsertRequestSeatPolicy value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
