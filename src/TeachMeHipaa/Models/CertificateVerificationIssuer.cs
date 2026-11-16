// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Who issued it.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(CertificateVerificationIssuerJsonConverter))]
public readonly record struct CertificateVerificationIssuer
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public CertificateVerificationIssuer(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>TeachMeHIPAA</c>.</summary>
    public static CertificateVerificationIssuer TeachMeHIPAA { get; } = new("TeachMeHIPAA");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class CertificateVerificationIssuerJsonConverter : JsonConverter<CertificateVerificationIssuer>
{
    public override CertificateVerificationIssuer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A CertificateVerificationIssuer must be a string."));

    public override void Write(Utf8JsonWriter writer, CertificateVerificationIssuer value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
