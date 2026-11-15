// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>One of the API's kinds of <c>CancelEnrollmentResponse</c>.</summary>
/// <remarks>
/// Known kinds: <see cref="EnrollmentCancelRefunded"/>, <see cref="Enrollment"/>.
/// Chosen by which of their fields is present, else <see cref="Enrollment"/>.
/// </remarks>
[JsonConverter(typeof(CancelEnrollmentResponseJsonConverter))]
public abstract record CancelEnrollmentResponse : TeachMeHipaaEntity
{
    private protected CancelEnrollmentResponse()
    {
    }

    /// <summary>Unique identifier for the enrollment.</summary>
    [JsonPropertyName("enrollmentId")]
    public string EnrollmentId { get; init; } = string.Empty;

    /// <summary>The learner's member id.</summary>
    [JsonPropertyName("subjectId")]
    public string SubjectId { get; init; } = string.Empty;

    /// <summary>The course.</summary>
    [JsonPropertyName("courseId")]
    public string CourseId { get; init; } = string.Empty;

    /// <summary>The organization's id.</summary>
    [JsonPropertyName("orgId")]
    public string OrgId { get; init; } = string.Empty;

    /// <summary>Whether the enrollment is archived.</summary>
    [JsonPropertyName("archived")]
    public bool Archived { get; init; }

    /// <summary>The certification cycle, starting at 0. Renewing adds one.</summary>
    [JsonPropertyName("cycle")]
    public long Cycle { get; init; }

    /// <summary>The resource's revision. Its strong <c>ETag</c> is derived from it.</summary>
    [JsonPropertyName("rev")]
    public long Rev { get; init; }

    /// <summary>When the learner was invited (ISO 8601).</summary>
    [JsonPropertyName("invitedAt")]
    public string? InvitedAt { get; init; }

    /// <summary>When the learner started.</summary>
    [JsonPropertyName("startedAt")]
    public string? StartedAt { get; init; }

    /// <summary>When the learner completed the course.</summary>
    [JsonPropertyName("completedAt")]
    public string? CompletedAt { get; init; }

    /// <summary>When the learner was certified.</summary>
    [JsonPropertyName("certifiedAt")]
    public string? CertifiedAt { get; init; }

    /// <summary>When the certification expires.</summary>
    [JsonPropertyName("expiresAt")]
    public string? ExpiresAt { get; init; }

    /// <summary>The certificate issued for this cycle, if any.</summary>
    [JsonPropertyName("certificateId")]
    public string? CertificateId { get; init; }
}

internal sealed class CancelEnrollmentResponseJsonConverter : JsonConverter<CancelEnrollmentResponse>
{
    public override CancelEnrollmentResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var json = document.RootElement;
        if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty("refund", out _))
        {
            return json.Deserialize(TeachMeHipaaJsonContext.Default.EnrollmentCancelRefunded);
        }

        return json.Deserialize(TeachMeHipaaJsonContext.Default.Enrollment);
    }

    public override void Write(Utf8JsonWriter writer, CancelEnrollmentResponse value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case EnrollmentCancelRefunded member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EnrollmentCancelRefunded);
                break;
            case Enrollment member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.Enrollment);
                break;
        }
    }
}
