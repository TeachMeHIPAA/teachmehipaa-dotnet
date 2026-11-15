// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>SeatOrderCreated</c> object.</summary>
public sealed record SeatOrderCreated : TeachMeHipaaEntity
{
    /// <summary>Unique identifier for the seat order.</summary>
    [JsonPropertyName("seatOrderId")]
    public string SeatOrderId { get; init; } = string.Empty;

    /// <summary>The order's status.</summary>
    [JsonPropertyName("status")]
    public SeatOrderCreatedStatus Status { get; init; }

    /// <summary>Where to send the buyer next: a checkout <c>url</c> to redirect to, or a hosted invoice.</summary>
    [JsonPropertyName("nextAction")]
    public SeatOrderCreatedNextAction? NextAction { get; init; }

    /// <summary>Why the payment failed, if it did.</summary>
    [JsonPropertyName("failure")]
    public SeatOrderCreatedFailure? Failure { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
