// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>Certificate</c> object.</summary>
public sealed record Certificate : TeachMeHipaaEntity
{
    /// <summary>Unique identifier for the certificate.</summary>
    [JsonPropertyName("certificateId")]
    public string CertificateId { get; init; } = string.Empty;

    /// <summary>The holder's member id.</summary>
    [JsonPropertyName("subjectId")]
    public string? SubjectId { get; init; }

    /// <summary>Whether the holder could be resolved: <c>resolved</c>, <c>none</c> or <c>lookup_failed</c>.</summary>
    [JsonPropertyName("holderStatus")]
    public CertificateHolderStatus? HolderStatus { get; init; }

    /// <summary>The course the certificate is for.</summary>
    [JsonPropertyName("courseId")]
    public string CourseId { get; init; } = string.Empty;

    /// <summary>The course version the holder passed.</summary>
    [JsonPropertyName("courseVersion")]
    public string? CourseVersion { get; init; }

    /// <summary>When it was issued (ISO 8601).</summary>
    [JsonPropertyName("issuedAt")]
    public string? IssuedAt { get; init; }

    /// <summary>When it expires.</summary>
    [JsonPropertyName("expiresAt")]
    public string? ExpiresAt { get; init; }

    /// <summary><c>VALID</c>, <c>EXPIRED</c> or <c>REVOKED</c>.</summary>
    [JsonPropertyName("status")]
    public CertificateStatus Status { get; init; }

    /// <summary>When it was revoked, if it was.</summary>
    [JsonPropertyName("revokedAt")]
    public string? RevokedAt { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
