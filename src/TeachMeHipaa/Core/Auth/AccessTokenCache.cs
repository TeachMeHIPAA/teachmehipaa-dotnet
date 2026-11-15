using System.Diagnostics.CodeAnalysis;

namespace TeachMeHipaa;

/// <summary>An Access token and when it stops working.</summary>
internal sealed record CachedToken(string AccessToken, DateTimeOffset ExpiresAt);

/// <summary>
/// Caches an Access token until 60 s before it expires and lets only one mint
/// run at a time, so concurrent calls share it.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "SemaphoreSlim without AvailableWaitHandle holds no unmanaged resources.")]
internal sealed class AccessTokenCache(Func<CancellationToken, Task<CachedToken>> mint, TimeProvider timeProvider)
{
    private static readonly TimeSpan RemintMargin = TimeSpan.FromSeconds(60);

    private readonly SemaphoreSlim _mintLock = new(1, 1);
    private volatile CachedToken? _current;

    public async Task<string> GetAsync(CancellationToken cancellationToken)
    {
        var current = _current;
        if (current is not null && IsFresh(current))
        {
            return current.AccessToken;
        }

        await _mintLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Callers that queued behind a mint reuse its token.
            current = _current;
            if (current is null || !IsFresh(current))
            {
                current = await mint(cancellationToken).ConfigureAwait(false);
                _current = current;
            }

            return current.AccessToken;
        }
        finally
        {
            _mintLock.Release();
        }
    }

    public void Invalidate(string accessToken)
    {
        var current = _current;
        if (current?.AccessToken == accessToken)
        {
            Interlocked.CompareExchange(ref _current, null, current);
        }
    }

    private bool IsFresh(CachedToken token) => timeProvider.GetUtcNow() < token.ExpiresAt - RemintMargin;
}
