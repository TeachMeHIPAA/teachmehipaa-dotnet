using Microsoft.Extensions.Logging;

namespace TeachMeHipaa;

/// <summary>
/// Settings for a <see cref="TeachMeHipaaClient"/>. The client copies them when
/// it is constructed, so later changes have no effect on it.
/// </summary>
public sealed class TeachMeHipaaClientOptions
{
    /// <summary>Your API client's id (<c>cli_…</c>). Required unless <see cref="TokenProvider"/> is given.</summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>Your API client's secret (<c>tmh_cs_…</c>). Required unless <see cref="TokenProvider"/> is given.</summary>
    public string ClientSecret { get; init; } = string.Empty;

    /// <summary>
    /// Supplies every Access token instead of <see cref="ClientId"/> and
    /// <see cref="ClientSecret"/>, such as a <see cref="StaticTokenProvider"/>.
    /// Give this or the credentials and <see cref="Scopes"/>, not both.
    /// </summary>
    public ITokenProvider? TokenProvider { get; init; }

    /// <summary>
    /// Which deployment your credentials belong to. Defaults to
    /// <see cref="TeachMeHipaaEnvironment.Live"/>; Sandbox credentials never
    /// work against Live.
    /// </summary>
    public TeachMeHipaaEnvironment Environment { get; init; } = TeachMeHipaaEnvironment.Live;

    /// <summary>
    /// Overrides the Environment's base URL, such as <c>http://localhost:4010/v1</c>
    /// for a mock server.
    /// </summary>
    public Uri? BaseUrl { get; init; }

    /// <summary>
    /// The Organization to act for. When omitted, the client looks it up once
    /// from the Access token.
    /// </summary>
    public string? OrganizationId { get; init; }

    /// <summary>
    /// The Scopes to request, such as <c>learners:write</c>. Defaults to every
    /// Scope the API client holds.
    /// </summary>
    public IReadOnlyList<string>? Scopes { get; init; }

    /// <summary>Retries for a failed request that is safe to replay. Defaults to 2.</summary>
    public int MaxRetries { get; init; } = 2;

    /// <summary>How long to wait for each response. Defaults to 80 seconds.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(80);

    /// <summary>
    /// An <see cref="System.Net.Http.HttpClient"/> to send requests with, such as
    /// one from <c>IHttpClientFactory</c>. Give this or
    /// <see cref="HttpMessageHandler"/>, not both. The client never disposes it.
    /// </summary>
    public HttpClient? HttpClient { get; init; }

    /// <summary>
    /// The innermost handler that sends requests, such as one with a proxy or a
    /// client certificate, or a fake in tests. Give this or
    /// <see cref="HttpClient"/>, not both. The client never disposes it.
    /// </summary>
    public HttpMessageHandler? HttpMessageHandler { get; init; }

    /// <summary>
    /// Opt-in request logging at debug level: method, path, status, latency
    /// and request id, or the error kind for a connection error or timeout.
    /// Bodies and credentials are never logged.
    /// </summary>
    public ILogger? Logger { get; init; }

    /// <summary>Your software's name, version and URL, sent in the User-Agent.</summary>
    public AppInfo? AppInfo { get; init; }

    /// <summary>The source of time for token expiry and timeouts. Defaults to <see cref="System.TimeProvider.System"/>.</summary>
    public TimeProvider? TimeProvider { get; init; }
}
