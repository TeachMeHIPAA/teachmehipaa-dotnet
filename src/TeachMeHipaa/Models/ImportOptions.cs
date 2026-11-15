// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>ImportOptions</c> object.</summary>
public sealed record ImportOptions : TeachMeHipaaEntity
{
    /// <summary>The course to invite everyone to. Defaults to the organization's default course.</summary>
    [JsonPropertyName("courseId")]
    public string? CourseId { get; init; }

    [JsonInclude]
    [JsonPropertyName("validateOnly")]
    internal bool? ValidateOnly { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
