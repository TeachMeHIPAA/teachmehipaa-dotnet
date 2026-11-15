// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>How the invite was delivered: <c>email</c>, or <c>none</c> when you deliver it.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(LearnerCreatedDeliveryJsonConverter))]
public readonly record struct LearnerCreatedDelivery
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public LearnerCreatedDelivery(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>email</c>.</summary>
    public static LearnerCreatedDelivery Email { get; } = new("email");

    /// <summary><c>none</c>.</summary>
    public static LearnerCreatedDelivery None { get; } = new("none");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class LearnerCreatedDeliveryJsonConverter : JsonConverter<LearnerCreatedDelivery>
{
    public override LearnerCreatedDelivery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A LearnerCreatedDelivery must be a string."));

    public override void Write(Utf8JsonWriter writer, LearnerCreatedDelivery value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
