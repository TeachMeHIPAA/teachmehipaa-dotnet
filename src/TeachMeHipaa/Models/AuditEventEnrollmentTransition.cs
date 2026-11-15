// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AuditEventEnrollmentTransition</c> object.</summary>
/// <remarks>One kind of <see cref="AuditEvent"/>.</remarks>
public sealed record AuditEventEnrollmentTransition : AuditEvent
{
    /// <summary>The <c>eventId</c> field.</summary>
    [JsonPropertyName("eventId")]
    public string EventId { get; init; } = string.Empty;

    /// <summary>The <c>at</c> field.</summary>
    [JsonPropertyName("at")]
    public string At { get; init; } = string.Empty;

    /// <summary>Always <c>enrollment.transition</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "enrollment.transition";

    /// <summary>The <c>transition</c> field.</summary>
    [JsonPropertyName("transition")]
    public AuditEventEnrollmentTransitionTransition Transition { get; init; }

    /// <summary>The <c>subjectId</c> field.</summary>
    [JsonPropertyName("subjectId")]
    public string SubjectId { get; init; } = string.Empty;

    /// <summary>The <c>courseId</c> field.</summary>
    [JsonPropertyName("courseId")]
    public string CourseId { get; init; } = string.Empty;

    /// <summary>The <c>statusFrom</c> field.</summary>
    [JsonPropertyName("statusFrom")]
    public AuditEventEnrollmentTransitionStatusFrom StatusFrom { get; init; }

    /// <summary>The <c>statusTo</c> field.</summary>
    [JsonPropertyName("statusTo")]
    public AuditEventEnrollmentTransitionStatusTo StatusTo { get; init; }

    /// <summary>The <c>archivedFrom</c> field.</summary>
    [JsonPropertyName("archivedFrom")]
    public bool ArchivedFrom { get; init; }

    /// <summary>The <c>archivedTo</c> field.</summary>
    [JsonPropertyName("archivedTo")]
    public bool ArchivedTo { get; init; }

    /// <summary>The <c>rev</c> field.</summary>
    [JsonPropertyName("rev")]
    public long Rev { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
