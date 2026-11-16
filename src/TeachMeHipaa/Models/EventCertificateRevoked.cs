// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EventCertificateRevoked</c> object.</summary>
/// <remarks>One kind of <see cref="Event"/>.</remarks>
public sealed record EventCertificateRevoked : Event
{
    /// <summary>Always <c>certificate.revoked</c>.</summary>
    [JsonPropertyName("eventType")]
    public string EventType { get; } = "certificate.revoked";

    /// <summary>The <c>data</c> field.</summary>
    [JsonPropertyName("data")]
    public CertificateRevokedEventData Data { get; init; } = new();

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
