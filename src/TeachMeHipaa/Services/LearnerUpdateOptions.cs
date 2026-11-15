// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="LearnerService.UpdateAsync"/>.</summary>
/// <remarks>Sent as a JSON merge patch. A property left null is left out, so the field stays unchanged; clearing a field is not supported yet.</remarks>
public sealed class LearnerUpdateOptions
{
    /// <summary>The learner's full name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Your own id for the learner. Set it to <c>null</c> to clear it.</summary>
    [JsonPropertyName("externalId")]
    public string? ExternalId { get; set; }

    /// <summary>The course. Defaults to the organization's default course.</summary>
    [JsonIgnore]
    public string? CourseId { get; set; }
}
