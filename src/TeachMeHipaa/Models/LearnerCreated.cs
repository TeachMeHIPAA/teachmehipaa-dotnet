// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>LearnerCreated</c> object.</summary>
public sealed record LearnerCreated : TeachMeHipaaEntity
{
    /// <summary>Unique identifier for the learner.</summary>
    [JsonPropertyName("learnerId")]
    public string LearnerId { get; init; } = string.Empty;

    /// <summary>The learner's email address.</summary>
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    /// <summary>The learner's full name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Your own id for the learner.</summary>
    [JsonPropertyName("externalId")]
    public string? ExternalId { get; init; }

    /// <summary>The learner's training status. One of <c>not_started</c> (invited), <c>invite_bounced</c>, <c>in_progress</c>, <c>compliant</c>, <c>expiring_soon</c> (within 60 days of expiry), <c>expired</c> or <c>removed</c>.</summary>
    [JsonPropertyName("status")]
    public LearnerStatus Status { get; init; }

    /// <summary>The course the learner was invited to.</summary>
    [JsonPropertyName("course")]
    public LearnerCreatedCourse Course { get; init; } = new();

    /// <summary>The <c>invitedAt</c> field.</summary>
    [JsonPropertyName("invitedAt")]
    public DateTimeOffset? InvitedAt { get; init; }

    /// <summary>The <c>startedAt</c> field.</summary>
    [JsonPropertyName("startedAt")]
    public DateTimeOffset? StartedAt { get; init; }

    /// <summary>The <c>completedAt</c> field.</summary>
    [JsonPropertyName("completedAt")]
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>The <c>dueAt</c> field.</summary>
    [JsonPropertyName("dueAt")]
    public DateTimeOffset? DueAt { get; init; }

    /// <summary>The <c>certificate</c> field.</summary>
    [JsonPropertyName("certificate")]
    public LearnerCreatedCertificate? Certificate { get; init; }

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
