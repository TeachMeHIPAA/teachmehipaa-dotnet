// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The file format. Only <c>csv</c> today.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(LearnersExportRequestFormatJsonConverter))]
public readonly record struct LearnersExportRequestFormat
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public LearnersExportRequestFormat(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>csv</c>.</summary>
    public static LearnersExportRequestFormat Csv { get; } = new("csv");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class LearnersExportRequestFormatJsonConverter : JsonConverter<LearnersExportRequestFormat>
{
    public override LearnersExportRequestFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A LearnersExportRequestFormat must be a string."));

    public override void Write(Utf8JsonWriter writer, LearnersExportRequestFormat value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
