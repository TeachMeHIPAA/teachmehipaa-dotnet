// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Learner (200, already existed) or LearnerCreated (201, created), told apart by <c>created</c>.</summary>
public sealed record LearnerWriteResult : TeachMeHipaaEntity
{
    /// <summary>Unique identifier for the learner. The same as the member's <c>subjectId</c>.</summary>
    [JsonPropertyName("learnerId")]
    public string LearnerId { get; init; } = string.Empty;

    /// <summary>The learner's email address.</summary>
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    /// <summary>The learner's full name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Your own id for the learner, unique within the organization.</summary>
    [JsonPropertyName("externalId")]
    public string? ExternalId { get; init; }

    /// <summary>The learner's training status in this course. One of <c>not_started</c> (invited), <c>invite_bounced</c>, <c>in_progress</c>, <c>compliant</c>, <c>expiring_soon</c> (within 60 days of expiry), <c>expired</c> or <c>removed</c>.</summary>
    [JsonPropertyName("status")]
    public LearnerStatus Status { get; init; }

    /// <summary>The course this learner record is for.</summary>
    [JsonPropertyName("course")]
    public LearnerCourse Course { get; init; } = new();

    /// <summary>When the learner was invited (ISO 8601).</summary>
    [JsonPropertyName("invitedAt")]
    public DateTimeOffset? InvitedAt { get; init; }

    /// <summary>When the learner started training.</summary>
    [JsonPropertyName("startedAt")]
    public DateTimeOffset? StartedAt { get; init; }

    /// <summary>When the learner completed the course.</summary>
    [JsonPropertyName("completedAt")]
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>When the learner's training is due.</summary>
    [JsonPropertyName("dueAt")]
    public DateTimeOffset? DueAt { get; init; }

    /// <summary>The learner's current certificate, with its verification and PDF links.</summary>
    [JsonPropertyName("certificate")]
    public LearnerCertificate? Certificate { get; init; }

    /// <summary>Not on the wire: the SDK sets it from the status, true for 201 (created) and false for 200 (already existed).</summary>
    [JsonPropertyName("created")]
    public bool Created { get; init; }

    /// <summary>How the invite was delivered: <c>email</c>, or <c>none</c> when you deliver it.</summary>
    [JsonPropertyName("delivery")]
    public LearnerCreatedDelivery? Delivery { get; init; }

    /// <summary>The invite link, returned once when <c>delivery</c> is <c>none</c>. Send it to the learner yourself.</summary>
    [JsonPropertyName("joinUrl")]
    public string? JoinUrl { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
