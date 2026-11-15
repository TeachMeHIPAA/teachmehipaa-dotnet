// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The course the learner was invited to.</summary>
public sealed record LearnerCreatedCourse : TeachMeHipaaEntity
{
    /// <summary>The <c>courseId</c> field.</summary>
    [JsonPropertyName("courseId")]
    public string CourseId { get; init; } = string.Empty;

    /// <summary>The <c>title</c> field.</summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
