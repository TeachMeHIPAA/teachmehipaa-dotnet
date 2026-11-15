using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>
/// Parameters for <see cref="LearnerService.UpdateAsync"/>, sent as a merge
/// patch (<c>dev#/components/schemas/LearnerPatch</c>). A field left null is
/// left unchanged.
/// </summary>
public sealed class LearnerUpdateOptions
{
    /// <summary>The Learner's full name.</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>Your own id for the Learner, unique within the Organization.</summary>
    [JsonPropertyName("externalId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExternalId { get; set; }
}
