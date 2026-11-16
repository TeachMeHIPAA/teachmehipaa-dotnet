// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EnrollmentsBatchRequestSelectFilterFilter</c> object.</summary>
public sealed record EnrollmentsBatchRequestSelectFilterFilter : TeachMeHipaaEntity
{
    /// <summary>The <c>status</c> field.</summary>
    [JsonPropertyName("status")]
    public EnrollmentsBatchRequestSelectFilterFilterStatus? Status { get; init; }

    /// <summary>The <c>courseId</c> field.</summary>
    [JsonPropertyName("courseId")]
    public string? CourseId { get; init; }

    /// <summary>The <c>archived</c> field.</summary>
    [JsonPropertyName("archived")]
    public bool? Archived { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
