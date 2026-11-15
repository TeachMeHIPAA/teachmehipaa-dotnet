// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>SeatOrderQuote</c> object.</summary>
public sealed record SeatOrderQuote : TeachMeHipaaEntity
{
    /// <summary>Seats quoted.</summary>
    [JsonPropertyName("quantity")]
    public long Quantity { get; init; }

    /// <summary>Price per seat, in the currency's smallest unit.</summary>
    [JsonPropertyName("unitAmount")]
    public long UnitAmount { get; init; }

    /// <summary>Three-letter ISO currency code, lowercase.</summary>
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>Price before tax.</summary>
    [JsonPropertyName("subtotal")]
    public long Subtotal { get; init; }

    /// <summary>Always <c>null</c>: tax is calculated at payment.</summary>
    [JsonPropertyName("tax")]
    public JsonElement? Tax { get; init; }

    /// <summary>The quoted total.</summary>
    [JsonPropertyName("total")]
    public long Total { get; init; }

    /// <summary>When the quote expires, 15 minutes after it was made.</summary>
    [JsonPropertyName("quoteExpiresAt")]
    public DateTimeOffset QuoteExpiresAt { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
