// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>PaymentMethodPrepare</c> object.</summary>
public sealed record PaymentMethodPrepare : TeachMeHipaaEntity
{
    /// <summary>The Stripe SetupIntent.</summary>
    [JsonPropertyName("setupIntentId")]
    public string SetupIntentId { get; init; } = string.Empty;

    /// <summary>Confirm the SetupIntent with Stripe.js using this secret.</summary>
    [JsonPropertyName("clientSecret")]
    public string ClientSecret { get; init; } = string.Empty;

    /// <summary>The Stripe publishable key to load Stripe.js with.</summary>
    [JsonPropertyName("publishableKey")]
    public string PublishableKey { get; init; } = string.Empty;

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
