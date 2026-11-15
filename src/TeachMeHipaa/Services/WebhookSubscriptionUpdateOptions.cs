// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="WebhookSubscriptionService.UpdateAsync"/>.</summary>
/// <remarks>Sent as a JSON merge patch: a property left unset is left out, so the field keeps its value, and one set to null clears the field.</remarks>
public sealed class WebhookSubscriptionUpdateOptions
{
    /// <summary>Creates the parameters of a conditional update.</summary>
    /// <param name="ifMatch">The ETag you read the object with, or the object itself. Sent as <c>If-Match</c>.</param>
    public WebhookSubscriptionUpdateOptions(ETag ifMatch) => IfMatch = ifMatch;

    /// <summary>Sent as <c>If-Match</c>: the ETag you read the object with, or the object itself. If the object changed since, the call fails with <see cref="PreconditionFailedException"/>. Required.</summary>
    [JsonIgnore]
    public ETag IfMatch { get; set; }

    /// <summary>The HTTPS URL to deliver events to.</summary>
    [JsonPropertyName("endpointUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonConverter(typeof(OptionalJsonConverter<string>))]
    public Optional<string> EndpointUrl { get; set; }

    /// <summary>The event types to deliver.</summary>
    [JsonPropertyName("eventTypes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonConverter(typeof(OptionalJsonConverter<IReadOnlyList<EventType>>))]
    public Optional<IReadOnlyList<EventType>> EventTypes { get; set; }

    /// <summary>Turn deliveries on or off.</summary>
    [JsonPropertyName("enabled")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonConverter(typeof(OptionalJsonConverter<bool>))]
    public Optional<bool> Enabled { get; set; }

    /// <summary>A description for your own reference.</summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonConverter(typeof(OptionalJsonConverter<string>))]
    public Optional<string?> Description { get; set; }

    /// <summary>Send <c>seats.low</c> when available seats drop to this number.</summary>
    [JsonPropertyName("seatsLowThreshold")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonConverter(typeof(OptionalJsonConverter<long>))]
    public Optional<long> SeatsLowThreshold { get; set; }
}
