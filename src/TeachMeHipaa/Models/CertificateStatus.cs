// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary><c>VALID</c>, <c>EXPIRED</c> or <c>REVOKED</c>.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(CertificateStatusJsonConverter))]
public readonly record struct CertificateStatus
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public CertificateStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>VALID</c>.</summary>
    public static CertificateStatus Valid { get; } = new("VALID");

    /// <summary><c>EXPIRED</c>.</summary>
    public static CertificateStatus Expired { get; } = new("EXPIRED");

    /// <summary><c>REVOKED</c>.</summary>
    public static CertificateStatus Revoked { get; } = new("REVOKED");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class CertificateStatusJsonConverter : JsonConverter<CertificateStatus>
{
    public override CertificateStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A CertificateStatus must be a string."));

    public override void Write(Utf8JsonWriter writer, CertificateStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
