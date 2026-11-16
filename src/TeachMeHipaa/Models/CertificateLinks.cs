// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>CertificateLinks</c> object.</summary>
public sealed record CertificateLinks : TeachMeHipaaEntity
{
    /// <summary>The certificate.</summary>
    [JsonPropertyName("certificateId")]
    public string CertificateId { get; init; } = string.Empty;

    /// <summary>Adds the certificate to the holder's LinkedIn profile.</summary>
    [JsonPropertyName("linkedInUrl")]
    public string LinkedInUrl { get; init; } = string.Empty;

    /// <summary>The public <c>tme.so</c> verification link.</summary>
    [JsonPropertyName("shortLink")]
    public string ShortLink { get; init; } = string.Empty;

    /// <summary>The verification link's id.</summary>
    [JsonPropertyName("tinyId")]
    public string TinyId { get; init; } = string.Empty;

    /// <summary>The <c>paywalled</c> field.</summary>
    [JsonPropertyName("paywalled")]
    public bool? Paywalled { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
