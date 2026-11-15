// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>SeatsInventory</c> object.</summary>
public sealed record SeatsInventory : TeachMeHipaaEntity
{
    /// <summary>All seats on record.</summary>
    [JsonPropertyName("total")]
    public long Total { get; init; }

    /// <summary>Seats in use.</summary>
    [JsonPropertyName("used")]
    public long Used { get; init; }

    /// <summary>Seats a new invite can use now.</summary>
    [JsonPropertyName("available")]
    public long Available { get; init; }

    /// <summary>Seats the organization holds and hasn't been refunded for, including a trial seat.</summary>
    [JsonPropertyName("purchased")]
    public long? Purchased { get; init; }

    /// <summary>Seats held by an invited or enrolled learner.</summary>
    [JsonPropertyName("assigned")]
    public long? Assigned { get; init; }

    /// <summary>Seats by internal state. Prefer <c>purchased</c>, <c>assigned</c> and <c>available</c>.</summary>
    [JsonPropertyName("byState")]
    public SeatsInventoryByState ByState { get; init; } = new();

    /// <summary>Fields this version of the library doesn't know yet, kept as sent.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
}
