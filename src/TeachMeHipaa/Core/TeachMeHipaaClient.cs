namespace TeachMeHipaa;

/// <summary>
/// A client for one API client's credentials and one Organization. It is
/// thread-safe and meant to be shared; its configuration is fixed when it is
/// constructed, so create another client to change it.
/// </summary>
public sealed partial class TeachMeHipaaClient
{
    /// <summary>
    /// Creates a client from your API client's id and secret, or a token
    /// provider. With neither, the client makes only the public calls, such
    /// as Certificate verifications and Courses; any other call throws an
    /// <see cref="InvalidOperationException"/> before sending a request.
    /// </summary>
    /// <param name="options">The client's settings. They are copied, so later changes have no effect.</param>
    /// <exception cref="ArgumentException">A setting is missing or invalid, such as a ClientId without its ClientSecret.</exception>
    public TeachMeHipaaClient(TeachMeHipaaClientOptions options)
        : this(CreateRequestor(options, transport: null))
    {
    }

    /// <summary>A client whose requests end in <paramref name="transport"/>, such as <c>AddTeachMeHipaa</c>'s factory client.</summary>
    internal TeachMeHipaaClient(TeachMeHipaaClientOptions options, HttpMessageHandler transport)
        : this(CreateRequestor(options, transport))
    {
    }

    /// <summary>
    /// Verifies Deliveries and signs payloads for your own tests, the same as
    /// <see cref="WebhookUtility"/>. Needs only the Signing secret.
    /// </summary>
    public TeachMeHipaaWebhooks Webhooks { get; } = TeachMeHipaaWebhooks.Instance;

    private static readonly OptionChecks TransportChecks = new(nameof(TeachMeHipaaClientOptions));

    private static ApiRequestor CreateRequestor(TeachMeHipaaClientOptions options, HttpMessageHandler? transport)
    {
        var config = ClientConfiguration.From(options);

        transport ??= TransportChecks.Transport(options.HttpClient, options.HttpMessageHandler);
        transport = new TimeoutHandler(config.Timeout, config.TimeProvider) { InnerHandler = transport };
        if (config.Logger is not null)
        {
            transport = new LoggingHandler(config.Logger, config.TimeProvider) { InnerHandler = transport };
        }

        var api = new RetryHandler(RetryPolicy.Api, config.MaxRetries, config.TimeProvider) { InnerHandler = transport };
        // The token endpoint is called through the same transport but not
        // through authentication, which would otherwise recurse into itself.
        var tokenProvider = config.Credentials is { } credentials
            ? new ClientCredentialsTokenProvider(
                config,
                credentials,
                new HttpMessageInvoker(
                    new RetryHandler(RetryPolicy.TokenEndpoint, config.MaxRetries, config.TimeProvider) { InnerHandler = transport },
                    disposeHandler: false))
            : config.TokenProvider;
        // Without a token provider the client makes only public calls; the
        // requestor refuses the others before they reach the pipeline.
        HttpMessageHandler pipeline = tokenProvider is null
            ? api
            : new AuthenticationHandler(tokenProvider) { InnerHandler = api };
        return new ApiRequestor(config, new HttpMessageInvoker(pipeline, disposeHandler: false));
    }
}
