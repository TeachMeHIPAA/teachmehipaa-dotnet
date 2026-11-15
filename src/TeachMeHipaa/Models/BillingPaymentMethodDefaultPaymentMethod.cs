// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>BillingPaymentMethodDefaultPaymentMethod</c> object.</summary>
public sealed record BillingPaymentMethodDefaultPaymentMethod : TeachMeHipaaEntity
{
    /// <summary>The <c>brand</c> field.</summary>
    [JsonPropertyName("brand")]
    public string Brand { get; init; } = string.Empty;

    /// <summary>The <c>last4</c> field.</summary>
    [JsonPropertyName("last4")]
    public string Last4 { get; init; } = string.Empty;

    /// <summary>The <c>expMonth</c> field.</summary>
    [JsonPropertyName("expMonth")]
    public long ExpMonth { get; init; }

    /// <summary>The <c>expYear</c> field.</summary>
    [JsonPropertyName("expYear")]
    public long ExpYear { get; init; }

    /// <summary>The <c>paymentMethodId</c> field.</summary>
    [JsonPropertyName("paymentMethodId")]
    public string? PaymentMethodId { get; init; }

    /// <summary>The <c>savedBy</c> field.</summary>
    [JsonPropertyName("savedBy")]
    public PaymentMethodSavedBy? SavedBy { get; init; }

    /// <summary>The <c>savedAt</c> field.</summary>
    [JsonPropertyName("savedAt")]
    public DateTimeOffset? SavedAt { get; init; }

    /// <summary>The <c>expired</c> field.</summary>
    [JsonPropertyName("expired")]
    public bool Expired { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
