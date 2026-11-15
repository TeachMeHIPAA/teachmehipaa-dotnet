// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AuditEventSeatOrderCreated</c> object.</summary>
/// <remarks>One kind of <see cref="AuditEvent"/>.</remarks>
public sealed record AuditEventSeatOrderCreated : AuditEvent
{
    /// <summary>The <c>eventId</c> field.</summary>
    [JsonPropertyName("eventId")]
    public string EventId { get; init; } = string.Empty;

    /// <summary>The <c>at</c> field.</summary>
    [JsonPropertyName("at")]
    public string At { get; init; } = string.Empty;

    /// <summary>The <c>actorSubjectId</c> field.</summary>
    [JsonPropertyName("actorSubjectId")]
    public string ActorSubjectId { get; init; } = string.Empty;

    /// <summary>The <c>actor</c> field.</summary>
    [JsonPropertyName("actor")]
    public AuditEventActor Actor { get; init; } = null!;

    /// <summary>The <c>seatOrderId</c> field.</summary>
    [JsonPropertyName("seatOrderId")]
    public string SeatOrderId { get; init; } = string.Empty;

    /// <summary>The <c>quantity</c> field.</summary>
    [JsonPropertyName("quantity")]
    public long Quantity { get; init; }

    /// <summary>The <c>paymentMethod</c> field.</summary>
    [JsonPropertyName("paymentMethod")]
    public AuditEventPaymentMethod PaymentMethod { get; init; }

    /// <summary>Always <c>seat_order.created</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "seat_order.created";

    /// <summary>The <c>grantId</c> field.</summary>
    [JsonPropertyName("grantId")]
    public string? GrantId { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
