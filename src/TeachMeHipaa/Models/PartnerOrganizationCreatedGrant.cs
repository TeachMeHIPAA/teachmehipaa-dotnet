// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Your app's connection to the organization.</summary>
public sealed record PartnerOrganizationCreatedGrant : TeachMeHipaaEntity
{
    /// <summary>The <c>grantId</c> field.</summary>
    [JsonPropertyName("grantId")]
    public string GrantId { get; init; } = string.Empty;

    /// <summary>The <c>scopes</c> field.</summary>
    [JsonPropertyName("scopes")]
    public IReadOnlyList<PartnerOrganizationCreatedGrantScopesItem> Scopes { get; init; } = [];

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
