// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>Partner</c> object.</summary>
public sealed record Partner : TeachMeHipaaEntity
{
    /// <summary>Unique identifier for the partner account.</summary>
    [JsonPropertyName("partnerId")]
    public string PartnerId { get; init; } = string.Empty;

    /// <summary>The partner's name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>The partner app.</summary>
    [JsonPropertyName("appId")]
    public string AppId { get; init; } = string.Empty;

    /// <summary><c>active</c> or <c>suspended</c>.</summary>
    [JsonPropertyName("status")]
    public PartnerStatus Status { get; init; }

    /// <summary>How many organizations the partner can create.</summary>
    [JsonPropertyName("maxChildOrgs")]
    public long MaxChildOrgs { get; init; }

    /// <summary>How many it has created.</summary>
    [JsonPropertyName("childOrgCount")]
    public long ChildOrgCount { get; init; }

    /// <summary>When it was created (ISO 8601).</summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
