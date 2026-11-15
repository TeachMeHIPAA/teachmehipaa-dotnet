namespace TeachMeHipaa;

/// <summary>
/// A client for one API client's credentials and one Organization. It is
/// thread-safe and meant to be shared; its configuration is fixed when it is
/// constructed, so create another client to change it.
/// </summary>
public sealed partial class TeachMeHipaaClient
{
    /// <summary>Creates a client from your API client's id and secret.</summary>
    /// <param name="options">The client's settings. They are copied, so later changes have no effect.</param>
    /// <exception cref="ArgumentException">A setting is missing or invalid.</exception>
    public TeachMeHipaaClient(TeachMeHipaaClientOptions options)
        : this(CreateRequestor(options))
    {
    }

    /// <summary>
    /// Verifies Deliveries and signs payloads for your own tests, the same as
    /// <see cref="WebhookUtility"/>. Needs only the Signing secret.
    /// </summary>
    public TeachMeHipaaWebhooks Webhooks { get; } = TeachMeHipaaWebhooks.Instance;

    private static readonly OptionChecks TransportChecks = new(nameof(TeachMeHipaaClientOptions));

    private static ApiRequestor CreateRequestor(TeachMeHipaaClientOptions options)
    {
        var config = ClientConfiguration.From(options);

        var transport = TransportChecks.Transport(options.HttpClient, options.HttpMessageHandler);
        if (config.Logger is not null)
        {
            transport = new LoggingHandler(config.Logger, config.TimeProvider) { InnerHandler = transport };
        }

        transport = new TimeoutHandler(config.Timeout, config.TimeProvider) { InnerHandler = transport };

        // The token endpoint is called through the same transport but not
        // through authentication, which would otherwise recurse into itself.
        var tokenProvider = config.Credentials is { } credentials
            ? new ClientCredentialsTokenProvider(
                config,
                credentials,
                new HttpMessageInvoker(
                    new RetryHandler(RetryPolicy.TokenEndpoint, config.MaxRetries, config.TimeProvider) { InnerHandler = transport },
                    disposeHandler: false))
            : config.TokenProvider!;
        var api = new HttpMessageInvoker(
            new AuthenticationHandler(tokenProvider)
            {
                InnerHandler = new RetryHandler(RetryPolicy.Api, config.MaxRetries, config.TimeProvider) { InnerHandler = transport },
            },
            disposeHandler: false);
        return new ApiRequestor(config, api);
    }
}
