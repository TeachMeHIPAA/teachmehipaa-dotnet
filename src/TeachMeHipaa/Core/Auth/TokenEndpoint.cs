namespace TeachMeHipaa;

internal static class TokenEndpoint
{
    /// <summary>The token endpoint under <paramref name="baseUrl"/>, a base URL without a trailing slash.</summary>
    public static Uri UriFor(string baseUrl) => new($"{baseUrl}/auth/token");
}
