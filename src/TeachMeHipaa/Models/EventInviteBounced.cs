// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EventInviteBounced</c> object.</summary>
/// <remarks>One kind of <see cref="Event"/>.</remarks>
public sealed record EventInviteBounced : Event
{
    /// <summary>Always <c>invite.bounced</c>.</summary>
    [JsonPropertyName("eventType")]
    public string EventType { get; } = "invite.bounced";

    /// <summary>The <c>data</c> field.</summary>
    [JsonPropertyName("data")]
    public LearnerEventData Data { get; init; } = new();

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
