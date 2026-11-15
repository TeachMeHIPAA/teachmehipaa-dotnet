namespace TeachMeHipaa;

/// <summary>A PKCE code verifier and the challenge to put on the consent URL.</summary>
/// <param name="CodeVerifier">Keep this server-side until the code exchange; never send it to the browser.</param>
/// <param name="CodeChallenge">The S256 hash of <paramref name="CodeVerifier"/>, for <see cref="AuthorizeUrlOptions.CodeChallenge"/>.</param>
public sealed record PkcePair(string CodeVerifier, string CodeChallenge)
{
    /// <summary>The challenge method: always <c>S256</c>, the only one TeachMeHIPAA accepts.</summary>
    public string Method { get; } = "S256";
}
