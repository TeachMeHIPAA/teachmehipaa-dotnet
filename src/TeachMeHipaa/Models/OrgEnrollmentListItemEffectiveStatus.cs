// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>OrgEnrollmentListItemEffectiveStatus</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(OrgEnrollmentListItemEffectiveStatusJsonConverter))]
public readonly record struct OrgEnrollmentListItemEffectiveStatus
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public OrgEnrollmentListItemEffectiveStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>INVITED</c>.</summary>
    public static OrgEnrollmentListItemEffectiveStatus Invited { get; } = new("INVITED");

    /// <summary><c>EMAIL_BOUNCED</c>.</summary>
    public static OrgEnrollmentListItemEffectiveStatus EmailBounced { get; } = new("EMAIL_BOUNCED");

    /// <summary><c>STARTED</c>.</summary>
    public static OrgEnrollmentListItemEffectiveStatus Started { get; } = new("STARTED");

    /// <summary><c>ASSESSMENT_IN_PROGRESS</c>.</summary>
    public static OrgEnrollmentListItemEffectiveStatus AssessmentInProgress { get; } = new("ASSESSMENT_IN_PROGRESS");

    /// <summary><c>CERTIFIED</c>.</summary>
    public static OrgEnrollmentListItemEffectiveStatus Certified { get; } = new("CERTIFIED");

    /// <summary><c>CANCELLED</c>.</summary>
    public static OrgEnrollmentListItemEffectiveStatus Cancelled { get; } = new("CANCELLED");

    /// <summary><c>EXPIRING</c>.</summary>
    public static OrgEnrollmentListItemEffectiveStatus Expiring { get; } = new("EXPIRING");

    /// <summary><c>EXPIRED</c>.</summary>
    public static OrgEnrollmentListItemEffectiveStatus Expired { get; } = new("EXPIRED");

    /// <summary><c>ARCHIVED</c>.</summary>
    public static OrgEnrollmentListItemEffectiveStatus Archived { get; } = new("ARCHIVED");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class OrgEnrollmentListItemEffectiveStatusJsonConverter : JsonConverter<OrgEnrollmentListItemEffectiveStatus>
{
    public override OrgEnrollmentListItemEffectiveStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A OrgEnrollmentListItemEffectiveStatus must be a string."));

    public override void Write(Utf8JsonWriter writer, OrgEnrollmentListItemEffectiveStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
