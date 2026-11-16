// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>ClientCredentialsTokenResponse</c> object.</summary>
public sealed record ClientCredentialsTokenResponse : TeachMeHipaaEntity
{
    /// <summary>The access token. Send it as <c>Authorization: Bearer</c>.</summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>Always <c>Bearer</c>.</summary>
    [JsonPropertyName("token_type")]
    public ClientCredentialsTokenResponseTokenType TokenType { get; init; }

    /// <summary>Seconds until the token expires.</summary>
    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; init; }

    /// <summary>The scopes the token holds, space-delimited.</summary>
    [JsonPropertyName("scope")]
    public string Scope { get; init; } = string.Empty;

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
