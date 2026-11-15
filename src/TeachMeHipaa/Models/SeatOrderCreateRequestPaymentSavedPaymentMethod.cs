// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>SeatOrderCreateRequestPaymentSavedPaymentMethod</c> object.</summary>
/// <remarks>One kind of <see cref="SeatOrderCreateRequestPayment"/>.</remarks>
public sealed record SeatOrderCreateRequestPaymentSavedPaymentMethod : SeatOrderCreateRequestPayment
{
    /// <summary>Always <c>saved_payment_method</c>.</summary>
    [JsonPropertyName("method")]
    public string Method { get; } = "saved_payment_method";

    /// <summary>The <c>returnUrl</c> field.</summary>
    [JsonPropertyName("returnUrl")]
    public string? ReturnUrl { get; init; }

    /// <summary>The <c>paymentMethodId</c> field.</summary>
    [JsonPropertyName("paymentMethodId")]
    public string? PaymentMethodId { get; init; }

    /// <summary>The <c>expectedTotal</c> field.</summary>
    [JsonPropertyName("expectedTotal")]
    public long? ExpectedTotal { get; init; }

    /// <summary>The <c>currency</c> field.</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
