// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EnrollmentActionAccepted</c> object.</summary>
public sealed record EnrollmentActionAccepted : TeachMeHipaaEntity
{
    /// <summary>The enrollment the request is for.</summary>
    [JsonPropertyName("enrollmentId")]
    public string EnrollmentId { get; init; } = string.Empty;

    /// <summary>The action that was accepted: <c>remind</c> or <c>resendInvite</c>.</summary>
    [JsonPropertyName("action")]
    public EnrollmentActionAcceptedAction Action { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
