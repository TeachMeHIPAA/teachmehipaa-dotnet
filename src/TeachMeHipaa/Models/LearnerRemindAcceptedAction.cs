// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Always <c>remind</c>.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(LearnerRemindAcceptedActionJsonConverter))]
public readonly record struct LearnerRemindAcceptedAction
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public LearnerRemindAcceptedAction(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>remind</c>.</summary>
    public static LearnerRemindAcceptedAction Remind { get; } = new("remind");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class LearnerRemindAcceptedActionJsonConverter : JsonConverter<LearnerRemindAcceptedAction>
{
    public override LearnerRemindAcceptedAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A LearnerRemindAcceptedAction must be a string."));

    public override void Write(Utf8JsonWriter writer, LearnerRemindAcceptedAction value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
