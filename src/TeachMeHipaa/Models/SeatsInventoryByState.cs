// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Seats by internal state. Prefer <c>purchased</c>, <c>assigned</c> and <c>available</c>.</summary>
public sealed record SeatsInventoryByState : TeachMeHipaaEntity
{
    /// <summary>The <c>UNUSED</c> field.</summary>
    [JsonPropertyName("UNUSED")]
    public long Unused { get; init; }

    /// <summary>The <c>ASSIGNED</c> field.</summary>
    [JsonPropertyName("ASSIGNED")]
    public long Assigned { get; init; }

    /// <summary>The <c>ACCEPTED</c> field.</summary>
    [JsonPropertyName("ACCEPTED")]
    public long Accepted { get; init; }

    /// <summary>The <c>REVOKED</c> field.</summary>
    [JsonPropertyName("REVOKED")]
    public long Revoked { get; init; }

    /// <summary>The <c>FREE</c> field.</summary>
    [JsonPropertyName("FREE")]
    public long Free { get; init; }

    /// <summary>The <c>REFUNDED</c> field.</summary>
    [JsonPropertyName("REFUNDED")]
    public long Refunded { get; init; }

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
