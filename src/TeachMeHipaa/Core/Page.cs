using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>One page of a list, in the API's order.</summary>
/// <typeparam name="T">The listed object.</typeparam>
public sealed record Page<T> : TeachMeHipaaEntity
{
    /// <summary>The objects on this page.</summary>
    [JsonPropertyName("items")]
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>Pass as the list's cursor to fetch the next page; null on the last page.</summary>
    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; init; }
}
