// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>CertificateVerification</c> object.</summary>
public sealed record CertificateVerification : TeachMeHipaaEntity
{
    /// <summary><c>valid</c>, <c>expired</c> or <c>revoked</c>.</summary>
    [JsonPropertyName("status")]
    public CertificateVerificationStatus Status { get; init; }

    /// <summary>The name printed on the certificate.</summary>
    [JsonPropertyName("learnerName")]
    public string LearnerName { get; init; } = string.Empty;

    /// <summary>The course.</summary>
    [JsonPropertyName("courseTitle")]
    public string CourseTitle { get; init; } = string.Empty;

    /// <summary>When it was issued (ISO 8601).</summary>
    [JsonPropertyName("issuedAt")]
    public DateTimeOffset IssuedAt { get; init; }

    /// <summary>When it expires.</summary>
    [JsonPropertyName("expiresAt")]
    public DateTimeOffset ExpiresAt { get; init; }

    /// <summary>Who issued it.</summary>
    [JsonPropertyName("issuer")]
    public CertificateVerificationIssuer Issuer { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
