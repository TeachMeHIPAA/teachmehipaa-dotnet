// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="WebhookSubscriptionService.CreateAsync"/>.</summary>
public sealed class WebhookSubscriptionCreateOptions
{
    /// <summary>The HTTPS URL to deliver events to. Required.</summary>
    [JsonPropertyName("endpointUrl")]
    public string EndpointUrl { get; set; } = string.Empty;

    /// <summary>The event types to deliver. Required.</summary>
    [JsonPropertyName("eventTypes")]
    public IReadOnlyList<EventType> EventTypes { get; set; } = [];

    /// <summary>A description for your own reference.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Send <c>seats.low</c> when available seats drop to this number. Defaults to 5.</summary>
    [JsonPropertyName("seatsLowThreshold")]
    public long? SeatsLowThreshold { get; set; }
}
