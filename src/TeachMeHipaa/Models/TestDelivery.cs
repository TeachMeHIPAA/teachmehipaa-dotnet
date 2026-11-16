// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The body of a :test delivery. It is not an event envelope; its x-tmh-event-id is a UUID.</summary>
public sealed record TestDelivery : TeachMeHipaaEntity
{
    /// <summary>The subscription that was tested.</summary>
    [JsonPropertyName("subscriptionId")]
    public string SubscriptionId { get; init; } = string.Empty;

    /// <summary>Always true.</summary>
    [JsonPropertyName("test")]
    public bool Test { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
