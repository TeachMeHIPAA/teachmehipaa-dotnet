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
[JsonConverter(typeof(EnrollmentsExportRequestFormatJsonConverter))]
public readonly record struct EnrollmentsExportRequestFormat
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public EnrollmentsExportRequestFormat(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>csv</c>.</summary>
    public static EnrollmentsExportRequestFormat Csv { get; } = new("csv");

    /// <summary><c>xlsx</c>.</summary>
    public static EnrollmentsExportRequestFormat Xlsx { get; } = new("xlsx");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class EnrollmentsExportRequestFormatJsonConverter : JsonConverter<EnrollmentsExportRequestFormat>
{
    public override EnrollmentsExportRequestFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A EnrollmentsExportRequestFormat must be a string."));

    public override void Write(Utf8JsonWriter writer, EnrollmentsExportRequestFormat value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
