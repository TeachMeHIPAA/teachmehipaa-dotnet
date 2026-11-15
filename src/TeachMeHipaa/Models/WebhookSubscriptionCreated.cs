// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>WebhookSubscriptionCreated</c> object.</summary>
public sealed record WebhookSubscriptionCreated : TeachMeHipaaEntity
{
    /// <summary>Unique identifier for the subscription.</summary>
    [JsonPropertyName("subscriptionId")]
    public string SubscriptionId { get; init; } = string.Empty;

    /// <summary>The HTTPS URL events are delivered to.</summary>
    [JsonPropertyName("endpointUrl")]
    public string EndpointUrl { get; init; } = string.Empty;

    /// <summary>The event types delivered.</summary>
    [JsonPropertyName("eventTypes")]
    public IReadOnlyList<EventType> EventTypes { get; init; } = [];

    /// <summary>Whether deliveries are on.</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }

    /// <summary>The <c>description</c> field.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>The <c>seatsLowThreshold</c> field.</summary>
    [JsonPropertyName("seatsLowThreshold")]
    public long SeatsLowThreshold { get; init; }

    /// <summary>Identifies the signing secret.</summary>
    [JsonPropertyName("secretKeyId")]
    public string SecretKeyId { get; init; } = string.Empty;

    /// <summary>The <c>createdAt</c> field.</summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>The <c>rev</c> field.</summary>
    [JsonPropertyName("rev")]
    public long Rev { get; init; }

    /// <summary>The signing secret. Returned only once: store it now.</summary>
    [JsonPropertyName("secret")]
    public string Secret { get; init; } = string.Empty;

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
