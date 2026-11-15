namespace TeachMeHipaa;

/// <summary>
/// Supplies the Access token for each request. Client credentials is the
/// default adapter; partner Grants and static tokens plug in here too.
/// </summary>
internal interface ITokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);
}
