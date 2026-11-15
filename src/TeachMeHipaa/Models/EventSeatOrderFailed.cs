// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EventSeatOrderFailed</c> object.</summary>
/// <remarks>One kind of <see cref="Event"/>.</remarks>
public sealed record EventSeatOrderFailed : Event
{
    /// <summary>Always <c>seat_order.failed</c>.</summary>
    [JsonPropertyName("eventType")]
    public string EventType { get; } = "seat_order.failed";

    /// <summary>The <c>data</c> field.</summary>
    [JsonPropertyName("data")]
    public SeatOrderEventData Data { get; init; } = new();

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
