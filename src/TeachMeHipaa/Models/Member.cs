// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>Member</c> object.</summary>
public sealed record Member : TeachMeHipaaEntity
{
    /// <summary>Unique identifier for the member. <c>null</c> for an older member who can't be addressed yet.</summary>
    [JsonPropertyName("subjectId")]
    public string? SubjectId { get; init; }

    /// <summary>The member's email address.</summary>
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    /// <summary>The member's full name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary><c>admin</c> or <c>learner</c>.</summary>
    [JsonPropertyName("role")]
    public MemberRole Role { get; init; }

    /// <summary>Whether the member's account is disabled.</summary>
    [JsonPropertyName("isDisabled")]
    public bool IsDisabled { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
