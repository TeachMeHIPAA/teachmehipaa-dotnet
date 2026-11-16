// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary><c>valid</c>, <c>expired</c> or <c>revoked</c>.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(CertificateVerificationStatusJsonConverter))]
public readonly record struct CertificateVerificationStatus
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public CertificateVerificationStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>valid</c>.</summary>
    public static CertificateVerificationStatus Valid { get; } = new("valid");

    /// <summary><c>expired</c>.</summary>
    public static CertificateVerificationStatus Expired { get; } = new("expired");

    /// <summary><c>revoked</c>.</summary>
    public static CertificateVerificationStatus Revoked { get; } = new("revoked");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class CertificateVerificationStatusJsonConverter : JsonConverter<CertificateVerificationStatus>
{
    public override CertificateVerificationStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A CertificateVerificationStatus must be a string."));

    public override void Write(Utf8JsonWriter writer, CertificateVerificationStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
