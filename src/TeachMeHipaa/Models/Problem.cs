// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>Problem</c> object.</summary>
public sealed record Problem : TeachMeHipaaEntity
{
    /// <summary>A URI identifying the problem type.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>A short summary of the problem.</summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>The HTTP status code.</summary>
    [JsonPropertyName("status")]
    public long Status { get; init; }

    /// <summary>An explanation of this occurrence.</summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    /// <summary>Identifies this occurrence.</summary>
    [JsonPropertyName("instance")]
    public string? Instance { get; init; }

    /// <summary>A machine-readable error code, such as <c>seat_unavailable</c>.</summary>
    [JsonPropertyName("code")]
    public ProblemCode Code { get; init; }

    /// <summary>Each invalid field, with its <c>path</c> and <c>message</c>.</summary>
    [JsonPropertyName("errors")]
    public IReadOnlyList<ProblemErrorsItem>? Errors { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
