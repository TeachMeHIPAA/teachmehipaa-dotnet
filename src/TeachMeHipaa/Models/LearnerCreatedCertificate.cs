// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>LearnerCreatedCertificate</c> object.</summary>
public sealed record LearnerCreatedCertificate : TeachMeHipaaEntity
{
    /// <summary>The <c>certificateId</c> field.</summary>
    [JsonPropertyName("certificateId")]
    public string CertificateId { get; init; } = string.Empty;

    /// <summary>The <c>issuedAt</c> field.</summary>
    [JsonPropertyName("issuedAt")]
    public DateTimeOffset IssuedAt { get; init; }

    /// <summary>The <c>expiresAt</c> field.</summary>
    [JsonPropertyName("expiresAt")]
    public DateTimeOffset ExpiresAt { get; init; }

    /// <summary>The <c>verifyUrl</c> field.</summary>
    [JsonPropertyName("verifyUrl")]
    public string? VerifyUrl { get; init; }

    /// <summary>The <c>pdfUrl</c> field.</summary>
    [JsonPropertyName("pdfUrl")]
    public string PdfUrl { get; init; } = string.Empty;

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
