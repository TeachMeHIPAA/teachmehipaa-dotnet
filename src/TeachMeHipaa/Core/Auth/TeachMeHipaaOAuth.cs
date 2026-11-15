using System.Security.Cryptography;
using System.Text;

namespace TeachMeHipaa;

/// <summary>
/// Connects Organizations to a Partner app: PKCE pairs, the consent URL and
/// the code exchange. None of it needs a client.
/// </summary>
public static class TeachMeHipaaOAuth
{
    private const string LiveAuthorizationUrl = "https://console.teachmehipaa.com/oauth/authorize";

    private static readonly OptionChecks ConsentChecks = new(nameof(AuthorizeUrlOptions));
    private static readonly OptionChecks ExchangeChecks = new(nameof(ExchangeCodeOptions));

    private static readonly ExchangeGuidance CodeExchangeGuidance = new(
        "The code exchange was not retried: the code may have been consumed, and a code works only once. "
        + "Re-run consent to get a new code.");

    /// <summary>A fresh code verifier and its S256 code challenge.</summary>
    public static PkcePair CreatePkcePair()
    {
        var codeVerifier = RandomToken();
#if NET5_0_OR_GREATER
        var digest = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
#else
        using var sha256 = SHA256.Create();
        var digest = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
#endif
        return new PkcePair(codeVerifier, Base64Url(digest));
    }

    /// <summary>A random <c>state</c> that binds the consent redirect to the admin's session.</summary>
    public static string CreateState() => RandomToken();

    /// <summary>The URL to send an Organization admin to, to grant your Partner app access.</summary>
    /// <param name="options">What the consent URL carries.</param>
    /// <exception cref="ArgumentException">A setting is missing or invalid.</exception>
    public static string AuthorizeUrl(AuthorizeUrlOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var consentUrl = options.AuthorizationUrl is not null
            ? ConsentChecks.RequireAbsoluteUri(options.AuthorizationUrl, nameof(options.AuthorizationUrl)).GetLeftPart(UriPartial.Path)
            : options.Environment switch
            {
                TeachMeHipaaEnvironment.Live => LiveAuthorizationUrl,
                // The Sandbox console's consent URL is not published yet, so
                // the caller names it rather than the library guessing a host.
                TeachMeHipaaEnvironment.Sandbox => throw ConsentChecks.Invalid(
                    "must be given for the Sandbox Environment, which has no built-in consent URL",
                    nameof(options.AuthorizationUrl)),
                _ => throw ConsentChecks.Invalid("must be Live or Sandbox", nameof(options.Environment)),
            };
        var parameters = new[]
        {
            ("response_type", "code"),
            ("client_id", ConsentChecks.RequireText(options.ClientId, nameof(options.ClientId))),
            ("redirect_uri", ConsentChecks.RequireAbsoluteUri(options.RedirectUri, nameof(options.RedirectUri)).OriginalString),
            ("scope", string.Join(" ", ConsentChecks.ResolveScopes(options.Scopes))),
            // Without state, a forged redirect could attach an attacker's
            // Grant to the partner's session (RFC 6749 §10.12).
            ("state", ConsentChecks.RequireText(options.State, nameof(options.State))),
            ("code_challenge", ConsentChecks.RequireText(options.CodeChallenge, nameof(options.CodeChallenge))),
            ("code_challenge_method", "S256"),
        };
        // EscapeDataString writes a space as %20; form encoding's + is read
        // back as a space only by form parsers.
        return $"{consentUrl}?{string.Join("&", parameters.Select(p => $"{p.Item1}={Uri.EscapeDataString(p.Item2)}"))}";
    }

    /// <summary>
    /// Exchanges the redirect's code for the Grant's first tokens, exactly
    /// once. Save the Refresh token in your token store before you use the
    /// Access token.
    /// </summary>
    /// <param name="options">The Partner app's credentials and the code to exchange.</param>
    /// <param name="httpClient">Sends the request; by default the library's shared connection pool does.</param>
    /// <param name="cancellationToken">Cancels the exchange.</param>
    /// <exception cref="ArgumentException">A setting is missing or invalid.</exception>
    /// <exception cref="OAuthException">The token endpoint refused the code.</exception>
    /// <exception cref="ApiConnectionException">
    /// No response arrived. The code may have been consumed, so re-run consent
    /// rather than retrying.
    /// </exception>
    public static async Task<GrantTokens> ExchangeCodeAsync(
        ExchangeCodeOptions options,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var basicCredentials = BasicCredentials.Encode(
            ExchangeChecks.RequireText(options.ClientId, nameof(options.ClientId)),
            ExchangeChecks.RequireText(options.ClientSecret, nameof(options.ClientSecret)));
        var form = new KeyValuePair<string, string>[]
        {
            new("grant_type", "authorization_code"),
            new("code", ExchangeChecks.RequireText(options.Code, nameof(options.Code))),
            new("redirect_uri", ExchangeChecks.RequireAbsoluteUri(options.RedirectUri, nameof(options.RedirectUri)).OriginalString),
            new("code_verifier", ExchangeChecks.RequireText(options.CodeVerifier, nameof(options.CodeVerifier))),
        };
        var endpoint = new TokenEndpoint(
            ExchangeChecks.ResolveBaseUrl(options.Environment, options.BaseUrl),
            ExchangeChecks.UserAgent(options.AppInfo),
            ExchangeChecks.RequirePositive(options.Timeout, nameof(options.Timeout)),
            TimeProvider.System,
            ExchangeChecks.Transport(httpClient, httpMessageHandler: null));
        var grant = await GrantExchange.SendAsync(endpoint, basicCredentials, form, [], CodeExchangeGuidance, cancellationToken).ConfigureAwait(false);
        return grant.Tokens;
    }

    // 32 bytes encode to 43 base64url characters: the shortest code verifier
    // RFC 7636 allows, carrying the full 256 bits it recommends.
    private static string RandomToken()
    {
#if NET5_0_OR_GREATER
        return Base64Url(RandomNumberGenerator.GetBytes(32));
#else
        var bytes = new byte[32];
        using var random = RandomNumberGenerator.Create();
        random.GetBytes(bytes);
        return Base64Url(bytes);
#endif
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
