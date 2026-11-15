namespace TeachMeHipaa;

/// <summary>
/// Validates one options class's settings, naming the class and setting in
/// each <see cref="ArgumentException"/> so a caller sees which one to fix.
/// </summary>
internal sealed class OptionChecks(string owner)
{
    internal const string LiveBaseUrl = "https://api.tme.so/v1";
    internal const string SandboxBaseUrl = "https://api-teachmehipaa-dev.web.app/v1";

    public ArgumentException Invalid(string problem, string option) =>
        new($"{owner}.{option} {problem}.", option);

    public string RequireText(string? value, string option) =>
        string.IsNullOrWhiteSpace(value) ? throw Invalid("must be a non-empty string", option) : value!;

    public Uri RequireAbsoluteUri(Uri? value, string option) =>
        value is { IsAbsoluteUri: true } ? value : throw Invalid("must be an absolute URL", option);

    public TimeSpan RequirePositive(TimeSpan value, string option) =>
        value > TimeSpan.Zero ? value : throw Invalid("must be positive", option);

    /// <summary>The base URL without a trailing slash: the explicit one, or the Environment's.</summary>
    public string ResolveBaseUrl(TeachMeHipaaEnvironment environment, Uri? baseUrl)
    {
        if (baseUrl is not null)
        {
            return baseUrl.IsAbsoluteUri && (baseUrl.Scheme == Uri.UriSchemeHttps || baseUrl.Scheme == Uri.UriSchemeHttp)
                ? baseUrl.AbsoluteUri.TrimEnd('/')
                : throw Invalid("must be an absolute http or https URL", "BaseUrl");
        }

        return environment switch
        {
            TeachMeHipaaEnvironment.Live => LiveBaseUrl,
            TeachMeHipaaEnvironment.Sandbox => SandboxBaseUrl,
            _ => throw Invalid("must be Live or Sandbox", "Environment"),
        };
    }

    public string[] ResolveScopes(IReadOnlyList<string>? scopes)
    {
        if (scopes is null)
        {
            return [];
        }

        var copy = scopes.ToArray();
        return copy.Any(string.IsNullOrWhiteSpace)
            ? throw Invalid("must not contain empty Scopes", "Scopes")
            : copy;
    }

    public string UserAgent(AppInfo? appInfo) =>
        UserAgentHeader.Build(appInfo is not null && string.IsNullOrWhiteSpace(appInfo.Name)
            ? throw Invalid("Name must be a non-empty string", "AppInfo")
            : appInfo);

    /// <summary>The handler that ends a pipeline: the caller's, their HttpClient, or the shared pool.</summary>
    public HttpMessageHandler Transport(HttpClient? httpClient, HttpMessageHandler? httpMessageHandler)
    {
        if (httpClient is not null && httpMessageHandler is not null)
        {
            throw Invalid("give HttpClient or HttpMessageHandler, not both", "HttpClient");
        }

        return httpMessageHandler
            ?? (httpClient is null ? SharedHttpHandler.Instance : new HttpClientSendingHandler(httpClient));
    }
}
