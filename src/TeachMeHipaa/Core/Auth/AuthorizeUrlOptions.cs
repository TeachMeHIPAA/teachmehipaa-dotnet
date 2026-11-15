namespace TeachMeHipaa;

/// <summary>What <see cref="TeachMeHipaaOAuth.AuthorizeUrl"/> puts on the consent URL.</summary>
public sealed class AuthorizeUrlOptions
{
    /// <summary>Your Partner app's id (<c>app_…</c>). Required.</summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>One of the app's registered redirect URIs, matched exactly. Required.</summary>
    public Uri? RedirectUri { get; init; }

    /// <summary>The Scopes to ask the Organization admin for.</summary>
    public IReadOnlyList<string> Scopes { get; init; } = [];

    /// <summary>
    /// Echoed back on the redirect; check it matches before exchanging the
    /// code. <see cref="TeachMeHipaaOAuth.CreateState"/> makes one. Required.
    /// </summary>
    public string State { get; init; } = string.Empty;

    /// <summary>From <see cref="TeachMeHipaaOAuth.CreatePkcePair"/>. Required.</summary>
    public string CodeChallenge { get; init; } = string.Empty;

    /// <summary>
    /// Which deployment's console to send the admin to. Defaults to Live;
    /// Sandbox has no built-in consent URL, so it needs <see cref="AuthorizationUrl"/>.
    /// </summary>
    public TeachMeHipaaEnvironment Environment { get; init; } = TeachMeHipaaEnvironment.Live;

    /// <summary>Overrides the Environment's consent URL.</summary>
    public Uri? AuthorizationUrl { get; init; }
}
