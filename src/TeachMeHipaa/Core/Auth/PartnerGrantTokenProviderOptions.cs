namespace TeachMeHipaa;

/// <summary>Settings for a <see cref="PartnerGrantTokenProvider"/>, copied when it is constructed.</summary>
public sealed class PartnerGrantTokenProviderOptions
{
    /// <summary>Your Partner app's id (<c>app_…</c>). Required.</summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>Your Partner app's secret (<c>tmh_as_…</c>). Required.</summary>
    public string ClientSecret { get; init; } = string.Empty;

    /// <summary>Receives every rotated Refresh token before its Access token is used. Required.</summary>
    public ITokenStore? TokenStore { get; init; }

    /// <summary>
    /// The Refresh token for the first exchange, such as the one
    /// <see cref="TeachMeHipaaOAuth.ExchangeCodeAsync"/> returned. When
    /// omitted, the first exchange loads it from <see cref="TokenStore"/>.
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>Narrows the Access token to these Scopes; it can never widen them.</summary>
    public IReadOnlyList<string>? Scopes { get; init; }

    /// <summary>
    /// Which deployment the Partner app belongs to. Defaults to Live. A client
    /// given this provider must use the same Environment and base URL.
    /// </summary>
    public TeachMeHipaaEnvironment Environment { get; init; } = TeachMeHipaaEnvironment.Live;

    /// <summary>Overrides the Environment's base URL, such as a mock server's.</summary>
    public Uri? BaseUrl { get; init; }

    /// <summary>How long to wait for each exchange. Defaults to 80 seconds.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(80);

    /// <summary>Your integration's name, version and URL, sent in the User-Agent.</summary>
    public AppInfo? AppInfo { get; init; }

    /// <summary>
    /// An <see cref="System.Net.Http.HttpClient"/> to send exchanges with. Give
    /// this or <see cref="HttpMessageHandler"/>, not both. Never disposed.
    /// </summary>
    public HttpClient? HttpClient { get; init; }

    /// <summary>
    /// The innermost handler that sends exchanges. Give this or
    /// <see cref="HttpClient"/>, not both. Never disposed.
    /// </summary>
    public HttpMessageHandler? HttpMessageHandler { get; init; }

    /// <summary>The source of time for token expiry and timeouts. Defaults to <see cref="System.TimeProvider.System"/>.</summary>
    public TimeProvider? TimeProvider { get; init; }
}
