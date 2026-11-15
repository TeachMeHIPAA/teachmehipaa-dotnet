// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="SeatOrderService.QuoteAsync"/>.</summary>
public sealed class SeatOrderQuoteOptions
{
    /// <summary>Seats to quote. Required.</summary>
    [JsonPropertyName("quantity")]
    public long Quantity { get; set; }

    /// <summary>The currency you expect to be charged in. It must match the organization's.</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}
