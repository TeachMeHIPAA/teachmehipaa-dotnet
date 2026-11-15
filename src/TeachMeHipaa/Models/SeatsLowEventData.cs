// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>SeatsLowEventData</c> object.</summary>
public sealed record SeatsLowEventData : TeachMeHipaaEntity
{
    /// <summary>The <c>available</c> field.</summary>
    [JsonPropertyName("available")]
    public long Available { get; init; }

    /// <summary>The <c>threshold</c> field.</summary>
    [JsonPropertyName("threshold")]
    public long Threshold { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
