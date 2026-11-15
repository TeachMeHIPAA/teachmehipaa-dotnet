// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="WebhookSubscriptionService.UpdateAsync"/>.</summary>
/// <remarks>Sent as a JSON merge patch. A property left null is left out, so the field stays unchanged; clearing a field is not supported yet.</remarks>
public sealed class WebhookSubscriptionUpdateOptions
{
    /// <summary>The HTTPS URL to deliver events to.</summary>
    [JsonPropertyName("endpointUrl")]
    public string? EndpointUrl { get; set; }

    /// <summary>The event types to deliver.</summary>
    [JsonPropertyName("eventTypes")]
    public IReadOnlyList<EventType>? EventTypes { get; set; }

    /// <summary>Turn deliveries on or off.</summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    /// <summary>A description for your own reference.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Send <c>seats.low</c> when available seats drop to this number.</summary>
    [JsonPropertyName("seatsLowThreshold")]
    public long? SeatsLowThreshold { get; set; }
}
