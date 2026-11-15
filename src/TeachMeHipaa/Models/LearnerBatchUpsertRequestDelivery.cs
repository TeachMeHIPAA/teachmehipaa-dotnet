// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Applies to every item. <c>email</c> (default) sends invites; <c>none</c> sends nothing.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(LearnerBatchUpsertRequestDeliveryJsonConverter))]
public readonly record struct LearnerBatchUpsertRequestDelivery
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public LearnerBatchUpsertRequestDelivery(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>email</c>.</summary>
    public static LearnerBatchUpsertRequestDelivery Email { get; } = new("email");

    /// <summary><c>none</c>.</summary>
    public static LearnerBatchUpsertRequestDelivery None { get; } = new("none");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class LearnerBatchUpsertRequestDeliveryJsonConverter : JsonConverter<LearnerBatchUpsertRequestDelivery>
{
    public override LearnerBatchUpsertRequestDelivery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A LearnerBatchUpsertRequestDelivery must be a string."));

    public override void Write(Utf8JsonWriter writer, LearnerBatchUpsertRequestDelivery value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
