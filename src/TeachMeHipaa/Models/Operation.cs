// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>Operation</c> object.</summary>
public sealed partial record Operation : TeachMeHipaaEntity
{
    /// <summary>Unique identifier for the operation.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>Whether the operation has finished.</summary>
    [JsonPropertyName("done")]
    public bool Done { get; init; }

    /// <summary>Progress information, if any.</summary>
    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, JsonElement>? Metadata { get; init; }

    /// <summary>The result, once <c>done</c> is <c>true</c>.</summary>
    [JsonPropertyName("response")]
    public JsonElement? Response { get; init; }

    /// <summary>The problem, if the operation failed.</summary>
    [JsonPropertyName("error")]
    public Problem? Error { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
