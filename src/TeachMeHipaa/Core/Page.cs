using System.Runtime.CompilerServices;

namespace TeachMeHipaa;

/// <summary>
/// One page of a list, in the API's order, that can also fetch the pages
/// after it. Iterate it with <c>await foreach</c> for every item from this
/// page onward; later pages are fetched only as you reach them.
/// </summary>
/// <typeparam name="T">The listed object.</typeparam>
public sealed record Page<T> : TeachMeHipaaEntity, IAsyncEnumerable<T>
{
    /// <summary>The objects on this page.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>Pass as the list's cursor to fetch the next page; null on the last page.</summary>
    public string? NextCursor { get; init; }

    /// <summary>Whether more items follow: <see cref="NextCursor"/> is not null.</summary>
    public bool HasNextPage => NextCursor is not null;

    /// <summary>Fetches the page after this one; null for a page the library didn't fetch.</summary>
    internal Func<CancellationToken, Task<Page<T>>>? FetchNext { get; init; }

    /// <summary>
    /// Fetches the next page with the same filters, limit and
    /// <see cref="RequestOptions"/>. Check <see cref="HasNextPage"/> first.
    /// </summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The next page, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="InvalidOperationException">This is the last page, or a page you built yourself.</exception>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Page<T>> GetNextPageAsync(CancellationToken cancellationToken = default)
    {
        if (!HasNextPage)
        {
            throw new InvalidOperationException("This is the last page; check HasNextPage first.");
        }

        return FetchNext is null
            ? throw new InvalidOperationException("Only a page the client fetched can fetch the next one.")
            : FetchNext(cancellationToken);
    }

    /// <summary>This page, then each page after it, fetched only as you ask for it.</summary>
    /// <param name="cancellationToken">Stops the iteration with an <see cref="OperationCanceledException"/>.</param>
    public async IAsyncEnumerable<Page<T>> PagesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = this;
        yield return page;
        while (page.HasNextPage)
        {
            cancellationToken.ThrowIfCancellationRequested();
            page = await page.GetNextPageAsync(cancellationToken).ConfigureAwait(false);
            yield return page;
        }
    }

    /// <summary>
    /// Every item on this page and the pages after it, fetched only as you
    /// reach them. Breaking out of the loop fetches nothing more.
    /// </summary>
    /// <param name="cancellationToken">Stops the iteration with an <see cref="OperationCanceledException"/>.</param>
    public async IAsyncEnumerable<T> AutoPagingAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var page in PagesAsync(cancellationToken).ConfigureAwait(false))
        {
            foreach (var item in page.Items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return item;
            }
        }
    }

    /// <inheritdoc cref="AutoPagingAsync"/>
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        AutoPagingAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);
}
