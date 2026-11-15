using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

// ExtensionData has a setter rather than init: the source generator binds
// init-only properties like constructor parameters, which extension data
// can't be.

/// <summary>A Learner's status in one Course (<c>dev#/components/schemas/LearnerDto</c>).</summary>
public sealed record Learner : TeachMeHipaaEntity
{
    /// <summary>Unique identifier for the Learner; the same as the Member's <c>subjectId</c>.</summary>
    [JsonPropertyName("learnerId")]
    public string LearnerId { get; init; } = string.Empty;

    /// <summary>The Learner's email address.</summary>
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    /// <summary>The Learner's full name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Your own id for the Learner, unique within the Organization.</summary>
    [JsonPropertyName("externalId")]
    public string? ExternalId { get; init; }

    /// <summary>The Learner's training status in this Course.</summary>
    [JsonPropertyName("status")]
    public LearnerStatus Status { get; init; }

    /// <summary>The Course this Learner record is for.</summary>
    [JsonPropertyName("course")]
    public LearnerCourse Course { get; init; } = new();

    /// <summary>When the Learner was invited.</summary>
    [JsonPropertyName("invitedAt")]
    public DateTimeOffset? InvitedAt { get; init; }

    /// <summary>When the Learner started training.</summary>
    [JsonPropertyName("startedAt")]
    public DateTimeOffset? StartedAt { get; init; }

    /// <summary>When the Learner completed the Course.</summary>
    [JsonPropertyName("completedAt")]
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>When the Learner's training is due.</summary>
    [JsonPropertyName("dueAt")]
    public DateTimeOffset? DueAt { get; init; }

    /// <summary>The Learner's current Certificate, with its verification and PDF links.</summary>
    [JsonPropertyName("certificate")]
    public LearnerCertificate? Certificate { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}

/// <summary>The Course a <see cref="Learner"/> record is for.</summary>
public sealed record LearnerCourse
{
    /// <summary>The Course's id.</summary>
    [JsonPropertyName("courseId")]
    public string CourseId { get; init; } = string.Empty;

    /// <summary>The Course's title.</summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}

/// <summary>A <see cref="Learner"/>'s current Certificate.</summary>
public sealed record LearnerCertificate
{
    /// <summary>The Certificate's id.</summary>
    [JsonPropertyName("certificateId")]
    public string CertificateId { get; init; } = string.Empty;

    /// <summary>When the Certificate was issued.</summary>
    [JsonPropertyName("issuedAt")]
    public DateTimeOffset IssuedAt { get; init; }

    /// <summary>When the Certificate expires.</summary>
    [JsonPropertyName("expiresAt")]
    public DateTimeOffset ExpiresAt { get; init; }

    /// <summary>The public Certificate verification page.</summary>
    [JsonPropertyName("verifyUrl")]
    public string? VerifyUrl { get; init; }

    /// <summary>Where to download the Certificate PDF.</summary>
    [JsonPropertyName("pdfUrl")]
    public string PdfUrl { get; init; } = string.Empty;

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
