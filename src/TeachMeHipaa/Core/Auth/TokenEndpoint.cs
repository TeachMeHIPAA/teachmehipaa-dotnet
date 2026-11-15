namespace TeachMeHipaa;

/// <summary>
/// Where a Grant's exchanges go, and the pipeline that sends them: a timeout
/// and nothing else. A retry could replay a spent code or a superseded
/// Refresh token, which revokes the Grant (ADR-0004).
/// </summary>
internal sealed class TokenEndpoint
{
    public TokenEndpoint(string baseUrl, string userAgent, TimeSpan timeout, TimeProvider timeProvider, HttpMessageHandler transport)
    {
        BaseUrl = baseUrl;
        Uri = UriFor(baseUrl);
        UserAgent = userAgent;
        TimeProvider = timeProvider;
        Invoker = new HttpMessageInvoker(
            new TimeoutHandler(timeout, timeProvider) { InnerHandler = transport },
            disposeHandler: false);
    }

    /// <summary>The API base URL, without a trailing slash.</summary>
    public string BaseUrl { get; }

    public Uri Uri { get; }

    public string UserAgent { get; }

    public TimeProvider TimeProvider { get; }

    public HttpMessageInvoker Invoker { get; }

    /// <summary>The token endpoint under <paramref name="baseUrl"/>, a base URL without a trailing slash.</summary>
    public static Uri UriFor(string baseUrl) => new($"{baseUrl}/auth/token");
}
