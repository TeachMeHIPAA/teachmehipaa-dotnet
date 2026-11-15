// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="SeatOrderService.CreateAsync"/>.</summary>
public sealed class SeatOrderCreateOptions
{
    /// <summary>Seats to buy. Required.</summary>
    [JsonPropertyName("quantity")]
    public long Quantity { get; set; }

    /// <summary>How to pay: <c>hosted_checkout</c> with a <c>returnUrl</c>, <c>saved_payment_method</c> with the card's <c>paymentMethodId</c>, or <c>invoice</c>. A <c>returnUrl</c> must be https and on your API client's allowlist. Required.</summary>
    [JsonPropertyName("payment")]
    public SeatOrderCreateRequestPayment Payment { get; set; } = null!;

    /// <summary>Your own reference for the order.</summary>
    [JsonPropertyName("clientReference")]
    public string? ClientReference { get; set; }
}
