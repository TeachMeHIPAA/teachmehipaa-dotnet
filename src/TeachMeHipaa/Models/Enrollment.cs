// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>Enrollment</c> object.</summary>
/// <remarks>One kind of <see cref="CancelEnrollmentResponse"/>.</remarks>
public sealed record Enrollment : CancelEnrollmentResponse
{
    /// <summary>The stored lifecycle state: <c>INVITED</c>, <c>EMAIL_BOUNCED</c>, <c>STARTED</c>, <c>ASSESSMENT_IN_PROGRESS</c>, <c>CERTIFIED</c> or <c>CANCELLED</c>.</summary>
    [JsonPropertyName("status")]
    public EnrollmentStatus Status { get; init; }

    /// <summary>The status to display. Adds <c>EXPIRING</c>, <c>EXPIRED</c> and <c>ARCHIVED</c>, derived when you read it.</summary>
    [JsonPropertyName("effectiveStatus")]
    public EnrollmentEffectiveStatus EffectiveStatus { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
