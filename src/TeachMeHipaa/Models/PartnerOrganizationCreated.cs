// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>PartnerOrganizationCreated</c> object.</summary>
public sealed record PartnerOrganizationCreated : TeachMeHipaaEntity
{
    /// <summary>The new organization's id.</summary>
    [JsonPropertyName("orgId")]
    public string OrgId { get; init; } = string.Empty;

    /// <summary>The organization's name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>When it was created (ISO 8601).</summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Your app's connection to the organization.</summary>
    [JsonPropertyName("grant")]
    public PartnerOrganizationCreatedGrant Grant { get; init; } = new();

    /// <summary>Returned only once. Exchange it at <c>POST /v1/auth/token</c> for the organization's tokens.</summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
