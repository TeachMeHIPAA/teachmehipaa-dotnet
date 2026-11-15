// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>PartnerOrganization</c> object.</summary>
public sealed record PartnerOrganization : TeachMeHipaaEntity
{
    /// <summary>The <c>orgId</c> field.</summary>
    [JsonPropertyName("orgId")]
    public string OrgId { get; init; } = string.Empty;

    /// <summary>The <c>name</c> field.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>The <c>createdAt</c> field.</summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>The <c>grantActive</c> field.</summary>
    [JsonPropertyName("grantActive")]
    public bool GrantActive { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
