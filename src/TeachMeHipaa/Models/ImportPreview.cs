// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>ImportPreview</c> object.</summary>
public sealed record ImportPreview : TeachMeHipaaEntity
{
    /// <summary>Rows in the file.</summary>
    [JsonPropertyName("totalRows")]
    public long TotalRows { get; init; }

    /// <summary>Rows that would be invited.</summary>
    [JsonPropertyName("validRows")]
    public long ValidRows { get; init; }

    /// <summary>Seats the import would use.</summary>
    [JsonPropertyName("seatsRequired")]
    public long SeatsRequired { get; init; }

    /// <summary>Seats available now.</summary>
    [JsonPropertyName("seatsAvailable")]
    public long SeatsAvailable { get; init; }

    /// <summary>Each row's result: <c>valid</c>, <c>invalid_email</c>, <c>duplicate_in_file</c>, <c>already_enrolled</c> or <c>already_invited</c>.</summary>
    [JsonPropertyName("rows")]
    public IReadOnlyList<ImportPreviewRowsItem> Rows { get; init; } = [];

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
