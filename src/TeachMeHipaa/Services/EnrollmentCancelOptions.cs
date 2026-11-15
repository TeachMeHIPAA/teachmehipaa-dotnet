// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="EnrollmentService.CancelAsync"/>.</summary>
public sealed class EnrollmentCancelOptions
{
    /// <summary>Refund the seat to the card that paid for it instead of returning it to the pool. Defaults to <c>false</c>.</summary>
    [JsonPropertyName("refund")]
    public bool? Refund { get; set; }
}
