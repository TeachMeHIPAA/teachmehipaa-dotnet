namespace TeachMeHipaa;

/// <summary>What a Grant's token exchange returns.</summary>
public sealed class GrantTokens
{
    /// <summary>The Access token for the Grant's Organization.</summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>Replaces the Refresh token just exchanged. Save it in your token store before anything else.</summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>How long the Access token lasts from when it was requested.</summary>
    public TimeSpan ExpiresIn { get; init; }

    /// <summary>The Scopes the Access token carries.</summary>
    public IReadOnlyList<string> Scopes { get; init; } = [];

    /// <summary>Describes the tokens without either secret, so logging one leaks nothing.</summary>
    public override string ToString() =>
        $"GrantTokens {{ ExpiresIn = {ExpiresIn}, Scopes = [{string.Join(", ", Scopes)}] }}";
}
