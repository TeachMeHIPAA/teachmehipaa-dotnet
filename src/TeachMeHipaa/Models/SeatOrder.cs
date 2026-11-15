// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>SeatOrder</c> object.</summary>
public sealed record SeatOrder : TeachMeHipaaEntity
{
    /// <summary>Unique identifier for the seat order.</summary>
    [JsonPropertyName("seatOrderId")]
    public string SeatOrderId { get; init; } = string.Empty;

    /// <summary>The organization's id.</summary>
    [JsonPropertyName("orgId")]
    public string OrgId { get; init; } = string.Empty;

    /// <summary>Seats ordered.</summary>
    [JsonPropertyName("quantity")]
    public long Quantity { get; init; }

    /// <summary>Price per seat, in the currency's smallest unit.</summary>
    [JsonPropertyName("unitAmount")]
    public long UnitAmount { get; init; }

    /// <summary>Three-letter ISO currency code, lowercase.</summary>
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>The order total, in the currency's smallest unit.</summary>
    [JsonPropertyName("total")]
    public long Total { get; init; }

    /// <summary><c>hosted_checkout</c>, <c>saved_payment_method</c> or <c>invoice</c>.</summary>
    [JsonPropertyName("paymentMethod")]
    public SeatOrderPaymentMethod PaymentMethod { get; init; }

    /// <summary><c>requires_action</c>, <c>processing</c>, <c>invoiced</c>, <c>paid</c>, <c>failed</c>, <c>expired</c> or <c>refunded</c>.</summary>
    [JsonPropertyName("status")]
    public SeatOrderStatus Status { get; init; }

    /// <summary>Seats granted so far.</summary>
    [JsonPropertyName("seatsGranted")]
    public long SeatsGranted { get; init; }

    /// <summary>Seats refunded through support.</summary>
    [JsonPropertyName("seatsRefunded")]
    public long SeatsRefunded { get; init; }

    /// <summary>Why the order failed, with Stripe's decline code when there is one.</summary>
    [JsonPropertyName("failure")]
    public SeatOrderFailure? Failure { get; init; }

    /// <summary>The API client that placed the order, if one did.</summary>
    [JsonPropertyName("clientId")]
    public string? ClientId { get; init; }

    /// <summary>Your own reference for the order.</summary>
    [JsonPropertyName("clientReference")]
    public string? ClientReference { get; init; }

    /// <summary>The Stripe checkout session, for a hosted checkout.</summary>
    [JsonPropertyName("checkoutId")]
    public string? CheckoutId { get; init; }

    /// <summary>When it was created (ISO 8601).</summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>When it last changed.</summary>
    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>The resource's revision. Its strong <c>ETag</c> is derived from it.</summary>
    [JsonPropertyName("rev")]
    public long Rev { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
