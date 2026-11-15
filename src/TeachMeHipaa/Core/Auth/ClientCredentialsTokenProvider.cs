namespace TeachMeHipaa;

/// <summary>An API client's id, secret and the Scopes to request.</summary>
internal sealed record ClientCredentials(string ClientId, string ClientSecret, IReadOnlyList<string> Scopes);

/// <summary>
/// Mints Access tokens with the OAuth client-credentials grant, caching each
/// one until 60 s before it expires and minting one at a time.
/// </summary>
internal sealed class ClientCredentialsTokenProvider : ITokenProvider
{
    private readonly ClientConfiguration _config;
    private readonly ClientCredentials _credentials;
    private readonly HttpMessageInvoker _transport;
    private readonly Uri _tokenUri;
    private readonly string _basicCredentials;
    private readonly AccessTokenCache _cache;

    public ClientCredentialsTokenProvider(ClientConfiguration config, ClientCredentials credentials, HttpMessageInvoker transport)
    {
        _config = config;
        _credentials = credentials;
        _transport = transport;
        _tokenUri = TokenEndpoint.UriFor(config.BaseUrl);
        _basicCredentials = BasicCredentials.Encode(credentials.ClientId, credentials.ClientSecret);
        _cache = new AccessTokenCache(MintAsync, config.TimeProvider);
    }

    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken) => _cache.GetAsync(cancellationToken);

    public void Invalidate(string accessToken) => _cache.Invalidate(accessToken);

    private async Task<CachedToken> MintAsync(CancellationToken cancellationToken)
    {
        using var request = TokenRequest.Create(
            _tokenUri,
            _basicCredentials,
            _config.UserAgent,
            [new("grant_type", "client_credentials")],
            _credentials.Scopes);
        RequestProperties.RetrySafe.Set(request, true);

        // Expiry counts from when the request left, so a slow response can't
        // stretch the token past its real lifetime.
        var requestedAt = _config.TimeProvider.GetUtcNow();
        using var response = await _transport.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = await ResponseReader.ReadTextAsync(response, cancellationToken).ConfigureAwait(false);
        var token = response.IsSuccessStatusCode
            ? ResponseReader.Parse(response, text, CoreJsonContext.Default.TokenEndpointResponse)
            : null;
        if (token is not { AccessToken.Length: > 0, ExpiresIn: > 0 })
        {
            throw ResponseReader.OAuthError(response, text, _config.TimeProvider.GetUtcNow());
        }

        return new CachedToken(token.AccessToken, requestedAt + TimeSpan.FromSeconds(token.ExpiresIn.Value));
    }
}
