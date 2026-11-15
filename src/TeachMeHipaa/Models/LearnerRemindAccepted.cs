// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>LearnerRemindAccepted</c> object.</summary>
public sealed record LearnerRemindAccepted : TeachMeHipaaEntity
{
    /// <summary>The learner the reminder is for.</summary>
    [JsonPropertyName("learnerId")]
    public string LearnerId { get; init; } = string.Empty;

    /// <summary>Always <c>remind</c>.</summary>
    [JsonPropertyName("action")]
    public LearnerRemindAcceptedAction Action { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
