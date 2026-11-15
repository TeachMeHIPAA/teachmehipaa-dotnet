// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>A <see cref="SeatOrderCreatedNextAction"/> of a kind this version of the library doesn't know. Its other fields are kept in <see cref="ExtensionData"/>.</summary>
public sealed record UnknownSeatOrderCreatedNextAction : SeatOrderCreatedNextAction
{
    /// <summary>The kind, as the API sent it.</summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
