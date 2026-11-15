// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EnrollmentCancelRefundedRefund</c> object.</summary>
public sealed record EnrollmentCancelRefundedRefund : TeachMeHipaaEntity
{
    /// <summary>The <c>refundId</c> field.</summary>
    [JsonPropertyName("refundId")]
    public string RefundId { get; init; } = string.Empty;

    /// <summary>The <c>amountCents</c> field.</summary>
    [JsonPropertyName("amountCents")]
    public long AmountCents { get; init; }

    /// <summary>The <c>currency</c> field.</summary>
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
