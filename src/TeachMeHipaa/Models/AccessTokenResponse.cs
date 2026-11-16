// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AccessTokenResponse</c> object.</summary>
public sealed record AccessTokenResponse : TeachMeHipaaEntity
{
    /// <summary>The <c>tokenType</c> field.</summary>
    [JsonPropertyName("tokenType")]
    public AccessTokenResponseTokenType TokenType { get; init; }

    /// <summary>The <c>accessToken</c> field.</summary>
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>The <c>expiresInSec</c> field.</summary>
    [JsonPropertyName("expiresInSec")]
    public long ExpiresInSec { get; init; }

    /// <summary>The <c>scopes</c> field.</summary>
    [JsonPropertyName("scopes")]
    public IReadOnlyList<AccessTokenResponseScopesItem> Scopes { get; init; } = [];

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
