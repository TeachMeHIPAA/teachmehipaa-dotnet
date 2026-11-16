// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EnrollmentsBatchRequestSelectFilter</c> object.</summary>
/// <remarks>One kind of <see cref="EnrollmentsBatchRequestSelect"/>.</remarks>
public sealed record EnrollmentsBatchRequestSelectFilter : EnrollmentsBatchRequestSelect
{
    /// <summary>The <c>filter</c> field.</summary>
    [JsonPropertyName("filter")]
    public EnrollmentsBatchRequestSelectFilterFilter Filter { get; init; } = new();

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
