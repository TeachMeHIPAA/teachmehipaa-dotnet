// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EnrollmentsBatchRequestSelectEnrollmentIds</c> object.</summary>
/// <remarks>One kind of <see cref="EnrollmentsBatchRequestSelect"/>.</remarks>
public sealed record EnrollmentsBatchRequestSelectEnrollmentIds : EnrollmentsBatchRequestSelect
{
    /// <summary>The <c>enrollmentIds</c> field.</summary>
    [JsonPropertyName("enrollmentIds")]
    public IReadOnlyList<string> EnrollmentIds { get; init; } = [];

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
