// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EnrollmentCertifiedEventData</c> object.</summary>
public sealed record EnrollmentCertifiedEventData : TeachMeHipaaEntity
{
    /// <summary>The <c>subjectId</c> field.</summary>
    [JsonPropertyName("subjectId")]
    public string SubjectId { get; init; } = string.Empty;

    /// <summary>The <c>courseId</c> field.</summary>
    [JsonPropertyName("courseId")]
    public string CourseId { get; init; } = string.Empty;

    /// <summary>The <c>enrollmentId</c> field.</summary>
    [JsonPropertyName("enrollmentId")]
    public string? EnrollmentId { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
