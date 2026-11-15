// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AuditEventOrganizationCreatedByPartner</c> object.</summary>
/// <remarks>One kind of <see cref="AuditEvent"/>.</remarks>
public sealed record AuditEventOrganizationCreatedByPartner : AuditEvent
{
    /// <summary>The <c>eventId</c> field.</summary>
    [JsonPropertyName("eventId")]
    public string EventId { get; init; } = string.Empty;

    /// <summary>The <c>at</c> field.</summary>
    [JsonPropertyName("at")]
    public string At { get; init; } = string.Empty;

    /// <summary>Always <c>organization.created_by_partner</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "organization.created_by_partner";

    /// <summary>The <c>actorSubjectId</c> field.</summary>
    [JsonPropertyName("actorSubjectId")]
    public string ActorSubjectId { get; init; } = string.Empty;

    /// <summary>The <c>actor</c> field.</summary>
    [JsonPropertyName("actor")]
    public AuditEventActor Actor { get; init; } = null!;

    /// <summary>The <c>partnerId</c> field.</summary>
    [JsonPropertyName("partnerId")]
    public string PartnerId { get; init; } = string.Empty;

    /// <summary>The <c>appId</c> field.</summary>
    [JsonPropertyName("appId")]
    public string AppId { get; init; } = string.Empty;

    /// <summary>The <c>grantId</c> field.</summary>
    [JsonPropertyName("grantId")]
    public string GrantId { get; init; } = string.Empty;

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
