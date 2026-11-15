using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>
/// The wire shape of a page, <c>{ items, nextCursor }</c>. <see cref="Page{T}"/>
/// itself isn't deserialized: System.Text.Json reads any
/// <see cref="IAsyncEnumerable{T}"/> as a JSON array.
/// </summary>
internal sealed record PageEnvelope<T> : TeachMeHipaaEntity
{
    [JsonPropertyName("items")]
    public IReadOnlyList<T> Items { get; init; } = [];

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; init; }
}

/// <summary>How a list asks for the page after one it fetched, from the IR's <c>pagination</c>.</summary>
internal sealed class PagePosition<T>
{
    private readonly Func<T, string>? _positionOf;

    private PagePosition(string parameter, Func<T, string>? positionOf)
    {
        Parameter = parameter;
        _positionOf = positionOf;
    }

    /// <summary>The query parameter that carries the position, such as <c>cursor</c>.</summary>
    public string Parameter { get; }

    /// <summary>A <c>cursor</c> list: the next page starts at the opaque <c>nextCursor</c>.</summary>
    public static PagePosition<T> Cursor(string parameter) => new(parameter, null);

    /// <summary>
    /// An <c>after</c> list, the Event log: the next page starts after the
    /// last item's id, which is the authoritative position; <c>nextCursor</c>
    /// only spells it.
    /// </summary>
    public static PagePosition<T> After(string parameter, Func<T, string> idOf) => new(parameter, idOf);

    /// <summary>Where the page after <paramref name="items"/> starts.</summary>
    public string NextPosition(IReadOnlyList<T> items, string nextCursor) =>
        _positionOf is null || items.Count == 0 ? nextCursor : _positionOf(items[items.Count - 1]);
}

/// <summary>The generated <c>ListAutoPagingAsync</c> methods.</summary>
internal static class AutoPaging
{
    /// <summary>
    /// Every item of every page, starting with the page
    /// <paramref name="firstPage"/> fetches. Nothing is fetched until the
    /// caller starts iterating.
    /// </summary>
    public static async IAsyncEnumerable<T> Of<T>(
        Func<CancellationToken, Task<Page<T>>> firstPage,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var page = await firstPage(cancellationToken).ConfigureAwait(false);
        await foreach (var item in page.AutoPagingAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }
    }
}
