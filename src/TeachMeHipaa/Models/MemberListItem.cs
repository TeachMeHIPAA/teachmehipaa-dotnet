// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>MemberListItem</c> object.</summary>
public sealed record MemberListItem : TeachMeHipaaEntity
{
    /// <summary>The <c>subjectId</c> field.</summary>
    [JsonPropertyName("subjectId")]
    public string? SubjectId { get; init; }

    /// <summary>The <c>email</c> field.</summary>
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    /// <summary>The <c>name</c> field.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>The <c>role</c> field.</summary>
    [JsonPropertyName("role")]
    public MemberListItemRole Role { get; init; }

    /// <summary>The <c>isDisabled</c> field.</summary>
    [JsonPropertyName("isDisabled")]
    public bool IsDisabled { get; init; }

    /// <summary>The <c>enrollmentSubjectId</c> field.</summary>
    [JsonPropertyName("enrollmentSubjectId")]
    public string? EnrollmentSubjectId { get; init; }

    /// <summary>The <c>holdsInvitation</c> field.</summary>
    [JsonPropertyName("holdsInvitation")]
    public bool? HoldsInvitation { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
