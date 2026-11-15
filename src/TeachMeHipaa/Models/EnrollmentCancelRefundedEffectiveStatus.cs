// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EnrollmentCancelRefundedEffectiveStatus</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(EnrollmentCancelRefundedEffectiveStatusJsonConverter))]
public readonly record struct EnrollmentCancelRefundedEffectiveStatus
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public EnrollmentCancelRefundedEffectiveStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>INVITED</c>.</summary>
    public static EnrollmentCancelRefundedEffectiveStatus Invited { get; } = new("INVITED");

    /// <summary><c>EMAIL_BOUNCED</c>.</summary>
    public static EnrollmentCancelRefundedEffectiveStatus EmailBounced { get; } = new("EMAIL_BOUNCED");

    /// <summary><c>STARTED</c>.</summary>
    public static EnrollmentCancelRefundedEffectiveStatus Started { get; } = new("STARTED");

    /// <summary><c>ASSESSMENT_IN_PROGRESS</c>.</summary>
    public static EnrollmentCancelRefundedEffectiveStatus AssessmentInProgress { get; } = new("ASSESSMENT_IN_PROGRESS");

    /// <summary><c>CERTIFIED</c>.</summary>
    public static EnrollmentCancelRefundedEffectiveStatus Certified { get; } = new("CERTIFIED");

    /// <summary><c>CANCELLED</c>.</summary>
    public static EnrollmentCancelRefundedEffectiveStatus Cancelled { get; } = new("CANCELLED");

    /// <summary><c>EXPIRING</c>.</summary>
    public static EnrollmentCancelRefundedEffectiveStatus Expiring { get; } = new("EXPIRING");

    /// <summary><c>EXPIRED</c>.</summary>
    public static EnrollmentCancelRefundedEffectiveStatus Expired { get; } = new("EXPIRED");

    /// <summary><c>ARCHIVED</c>.</summary>
    public static EnrollmentCancelRefundedEffectiveStatus Archived { get; } = new("ARCHIVED");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class EnrollmentCancelRefundedEffectiveStatusJsonConverter : JsonConverter<EnrollmentCancelRefundedEffectiveStatus>
{
    public override EnrollmentCancelRefundedEffectiveStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A EnrollmentCancelRefundedEffectiveStatus must be a string."));

    public override void Write(Utf8JsonWriter writer, EnrollmentCancelRefundedEffectiveStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
