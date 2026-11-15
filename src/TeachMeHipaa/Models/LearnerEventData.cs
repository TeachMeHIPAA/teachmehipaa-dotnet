// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>LearnerEventData</c> object.</summary>
public sealed record LearnerEventData : TeachMeHipaaEntity
{
    /// <summary>The <c>learnerId</c> field.</summary>
    [JsonPropertyName("learnerId")]
    public string LearnerId { get; init; } = string.Empty;

    /// <summary>The <c>email</c> field.</summary>
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    /// <summary>The <c>externalId</c> field.</summary>
    [JsonPropertyName("externalId")]
    public string? ExternalId { get; init; }

    /// <summary>The <c>courseId</c> field.</summary>
    [JsonPropertyName("courseId")]
    public string CourseId { get; init; } = string.Empty;

    /// <summary>The <c>status</c> field.</summary>
    [JsonPropertyName("status")]
    public LearnerStatus Status { get; init; }

    /// <summary>The <c>occurredAt</c> field.</summary>
    [JsonPropertyName("occurredAt")]
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
