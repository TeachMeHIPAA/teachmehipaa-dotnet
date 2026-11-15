// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary><c>email</c> (default) sends the invite email. <c>none</c> sends nothing and returns <c>joinUrl</c> for you to deliver.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(LearnerUpsertRequestDeliveryJsonConverter))]
public readonly record struct LearnerUpsertRequestDelivery
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public LearnerUpsertRequestDelivery(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>email</c>.</summary>
    public static LearnerUpsertRequestDelivery Email { get; } = new("email");

    /// <summary><c>none</c>.</summary>
    public static LearnerUpsertRequestDelivery None { get; } = new("none");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class LearnerUpsertRequestDeliveryJsonConverter : JsonConverter<LearnerUpsertRequestDelivery>
{
    public override LearnerUpsertRequestDelivery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A LearnerUpsertRequestDelivery must be a string."));

    public override void Write(Utf8JsonWriter writer, LearnerUpsertRequestDelivery value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
