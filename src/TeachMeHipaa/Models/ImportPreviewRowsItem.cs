// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>ImportPreviewRowsItem</c> object.</summary>
public sealed record ImportPreviewRowsItem : TeachMeHipaaEntity
{
    /// <summary>The <c>rowNumber</c> field.</summary>
    [JsonPropertyName("rowNumber")]
    public long RowNumber { get; init; }

    /// <summary>The <c>email</c> field.</summary>
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    /// <summary>The <c>name</c> field.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>The <c>courseId</c> field.</summary>
    [JsonPropertyName("courseId")]
    public string CourseId { get; init; } = string.Empty;

    /// <summary>The <c>status</c> field.</summary>
    [JsonPropertyName("status")]
    public ImportPreviewRowsItemStatus Status { get; init; }

    /// <summary>The <c>detail</c> field.</summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
