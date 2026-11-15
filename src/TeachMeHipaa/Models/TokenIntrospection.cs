// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>TokenIntrospection</c> object.</summary>
public sealed record TokenIntrospection : TeachMeHipaaEntity
{
    /// <summary><c>client</c> for an API client or partner app, <c>user</c> for a person.</summary>
    [JsonPropertyName("actorType")]
    public TokenIntrospectionActorType ActorType { get; init; }

    /// <summary>The API client or partner app the token belongs to.</summary>
    [JsonPropertyName("clientId")]
    public string? ClientId { get; init; }

    /// <summary>The person the token belongs to, for a user token.</summary>
    [JsonPropertyName("subjectId")]
    public string? SubjectId { get; init; }

    /// <summary>The organization the token is bound to, if any.</summary>
    [JsonPropertyName("orgId")]
    public string? OrgId { get; init; }

    /// <summary>The scopes the token holds.</summary>
    [JsonPropertyName("scopes")]
    public IReadOnlyList<TokenIntrospectionScopesItem> Scopes { get; init; } = [];

    /// <summary><c>test</c> for a Sandbox client, <c>live</c> otherwise.</summary>
    [JsonPropertyName("mode")]
    public TokenIntrospectionMode? Mode { get; init; }

    /// <summary>When the token expires (ISO 8601).</summary>
    [JsonPropertyName("expiresAt")]
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
