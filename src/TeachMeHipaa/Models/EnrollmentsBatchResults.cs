// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EnrollmentsBatchResults</c> object.</summary>
public sealed record EnrollmentsBatchResults : TeachMeHipaaEntity
{
    /// <summary>The operation that was run.</summary>
    [JsonPropertyName("operation")]
    public EnrollmentsBatchResultsOperation Operation { get; init; }

    /// <summary>One result per enrollment: <c>succeeded</c>, <c>skipped</c> or <c>failed</c> with a problem.</summary>
    [JsonPropertyName("results")]
    public IReadOnlyList<EnrollmentsBatchResultsResultsItem> Results { get; init; } = [];

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
