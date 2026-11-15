using Microsoft.Extensions.Logging;

namespace TeachMeHipaa;

/// <summary>The validated, copied settings a client runs with.</summary>
internal sealed record ClientConfiguration(
    string ClientId,
    string ClientSecret,
    string BaseUrl,
    string? OrganizationId,
    IReadOnlyList<string> Scopes,
    int MaxRetries,
    TimeSpan Timeout,
    string UserAgent,
    TimeProvider TimeProvider,
    ILogger? Logger)
{
    internal const string LiveBaseUrl = "https://api.tme.so/v1";
    internal const string SandboxBaseUrl = "https://api-teachmehipaa-dev.web.app/v1";

    public static ClientConfiguration From(TeachMeHipaaClientOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.HttpClient is not null && options.HttpMessageHandler is not null)
        {
            throw Invalid("give HttpClient or HttpMessageHandler, not both", nameof(options.HttpClient));
        }

        return new ClientConfiguration(
            ClientId: RequireText(options.ClientId, nameof(options.ClientId)),
            ClientSecret: RequireText(options.ClientSecret, nameof(options.ClientSecret)),
            BaseUrl: ResolveBaseUrl(options.Environment, options.BaseUrl),
            OrganizationId: options.OrganizationId is null
                ? null
                : RequireText(options.OrganizationId, nameof(options.OrganizationId)),
            Scopes: ResolveScopes(options.Scopes),
            MaxRetries: options.MaxRetries >= 0
                ? options.MaxRetries
                : throw Invalid("must not be negative", nameof(options.MaxRetries)),
            Timeout: options.Timeout > TimeSpan.Zero
                ? options.Timeout
                : throw Invalid("must be positive", nameof(options.Timeout)),
            UserAgent: UserAgentHeader.Build(ResolveAppInfo(options.AppInfo)),
            TimeProvider: options.TimeProvider ?? TimeProvider.System,
            Logger: options.Logger);
    }

    private static ArgumentException Invalid(string problem, string option) =>
        new($"TeachMeHipaaClientOptions.{option} {problem}.", option);

    private static string RequireText(string? value, string option) =>
        string.IsNullOrWhiteSpace(value) ? throw Invalid("must be a non-empty string", option) : value!;

    private static string ResolveBaseUrl(TeachMeHipaaEnvironment environment, Uri? baseUrl)
    {
        if (baseUrl is not null)
        {
            return baseUrl.IsAbsoluteUri && (baseUrl.Scheme == Uri.UriSchemeHttps || baseUrl.Scheme == Uri.UriSchemeHttp)
                ? baseUrl.AbsoluteUri.TrimEnd('/')
                : throw Invalid("must be an absolute http or https URL", nameof(TeachMeHipaaClientOptions.BaseUrl));
        }

        return environment switch
        {
            TeachMeHipaaEnvironment.Live => LiveBaseUrl,
            TeachMeHipaaEnvironment.Sandbox => SandboxBaseUrl,
            _ => throw Invalid("must be Live or Sandbox", nameof(TeachMeHipaaClientOptions.Environment)),
        };
    }

    private static string[] ResolveScopes(IReadOnlyList<string>? scopes)
    {
        if (scopes is null)
        {
            return [];
        }

        var copy = scopes.ToArray();
        return copy.Any(string.IsNullOrWhiteSpace)
            ? throw Invalid("must not contain empty Scopes", nameof(TeachMeHipaaClientOptions.Scopes))
            : copy;
    }

    private static AppInfo? ResolveAppInfo(AppInfo? appInfo) =>
        appInfo is not null && string.IsNullOrWhiteSpace(appInfo.Name)
            ? throw Invalid("Name must be a non-empty string", nameof(TeachMeHipaaClientOptions.AppInfo))
            : appInfo;
}
