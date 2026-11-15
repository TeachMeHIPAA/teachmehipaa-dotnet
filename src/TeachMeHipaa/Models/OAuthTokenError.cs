// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>OAuthTokenError</c> object.</summary>
public sealed record OAuthTokenError : TeachMeHipaaEntity
{
    /// <summary>The <c>error</c> field.</summary>
    [JsonPropertyName("error")]
    public OAuthErrorCode Error { get; init; }

    /// <summary>The <c>error_description</c> field.</summary>
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
