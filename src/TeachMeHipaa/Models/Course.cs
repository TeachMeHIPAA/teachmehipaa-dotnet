// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>Course</c> object.</summary>
public sealed record Course : TeachMeHipaaEntity
{
    /// <summary>Unique identifier for the course.</summary>
    [JsonPropertyName("courseId")]
    public string CourseId { get; init; } = string.Empty;

    /// <summary>The course's title.</summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>The current version of the course content.</summary>
    [JsonPropertyName("courseVersion")]
    public string CourseVersion { get; init; } = string.Empty;

    /// <summary>The score needed to pass the course's assessment.</summary>
    [JsonPropertyName("passingThreshold")]
    public double PassingThreshold { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
