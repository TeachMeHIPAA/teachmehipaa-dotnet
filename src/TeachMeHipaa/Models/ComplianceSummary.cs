// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>ComplianceSummary</c> object.</summary>
public sealed record ComplianceSummary : TeachMeHipaaEntity
{
    /// <summary>When the counts were taken (ISO 8601).</summary>
    [JsonPropertyName("asOf")]
    public DateTimeOffset AsOf { get; init; }

    /// <summary>The course.</summary>
    [JsonPropertyName("courseId")]
    public string CourseId { get; init; } = string.Empty;

    /// <summary>Learners by status: <c>learners</c>, <c>compliant</c>, <c>expiringSoon</c>, <c>expired</c>, <c>inProgress</c>, <c>notStarted</c> and <c>inviteBounced</c>.</summary>
    [JsonPropertyName("totals")]
    public ComplianceSummaryTotals Totals { get; init; } = new();

    /// <summary><c>compliant</c> divided by <c>learners</c>, or 0 with no learners.</summary>
    [JsonPropertyName("compliantRate")]
    public double CompliantRate { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
