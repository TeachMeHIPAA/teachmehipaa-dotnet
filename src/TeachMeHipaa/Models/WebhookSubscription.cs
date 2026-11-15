// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>WebhookSubscription</c> object.</summary>
public sealed record WebhookSubscription : TeachMeHipaaEntity
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

    /// <summary>Your description of the subscription.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Send <c>seats.low</c> when available seats drop to this number. Defaults to 5.</summary>
    [JsonPropertyName("seatsLowThreshold")]
    public long SeatsLowThreshold { get; init; }

    /// <summary>Identifies the signing secret. Sent as <c>x-tmh-key-id</c>.</summary>
    [JsonPropertyName("secretKeyId")]
    public string SecretKeyId { get; init; } = string.Empty;

    /// <summary>When it was created (ISO 8601).</summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>The resource's revision. Its strong <c>ETag</c> is derived from it.</summary>
    [JsonPropertyName("rev")]
    public long Rev { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
