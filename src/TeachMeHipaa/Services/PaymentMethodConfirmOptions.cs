// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="PaymentMethodService.ConfirmAsync"/>.</summary>
public sealed class PaymentMethodConfirmOptions
{
    /// <summary>The SetupIntent you confirmed with Stripe.js. Required.</summary>
    [JsonPropertyName("setupIntentId")]
    public string SetupIntentId { get; set; } = string.Empty;
}
