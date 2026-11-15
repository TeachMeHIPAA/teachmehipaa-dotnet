// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>SeatOrderEventData</c> object.</summary>
public sealed record SeatOrderEventData : TeachMeHipaaEntity
{
    /// <summary>The <c>seatOrderId</c> field.</summary>
    [JsonPropertyName("seatOrderId")]
    public string SeatOrderId { get; init; } = string.Empty;

    /// <summary>The <c>quantity</c> field.</summary>
    [JsonPropertyName("quantity")]
    public long Quantity { get; init; }

    /// <summary>The <c>status</c> field.</summary>
    [JsonPropertyName("status")]
    public SeatOrderEventDataStatus Status { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
