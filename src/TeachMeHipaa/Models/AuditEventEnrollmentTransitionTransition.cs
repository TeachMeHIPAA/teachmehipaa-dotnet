// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AuditEventEnrollmentTransitionTransition</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(AuditEventEnrollmentTransitionTransitionJsonConverter))]
public readonly record struct AuditEventEnrollmentTransitionTransition
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public AuditEventEnrollmentTransitionTransition(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>bounce</c>.</summary>
    public static AuditEventEnrollmentTransitionTransition Bounce { get; } = new("bounce");

    /// <summary><c>start</c>.</summary>
    public static AuditEventEnrollmentTransitionTransition Start { get; } = new("start");

    /// <summary><c>beginAssessment</c>.</summary>
    public static AuditEventEnrollmentTransitionTransition BeginAssessment { get; } = new("beginAssessment");

    /// <summary><c>submitAssessment</c>.</summary>
    public static AuditEventEnrollmentTransitionTransition SubmitAssessment { get; } = new("submitAssessment");

    /// <summary><c>certify</c>.</summary>
    public static AuditEventEnrollmentTransitionTransition Certify { get; } = new("certify");

    /// <summary><c>revokeCert</c>.</summary>
    public static AuditEventEnrollmentTransitionTransition RevokeCert { get; } = new("revokeCert");

    /// <summary><c>archive</c>.</summary>
    public static AuditEventEnrollmentTransitionTransition Archive { get; } = new("archive");

    /// <summary><c>unarchive</c>.</summary>
    public static AuditEventEnrollmentTransitionTransition Unarchive { get; } = new("unarchive");

    /// <summary><c>reinvite</c>.</summary>
    public static AuditEventEnrollmentTransitionTransition Reinvite { get; } = new("reinvite");

    /// <summary><c>cancelInvite</c>.</summary>
    public static AuditEventEnrollmentTransitionTransition CancelInvite { get; } = new("cancelInvite");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class AuditEventEnrollmentTransitionTransitionJsonConverter : JsonConverter<AuditEventEnrollmentTransitionTransition>
{
    public override AuditEventEnrollmentTransitionTransition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A AuditEventEnrollmentTransitionTransition must be a string."));

    public override void Write(Utf8JsonWriter writer, AuditEventEnrollmentTransitionTransition value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
