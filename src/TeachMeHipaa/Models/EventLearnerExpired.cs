// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EventLearnerExpired</c> object.</summary>
/// <remarks>One kind of <see cref="Event"/>.</remarks>
public sealed record EventLearnerExpired : Event
{
    /// <summary>Always <c>learner.expired</c>.</summary>
    [JsonPropertyName("eventType")]
    public string EventType { get; } = "learner.expired";

    /// <summary>The <c>data</c> field.</summary>
    [JsonPropertyName("data")]
    public LearnerEventData Data { get; init; } = new();

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
