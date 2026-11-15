using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace TeachMeHipaa;

/// <summary>
/// Mints Access tokens with the OAuth client-credentials grant, caches each one
/// until 60 s before it expires, and lets only one mint run at a time.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "SemaphoreSlim without AvailableWaitHandle holds no unmanaged resources.")]
internal sealed class ClientCredentialsTokenProvider : ITokenProvider
{
    private static readonly TimeSpan RemintMargin = TimeSpan.FromSeconds(60);

    private readonly ClientConfiguration _config;
    private readonly HttpMessageInvoker _transport;
    private readonly Uri _tokenUri;
    private readonly string _basicCredentials;
    private readonly SemaphoreSlim _mintLock = new(1, 1);
    private volatile CachedToken? _current;

    public ClientCredentialsTokenProvider(ClientConfiguration config, HttpMessageInvoker transport)
    {
        _config = config;
        _transport = transport;
        _tokenUri = new Uri($"{config.BaseUrl}/auth/token");
        // RFC 6749 §2.3.1: Basic credentials are form-encoded before base64.
        _basicCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{WebUtility.UrlEncode(config.ClientId)}:{WebUtility.UrlEncode(config.ClientSecret)}"));
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
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
                current = await MintAsync(cancellationToken).ConfigureAwait(false);
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

    private bool IsFresh(CachedToken token) =>
        _config.TimeProvider.GetUtcNow() < token.ExpiresAt - RemintMargin;

    private async Task<CachedToken> MintAsync(CancellationToken cancellationToken)
    {
        var form = new List<KeyValuePair<string, string>> { new("grant_type", "client_credentials") };
        // Omitting scope grants every Scope the API client holds.
        if (_config.Scopes.Count > 0)
        {
            form.Add(new("scope", string.Join(" ", _config.Scopes)));
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, _tokenUri)
        {
            Content = new FormUrlEncodedContent(form),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _basicCredentials);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("User-Agent", _config.UserAgent);
        RequestProperties.RetrySafe.Set(request, true);

        // Expiry counts from when the request left, so a slow response can't
        // stretch the token past its real lifetime.
        var requestedAt = _config.TimeProvider.GetUtcNow();
        using var response = await _transport.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = await ResponseReader.ReadTextAsync(response, cancellationToken).ConfigureAwait(false);
        var token = response.IsSuccessStatusCode
            ? ResponseReader.Parse(response, text, CoreJsonContext.Default.AccessTokenResponse)
            : null;
        if (token is not { AccessToken.Length: > 0, ExpiresIn: > 0 })
        {
            throw ResponseReader.OAuthError(response, text, _config.TimeProvider.GetUtcNow());
        }

        return new CachedToken(token.AccessToken, requestedAt + TimeSpan.FromSeconds(token.ExpiresIn.Value));
    }

    private sealed record CachedToken(string AccessToken, DateTimeOffset ExpiresAt);
}
