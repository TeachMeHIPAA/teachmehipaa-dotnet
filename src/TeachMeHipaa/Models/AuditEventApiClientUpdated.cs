// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AuditEventApiClientUpdated</c> object.</summary>
/// <remarks>One kind of <see cref="AuditEvent"/>.</remarks>
public sealed record AuditEventApiClientUpdated : AuditEvent
{
    /// <summary>The <c>eventId</c> field.</summary>
    [JsonPropertyName("eventId")]
    public string EventId { get; init; } = string.Empty;

    /// <summary>The <c>at</c> field.</summary>
    [JsonPropertyName("at")]
    public string At { get; init; } = string.Empty;

    /// <summary>Always <c>api_client.updated</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "api_client.updated";

    /// <summary>The <c>actorSubjectId</c> field.</summary>
    [JsonPropertyName("actorSubjectId")]
    public string ActorSubjectId { get; init; } = string.Empty;

    /// <summary>The <c>actor</c> field.</summary>
    [JsonPropertyName("actor")]
    public AuditEventActor Actor { get; init; } = null!;

    /// <summary>The <c>clientId</c> field.</summary>
    [JsonPropertyName("clientId")]
    public string ClientId { get; init; } = string.Empty;

    /// <summary>The <c>fields</c> field.</summary>
    [JsonPropertyName("fields")]
    public IReadOnlyList<AuditEventApiClientUpdatedFieldsItem> Fields { get; init; } = [];

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
