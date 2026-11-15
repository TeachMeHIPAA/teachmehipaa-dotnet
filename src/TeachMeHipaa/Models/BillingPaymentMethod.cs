// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>BillingPaymentMethod</c> object.</summary>
public sealed record BillingPaymentMethod : TeachMeHipaaEntity
{
    /// <summary>The saved card's brand, last four digits, expiry and id, or <c>null</c> when none is saved.</summary>
    [JsonPropertyName("defaultPaymentMethod")]
    public BillingPaymentMethodDefaultPaymentMethod? DefaultPaymentMethod { get; init; }

    /// <summary>Whether a <c>saved_payment_method</c> order would charge it now.</summary>
    [JsonPropertyName("chargeable")]
    public bool Chargeable { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
