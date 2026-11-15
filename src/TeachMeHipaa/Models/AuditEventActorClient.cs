// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AuditEventActorClient</c> object.</summary>
/// <remarks>One kind of <see cref="AuditEventActor"/>.</remarks>
public sealed record AuditEventActorClient : AuditEventActor
{
    /// <summary>Always <c>client</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "client";

    /// <summary>The <c>clientId</c> field.</summary>
    [JsonPropertyName("clientId")]
    public string ClientId { get; init; } = string.Empty;

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
