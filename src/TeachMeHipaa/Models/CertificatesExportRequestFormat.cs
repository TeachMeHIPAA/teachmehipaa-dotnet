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
[JsonConverter(typeof(CertificatesExportRequestFormatJsonConverter))]
public readonly record struct CertificatesExportRequestFormat
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public CertificatesExportRequestFormat(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>csv</c>.</summary>
    public static CertificatesExportRequestFormat Csv { get; } = new("csv");

    /// <summary><c>zip</c>.</summary>
    public static CertificatesExportRequestFormat Zip { get; } = new("zip");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class CertificatesExportRequestFormatJsonConverter : JsonConverter<CertificatesExportRequestFormat>
{
    public override CertificatesExportRequestFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A CertificatesExportRequestFormat must be a string."));

    public override void Write(Utf8JsonWriter writer, CertificatesExportRequestFormat value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
