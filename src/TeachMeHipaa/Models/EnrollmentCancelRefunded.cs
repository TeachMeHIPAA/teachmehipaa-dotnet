// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EnrollmentCancelRefunded</c> object.</summary>
/// <remarks>One kind of <see cref="CancelEnrollmentResponse"/>.</remarks>
public sealed record EnrollmentCancelRefunded : CancelEnrollmentResponse
{
    /// <summary>The <c>status</c> field.</summary>
    [JsonPropertyName("status")]
    public EnrollmentCancelRefundedStatus Status { get; init; }

    /// <summary>The <c>effectiveStatus</c> field.</summary>
    [JsonPropertyName("effectiveStatus")]
    public EnrollmentCancelRefundedEffectiveStatus EffectiveStatus { get; init; }

    /// <summary>The <c>refund</c> field.</summary>
    [JsonPropertyName("refund")]
    public EnrollmentCancelRefundedRefund Refund { get; init; } = new();

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
