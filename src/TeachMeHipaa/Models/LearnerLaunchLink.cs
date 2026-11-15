using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>A one-time URL that signs a Learner straight into their Training (<c>dev#/components/schemas/LearnerLaunchLink</c>).</summary>
public sealed record LearnerLaunchLink : TeachMeHipaaEntity
{
    /// <summary>The single-use sign-in URL. Redirect the Learner to it right away.</summary>
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    /// <summary>When the URL stops working, 5 minutes after it was created.</summary>
    [JsonPropertyName("expiresAt")]
    public DateTimeOffset ExpiresAt { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
