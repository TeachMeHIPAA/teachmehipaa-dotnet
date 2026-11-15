namespace TeachMeHipaa;

/// <summary>What <see cref="TeachMeHipaaOAuth.ExchangeCodeAsync"/> sends to the token endpoint.</summary>
public sealed class ExchangeCodeOptions
{
    /// <summary>Your Partner app's id (<c>app_…</c>). Required.</summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>Your Partner app's secret (<c>tmh_as_…</c>). Required.</summary>
    public string ClientSecret { get; init; } = string.Empty;

    /// <summary>The <c>code</c> from the redirect. It works once, within 10 minutes. Required.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>The same redirect URI the consent URL carried. Required.</summary>
    public Uri? RedirectUri { get; init; }

    /// <summary>The verifier from the <see cref="PkcePair"/> behind the consent URL. Required.</summary>
    public string CodeVerifier { get; init; } = string.Empty;

    /// <summary>Which deployment the Partner app belongs to. Defaults to Live.</summary>
    public TeachMeHipaaEnvironment Environment { get; init; } = TeachMeHipaaEnvironment.Live;

    /// <summary>Overrides the Environment's base URL, such as a mock server's.</summary>
    public Uri? BaseUrl { get; init; }

    /// <summary>How long to wait for the response. Defaults to 80 seconds.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(80);

    /// <summary>Your software's name, version and URL, sent in the User-Agent.</summary>
    public AppInfo? AppInfo { get; init; }
}
