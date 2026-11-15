namespace TeachMeHipaa;

/// <summary>
/// Supplies the Access token for each request. Client credentials is the
/// default adapter; partner Grants and static tokens plug in here too.
/// </summary>
internal interface ITokenProvider
{
    /// <summary>The Access token to send; throws the mint's error, which the caller's call then raises.</summary>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Called after the API answered 401 to <paramref name="accessToken"/>.
    /// Forgets it if it is still the cached one, so the next call mints afresh;
    /// a token that was already replaced is left alone, so concurrent 401s mint once.
    /// </summary>
    void Invalidate(string accessToken);
}
