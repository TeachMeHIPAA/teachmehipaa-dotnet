// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EnrollmentCancelRefunded</c> object.</summary>
/// <remarks>One kind of <see cref="CancelEnrollmentResponse"/>.</remarks>
public sealed record EnrollmentCancelRefunded : CancelEnrollmentResponse
{
    /// <summary>The <c>enrollmentId</c> field.</summary>
    [JsonPropertyName("enrollmentId")]
    public string EnrollmentId { get; init; } = string.Empty;

    /// <summary>The <c>subjectId</c> field.</summary>
    [JsonPropertyName("subjectId")]
    public string SubjectId { get; init; } = string.Empty;

    /// <summary>The <c>courseId</c> field.</summary>
    [JsonPropertyName("courseId")]
    public string CourseId { get; init; } = string.Empty;

    /// <summary>The <c>orgId</c> field.</summary>
    [JsonPropertyName("orgId")]
    public string OrgId { get; init; } = string.Empty;

    /// <summary>The <c>status</c> field.</summary>
    [JsonPropertyName("status")]
    public EnrollmentCancelRefundedStatus Status { get; init; }

    /// <summary>The <c>effectiveStatus</c> field.</summary>
    [JsonPropertyName("effectiveStatus")]
    public EnrollmentCancelRefundedEffectiveStatus EffectiveStatus { get; init; }

    /// <summary>The <c>archived</c> field.</summary>
    [JsonPropertyName("archived")]
    public bool Archived { get; init; }

    /// <summary>The <c>cycle</c> field.</summary>
    [JsonPropertyName("cycle")]
    public long Cycle { get; init; }

    /// <summary>The <c>rev</c> field.</summary>
    [JsonPropertyName("rev")]
    public long Rev { get; init; }

    /// <summary>The <c>invitedAt</c> field.</summary>
    [JsonPropertyName("invitedAt")]
    public string? InvitedAt { get; init; }

    /// <summary>The <c>startedAt</c> field.</summary>
    [JsonPropertyName("startedAt")]
    public string? StartedAt { get; init; }

    /// <summary>The <c>completedAt</c> field.</summary>
    [JsonPropertyName("completedAt")]
    public string? CompletedAt { get; init; }

    /// <summary>The <c>certifiedAt</c> field.</summary>
    [JsonPropertyName("certifiedAt")]
    public string? CertifiedAt { get; init; }

    /// <summary>The <c>expiresAt</c> field.</summary>
    [JsonPropertyName("expiresAt")]
    public string? ExpiresAt { get; init; }

    /// <summary>The <c>certificateId</c> field.</summary>
    [JsonPropertyName("certificateId")]
    public string? CertificateId { get; init; }

    /// <summary>The <c>refund</c> field.</summary>
    [JsonPropertyName("refund")]
    public EnrollmentCancelRefundedRefund Refund { get; init; } = new();

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
