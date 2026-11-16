namespace TeachMeHipaa;

/// <summary>
/// Sends an Access token you obtained yourself, as is. It is never renewed:
/// once it expires or is revoked, calls raise <see cref="AuthenticationException"/>.
/// </summary>
public sealed class StaticTokenProvider : ITokenProvider
{
    private readonly string _accessToken;

    /// <summary>Creates a provider that always sends <paramref name="accessToken"/>.</summary>
    /// <param name="accessToken">The Access token to send.</param>
    /// <exception cref="ArgumentException"><paramref name="accessToken"/> is empty.</exception>
    public StaticTokenProvider(string accessToken)
    {
        _accessToken = string.IsNullOrWhiteSpace(accessToken)
            ? throw new ArgumentException("The Access token must be a non-empty string.", nameof(accessToken))
            : accessToken;
    }

    /// <inheritdoc />
    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken) => Task.FromResult(_accessToken);

    /// <summary>Does nothing: this provider can't get another token.</summary>
    /// <param name="accessToken">The token the API refused.</param>
    public void Invalidate(string accessToken)
    {
    }
}
