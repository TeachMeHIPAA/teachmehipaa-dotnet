// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>OrgEnrollmentListItemPriorCertificateStatus</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(OrgEnrollmentListItemPriorCertificateStatusJsonConverter))]
public readonly record struct OrgEnrollmentListItemPriorCertificateStatus
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public OrgEnrollmentListItemPriorCertificateStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>VALID</c>.</summary>
    public static OrgEnrollmentListItemPriorCertificateStatus Valid { get; } = new("VALID");

    /// <summary><c>EXPIRED</c>.</summary>
    public static OrgEnrollmentListItemPriorCertificateStatus Expired { get; } = new("EXPIRED");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class OrgEnrollmentListItemPriorCertificateStatusJsonConverter : JsonConverter<OrgEnrollmentListItemPriorCertificateStatus>
{
    public override OrgEnrollmentListItemPriorCertificateStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A OrgEnrollmentListItemPriorCertificateStatus must be a string."));

    public override void Write(Utf8JsonWriter writer, OrgEnrollmentListItemPriorCertificateStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
