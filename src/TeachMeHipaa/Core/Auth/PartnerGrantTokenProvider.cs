namespace TeachMeHipaa;

/// <summary>
/// Access tokens for one Grant, from its Refresh token. Create one per Grant
/// and share it between clients: it runs one exchange at a time, which keeps
/// two exchanges of the same Refresh token from racing. Each rotated Refresh
/// token goes to the <see cref="ITokenStore"/> before its Access token is
/// used, and an exchange is never retried (ADR-0004).
/// </summary>
public sealed class PartnerGrantTokenProvider : ITokenProvider, ITokenEndpointBound
{
    private static readonly OptionChecks Checks = new(nameof(PartnerGrantTokenProviderOptions));

    private static readonly ExchangeGuidance RefreshGuidance = new(
        NetworkFailure: "The Refresh token exchange was not retried, because replaying a Refresh token the endpoint "
            + "may already have rotated revokes the Grant. If the next call fails with invalid_grant, re-run consent "
            + "for this Organization.",
        InvalidGrant: "The Grant was revoked, or its Refresh token was reused (presenting a superseded Refresh token "
            + "revokes the Grant). Re-run consent for the Organization.");

    private readonly TokenEndpoint _endpoint;
    private readonly string _basicCredentials;
    private readonly ITokenStore _tokenStore;
    private readonly string[] _scopes;
    private readonly AccessTokenCache _cache;

    // Read and written only by a mint, which the cache runs one at a time.
    private string? _latestRefreshToken;

    /// <summary>Creates a provider for one Grant.</summary>
    /// <param name="options">The provider's settings. They are copied, so later changes have no effect.</param>
    /// <exception cref="ArgumentException">A setting is missing or invalid.</exception>
    public PartnerGrantTokenProvider(PartnerGrantTokenProviderOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _basicCredentials = BasicCredentials.Encode(
            Checks.RequireText(options.ClientId, nameof(options.ClientId)),
            Checks.RequireText(options.ClientSecret, nameof(options.ClientSecret)));
        _tokenStore = options.TokenStore ?? throw Checks.Invalid("must be set", nameof(options.TokenStore));
        _latestRefreshToken = options.RefreshToken is null
            ? null
            : Checks.RequireText(options.RefreshToken, nameof(options.RefreshToken));
        _scopes = Checks.ResolveScopes(options.Scopes);
        var timeProvider = options.TimeProvider ?? TimeProvider.System;
        _endpoint = new TokenEndpoint(
            Checks.ResolveBaseUrl(options.Environment, options.BaseUrl),
            Checks.UserAgent(options.AppInfo),
            Checks.RequirePositive(options.Timeout, nameof(options.Timeout)),
            timeProvider,
            Checks.Transport(options.HttpClient, options.HttpMessageHandler));
        BaseUrl = new Uri(_endpoint.BaseUrl);
        _cache = new AccessTokenCache(MintAsync, timeProvider);
    }

    /// <summary>Where this provider exchanges Refresh tokens. A client given this provider must call the same base URL.</summary>
    public Uri BaseUrl { get; }

    /// <inheritdoc />
    /// <exception cref="OAuthException">The token endpoint refused the Refresh token; for <c>invalid_grant</c>, re-run consent.</exception>
    /// <exception cref="ApiConnectionException">No response arrived, and the exchange was not retried.</exception>
    /// <exception cref="TokenStoreException">The token store failed, so the new Access token was not used.</exception>
    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken) => _cache.GetAsync(cancellationToken);

    /// <inheritdoc />
    public void Invalidate(string accessToken) => _cache.Invalidate(accessToken);

    private async Task<CachedToken> MintAsync(CancellationToken cancellationToken)
    {
        var form = new KeyValuePair<string, string>[]
        {
            new("grant_type", "refresh_token"),
            new("refresh_token", _latestRefreshToken ?? await LoadAsync(cancellationToken).ConfigureAwait(false)),
        };
        var grant = await GrantExchange.SendAsync(_endpoint, _basicCredentials, form, _scopes, RefreshGuidance, cancellationToken).ConfigureAwait(false);
        // The token just presented is superseded whatever happens next, so
        // this process must never present it again, even if the save fails.
        _latestRefreshToken = grant.Tokens.RefreshToken;
        try
        {
            await _tokenStore.SaveAsync(grant.Tokens.RefreshToken, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            // The store didn't record the rotation, so the next process would
            // load and replay the superseded token and revoke the Grant. Using
            // the new Access token would hide that until too late.
            throw new TokenStoreException(
                "ITokenStore.SaveAsync failed, so the new Access token was not used. Until a save succeeds, your "
                    + "store holds a superseded Refresh token, and exchanging it would revoke the Grant.",
                exception);
        }

        return new CachedToken(grant.Tokens.AccessToken, grant.ExpiresAt);
    }

    private async Task<string> LoadAsync(CancellationToken cancellationToken)
    {
        var refreshToken = await _tokenStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        return string.IsNullOrEmpty(refreshToken)
            ? throw new TokenStoreException("ITokenStore.LoadAsync returned no Refresh token.")
            : refreshToken;
    }
}
