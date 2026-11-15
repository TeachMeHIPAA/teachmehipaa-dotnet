using Microsoft.Extensions.Logging;

namespace TeachMeHipaa;

/// <summary>
/// The validated, copied settings a client runs with. Exactly one of
/// <see cref="Credentials"/> and <see cref="TokenProvider"/> is set.
/// </summary>
internal sealed record ClientConfiguration(
    ClientCredentials? Credentials,
    ITokenProvider? TokenProvider,
    string BaseUrl,
    string? OrganizationId,
    int MaxRetries,
    TimeSpan Timeout,
    string UserAgent,
    TimeProvider TimeProvider,
    ILogger? Logger)
{
    private static readonly OptionChecks Checks = new(nameof(TeachMeHipaaClientOptions));

    public static ClientConfiguration From(TeachMeHipaaClientOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var baseUrl = Checks.ResolveBaseUrl(options.Environment, options.BaseUrl);
        return new ClientConfiguration(
            Credentials: options.TokenProvider is null
                ? new ClientCredentials(
                    Checks.RequireText(options.ClientId, nameof(options.ClientId)),
                    Checks.RequireText(options.ClientSecret, nameof(options.ClientSecret)),
                    Checks.ResolveScopes(options.Scopes))
                : null,
            TokenProvider: options.TokenProvider is null ? null : CheckTokenProvider(options, baseUrl),
            BaseUrl: baseUrl,
            OrganizationId: options.OrganizationId is null
                ? null
                : Checks.RequireText(options.OrganizationId, nameof(options.OrganizationId)),
            MaxRetries: options.MaxRetries >= 0
                ? options.MaxRetries
                : throw Checks.Invalid("must not be negative", nameof(options.MaxRetries)),
            Timeout: Checks.RequirePositive(options.Timeout, nameof(options.Timeout)),
            UserAgent: Checks.UserAgent(options.AppInfo),
            TimeProvider: options.TimeProvider ?? TimeProvider.System,
            Logger: options.Logger);
    }

    private static ITokenProvider CheckTokenProvider(TeachMeHipaaClientOptions options, string baseUrl)
    {
        var conflicting = new[]
        {
            (Name: nameof(options.ClientId), Given: !string.IsNullOrEmpty(options.ClientId)),
            (Name: nameof(options.ClientSecret), Given: !string.IsNullOrEmpty(options.ClientSecret)),
            (Name: nameof(options.Scopes), Given: options.Scopes is not null),
        }.FirstOrDefault(option => option.Given);
        if (conflicting.Given)
        {
            throw Checks.Invalid(
                $"must not be set with {nameof(options.TokenProvider)}: give a token provider or an API client's "
                    + "credentials, not both (Scopes belong to the token provider)",
                conflicting.Name);
        }

        // A Grant's Refresh token works only in its own Environment, and a
        // provider that refreshed against another one would hand the client
        // Access tokens its API refuses.
        if (options.TokenProvider is PartnerGrantTokenProvider partnerGrant
            && partnerGrant.BaseUrl.AbsoluteUri.TrimEnd('/') != baseUrl)
        {
            throw Checks.Invalid(
                $"exchanges Refresh tokens at {partnerGrant.BaseUrl.AbsoluteUri.TrimEnd('/')}, but the client calls "
                    + $"{baseUrl}; give both the same Environment and BaseUrl",
                nameof(options.TokenProvider));
        }

        return options.TokenProvider!;
    }
}
