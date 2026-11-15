// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>SeatOrderCreateRequestPaymentInvoice</c> object.</summary>
/// <remarks>One kind of <see cref="SeatOrderCreateRequestPayment"/>.</remarks>
public sealed record SeatOrderCreateRequestPaymentInvoice : SeatOrderCreateRequestPayment
{
    /// <summary>Always <c>invoice</c>.</summary>
    [JsonPropertyName("method")]
    public string Method { get; } = "invoice";

    /// <summary>The <c>daysUntilDue</c> field.</summary>
    [JsonPropertyName("daysUntilDue")]
    public long? DaysUntilDue { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
