// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EventLearnerInvited</c> object.</summary>
/// <remarks>One kind of <see cref="Event"/>.</remarks>
public sealed record EventLearnerInvited : Event
{
    /// <summary>The <c>eventId</c> field.</summary>
    [JsonPropertyName("eventId")]
    public string EventId { get; init; } = string.Empty;

    /// <summary>Always <c>learner.invited</c>.</summary>
    [JsonPropertyName("eventType")]
    public string EventType { get; } = "learner.invited";

    /// <summary>The <c>orgId</c> field.</summary>
    [JsonPropertyName("orgId")]
    public string OrgId { get; init; } = string.Empty;

    /// <summary>The <c>occurredAt</c> field.</summary>
    [JsonPropertyName("occurredAt")]
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>The <c>data</c> field.</summary>
    [JsonPropertyName("data")]
    public LearnerEventData Data { get; init; } = new();

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
