// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The status to display. Adds <c>EXPIRING</c>, <c>EXPIRED</c> and <c>ARCHIVED</c>, derived when you read it.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(EnrollmentEffectiveStatusJsonConverter))]
public readonly record struct EnrollmentEffectiveStatus
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public EnrollmentEffectiveStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>INVITED</c>.</summary>
    public static EnrollmentEffectiveStatus Invited { get; } = new("INVITED");

    /// <summary><c>EMAIL_BOUNCED</c>.</summary>
    public static EnrollmentEffectiveStatus EmailBounced { get; } = new("EMAIL_BOUNCED");

    /// <summary><c>STARTED</c>.</summary>
    public static EnrollmentEffectiveStatus Started { get; } = new("STARTED");

    /// <summary><c>ASSESSMENT_IN_PROGRESS</c>.</summary>
    public static EnrollmentEffectiveStatus AssessmentInProgress { get; } = new("ASSESSMENT_IN_PROGRESS");

    /// <summary><c>CERTIFIED</c>.</summary>
    public static EnrollmentEffectiveStatus Certified { get; } = new("CERTIFIED");

    /// <summary><c>CANCELLED</c>.</summary>
    public static EnrollmentEffectiveStatus Cancelled { get; } = new("CANCELLED");

    /// <summary><c>EXPIRING</c>.</summary>
    public static EnrollmentEffectiveStatus Expiring { get; } = new("EXPIRING");

    /// <summary><c>EXPIRED</c>.</summary>
    public static EnrollmentEffectiveStatus Expired { get; } = new("EXPIRED");

    /// <summary><c>ARCHIVED</c>.</summary>
    public static EnrollmentEffectiveStatus Archived { get; } = new("ARCHIVED");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class EnrollmentEffectiveStatusJsonConverter : JsonConverter<EnrollmentEffectiveStatus>
{
    public override EnrollmentEffectiveStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A EnrollmentEffectiveStatus must be a string."));

    public override void Write(Utf8JsonWriter writer, EnrollmentEffectiveStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
