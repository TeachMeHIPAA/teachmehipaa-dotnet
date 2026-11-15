namespace TeachMeHipaa;

/// <summary>
/// Where a Grant's Refresh token lives between processes, such as a database
/// row or a secrets manager. Every exchange replaces the Refresh token, and
/// presenting a replaced one revokes the Grant, so <see cref="SaveAsync"/>
/// must persist durably before it completes.
/// </summary>
public interface ITokenStore
{
    /// <summary>The Grant's current Refresh token, read once, at the first exchange.</summary>
    /// <param name="cancellationToken">Cancels the read.</param>
    ValueTask<string> LoadAsync(CancellationToken cancellationToken);

    /// <summary>Persists the Refresh token that replaces the current one.</summary>
    /// <param name="refreshToken">The new Refresh token.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    ValueTask SaveAsync(string refreshToken, CancellationToken cancellationToken);
}
