using TeachMeHipaa;

// In this namespace, as Microsoft's own extensions are, so AddTeachMeHipaa
// shows up on builder.Services without another using.
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers a <see cref="TeachMeHipaaClient"/> with dependency injection.</summary>
public static class TeachMeHipaaServiceCollectionExtensions
{
    /// <summary>
    /// The name of the <see cref="IHttpClientFactory"/> client that sends a
    /// registered <see cref="TeachMeHipaaClient"/>'s requests.
    /// </summary>
    public const string HttpClientName = nameof(TeachMeHipaaClient);

    /// <summary>
    /// Registers one shared <see cref="TeachMeHipaaClient"/> that sends its
    /// requests through a named <see cref="IHttpClientFactory"/> client, so
    /// handler rotation, proxies and your own <see cref="DelegatingHandler"/>s
    /// apply to it.
    /// </summary>
    /// <remarks>
    /// The client is a singleton because it caches the Access token and the
    /// Organization; one per scope would fetch both again for every request.
    /// It asks the factory for an <see cref="HttpClient"/> on every request,
    /// so it never holds on to a handler the factory has rotated out. Its own
    /// <see cref="TeachMeHipaaClientOptions.Timeout"/> applies to each attempt,
    /// so the factory client's timeout is turned off.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="createOptions">
    /// Builds the client's options, such as from configuration or a logger in
    /// the container. It runs once, when the client is first resolved, and
    /// must leave <see cref="TeachMeHipaaClientOptions.HttpClient"/> and
    /// <see cref="TeachMeHipaaClientOptions.HttpMessageHandler"/> unset.
    /// </param>
    /// <returns>The factory client's builder, to add handlers to or configure its primary handler.</returns>
    /// <exception cref="ArgumentException">
    /// When the client is resolved: an option is invalid, or the options bring their own transport.
    /// </exception>
    public static IHttpClientBuilder AddTeachMeHipaa(
        this IServiceCollection services,
        Func<IServiceProvider, TeachMeHipaaClientOptions> createOptions)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (createOptions is null)
        {
            throw new ArgumentNullException(nameof(createOptions));
        }

        services.AddSingleton(provider =>
        {
            var options = createOptions(provider)
                ?? throw new InvalidOperationException("AddTeachMeHipaa: the options factory returned null.");
            RejectTransport(options.HttpClient, nameof(TeachMeHipaaClientOptions.HttpClient));
            RejectTransport(options.HttpMessageHandler, nameof(TeachMeHipaaClientOptions.HttpMessageHandler));
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return new TeachMeHipaaClient(options, new HttpClientSendingHandler(() => factory.CreateClient(HttpClientName)));
        });
        return services.AddHttpClient(HttpClientName, httpClient => httpClient.Timeout = Timeout.InfiniteTimeSpan);
    }

    private static void RejectTransport(object? transport, string option)
    {
        if (transport is not null)
        {
            throw new ArgumentException(
                $"{nameof(TeachMeHipaaClientOptions)}.{option} must not be set with AddTeachMeHipaa: requests go through "
                    + $"the IHttpClientFactory client named \"{HttpClientName}\". Configure it with the IHttpClientBuilder "
                    + "AddTeachMeHipaa returns.",
                option);
        }
    }
}
