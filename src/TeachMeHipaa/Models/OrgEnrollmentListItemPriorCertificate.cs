// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>OrgEnrollmentListItemPriorCertificate</c> object.</summary>
public sealed record OrgEnrollmentListItemPriorCertificate : TeachMeHipaaEntity
{
    /// <summary>The <c>certificateId</c> field.</summary>
    [JsonPropertyName("certificateId")]
    public string CertificateId { get; init; } = string.Empty;

    /// <summary>The <c>issuedAt</c> field.</summary>
    [JsonPropertyName("issuedAt")]
    public string? IssuedAt { get; init; }

    /// <summary>The <c>expiresAt</c> field.</summary>
    [JsonPropertyName("expiresAt")]
    public string? ExpiresAt { get; init; }

    /// <summary>The <c>status</c> field.</summary>
    [JsonPropertyName("status")]
    public OrgEnrollmentListItemPriorCertificateStatus Status { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
