namespace TeachMeHipaa;

/// <summary>
/// Supplies the Access token for each request. A client mints its own with
/// client credentials unless <see cref="TeachMeHipaaClientOptions.TokenProvider"/>
/// gives it one of these, such as a <see cref="PartnerGrantTokenProvider"/>
/// or a <see cref="StaticTokenProvider"/>.
/// </summary>
public interface ITokenProvider
{
    /// <summary>The Access token to send. An exception thrown here is raised from the call that needed the token.</summary>
    /// <param name="cancellationToken">Cancels getting the token.</param>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Called after the API answered 401 to <paramref name="accessToken"/>.
    /// Forget it if it is still the cached one, so the next call gets a new
    /// one; leave a token that was already replaced alone, so concurrent 401s
    /// renew once. When the next call returns the same token, the client
    /// raises the 401 rather than sending the request again.
    /// </summary>
    /// <param name="accessToken">The token the API refused.</param>
    void Invalidate(string accessToken);
}
