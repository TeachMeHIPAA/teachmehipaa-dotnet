// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="PartnerOrganizationService.CreateAsync"/>.</summary>
public sealed class PartnerOrganizationCreateOptions
{
    /// <summary>The organization's name. Required.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>The email address of the organization's administrator. Required.</summary>
    [JsonPropertyName("adminEmail")]
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>The administrator's full name.</summary>
    [JsonPropertyName("adminName")]
    public string? AdminName { get; set; }

    /// <summary>The scopes your app gets on the new organization. A subset of your app's scopes. Required.</summary>
    [JsonPropertyName("scopes")]
    public IReadOnlyList<PartnerOrganizationCreateScopesItem> Scopes { get; set; } = [];
}
