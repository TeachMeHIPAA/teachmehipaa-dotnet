namespace TeachMeHipaa;

/// <summary>
/// The connection pool every client shares when the caller supplies no
/// transport, so creating many clients doesn't exhaust sockets.
/// </summary>
internal static class SharedHttpHandler
{
#if NET5_0_OR_GREATER
    // Recycling pooled connections picks up DNS changes behind the CDN.
    public static readonly HttpMessageHandler Instance =
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) };
#else
    public static readonly HttpMessageHandler Instance = new HttpClientHandler();
#endif
}
