// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>Enrollment</c> object.</summary>
/// <remarks>One kind of <see cref="CancelEnrollmentResponse"/>.</remarks>
public sealed record Enrollment : CancelEnrollmentResponse
{
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

    /// <summary>The stored lifecycle state: <c>INVITED</c>, <c>EMAIL_BOUNCED</c>, <c>STARTED</c>, <c>ASSESSMENT_IN_PROGRESS</c>, <c>CERTIFIED</c> or <c>CANCELLED</c>.</summary>
    [JsonPropertyName("status")]
    public EnrollmentStatus Status { get; init; }

    /// <summary>The status to display. Adds <c>EXPIRING</c>, <c>EXPIRED</c> and <c>ARCHIVED</c>, derived when you read it.</summary>
    [JsonPropertyName("effectiveStatus")]
    public EnrollmentEffectiveStatus EffectiveStatus { get; init; }

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

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
