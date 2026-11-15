// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="PaymentMethodService.SetupAsync"/>.</summary>
public sealed class PaymentMethodSetupOptions
{
    /// <summary>Where Stripe sends the buyer afterwards. Must be https and on your client's allowlist. Required.</summary>
    [JsonPropertyName("returnUrl")]
    public string ReturnUrl { get; set; } = string.Empty;
}
