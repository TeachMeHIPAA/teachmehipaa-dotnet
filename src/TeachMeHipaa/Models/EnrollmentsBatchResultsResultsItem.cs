// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EnrollmentsBatchResultsResultsItem</c> object.</summary>
public sealed record EnrollmentsBatchResultsResultsItem : TeachMeHipaaEntity
{
    /// <summary>The <c>enrollmentId</c> field.</summary>
    [JsonPropertyName("enrollmentId")]
    public string EnrollmentId { get; init; } = string.Empty;

    /// <summary>The <c>outcome</c> field.</summary>
    [JsonPropertyName("outcome")]
    public EnrollmentsBatchResultsResultsItemOutcome Outcome { get; init; }

    /// <summary>The <c>problem</c> field.</summary>
    [JsonPropertyName("problem")]
    public Problem? Problem { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
