namespace TeachMeHipaa;

/// <summary>
/// The <see cref="ITokenStore"/> failed or returned no Refresh token. The
/// <see cref="Exception.InnerException"/> is the store's own error.
/// </summary>
public class TokenStoreException : TeachMeHipaaException
{
    /// <summary>Creates an error with a default message.</summary>
    public TokenStoreException()
    {
    }

    /// <summary>Creates an error with a message.</summary>
    public TokenStoreException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an error with a message and the error that caused it.</summary>
    public TokenStoreException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
