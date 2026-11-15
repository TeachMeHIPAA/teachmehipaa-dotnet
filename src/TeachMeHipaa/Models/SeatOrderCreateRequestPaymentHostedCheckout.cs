// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>SeatOrderCreateRequestPaymentHostedCheckout</c> object.</summary>
/// <remarks>One kind of <see cref="SeatOrderCreateRequestPayment"/>.</remarks>
public sealed record SeatOrderCreateRequestPaymentHostedCheckout : SeatOrderCreateRequestPayment
{
    /// <summary>Always <c>hosted_checkout</c>.</summary>
    [JsonPropertyName("method")]
    public string Method { get; } = "hosted_checkout";

    /// <summary>The <c>returnUrl</c> field.</summary>
    [JsonPropertyName("returnUrl")]
    public string ReturnUrl { get; init; } = string.Empty;

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
