namespace TeachMeHipaa;

/// <summary>
/// A client for one API client's credentials and one Organization. It is
/// thread-safe and meant to be shared; its configuration is fixed when it is
/// constructed, so create another client to change it.
/// </summary>
public sealed class TeachMeHipaaClient
{
    /// <summary>Creates a client from your API client's id and secret.</summary>
    /// <param name="options">The client's settings. They are copied, so later changes have no effect.</param>
    /// <exception cref="ArgumentException">A setting is missing or invalid.</exception>
    public TeachMeHipaaClient(TeachMeHipaaClientOptions options)
    {
        var config = ClientConfiguration.From(options);

        HttpMessageHandler transport = options.HttpMessageHandler
            ?? (options.HttpClient is null ? SharedHttpHandler.Instance : new HttpClientSendingHandler(options.HttpClient));
        if (config.Logger is not null)
        {
            transport = new LoggingHandler(config.Logger, config.TimeProvider) { InnerHandler = transport };
        }

        transport = new TimeoutHandler(config.Timeout, config.TimeProvider) { InnerHandler = transport };

        // The token endpoint is called through the same transport but not
        // through authentication, which would otherwise recurse into itself.
        var tokenProvider = new ClientCredentialsTokenProvider(
            config,
            new HttpMessageInvoker(
                new RetryHandler(RetryPolicy.TokenEndpoint, config.MaxRetries, config.TimeProvider) { InnerHandler = transport },
                disposeHandler: false));
        var api = new HttpMessageInvoker(
            new AuthenticationHandler(tokenProvider)
            {
                InnerHandler = new RetryHandler(RetryPolicy.Api, config.MaxRetries, config.TimeProvider) { InnerHandler = transport },
            },
            disposeHandler: false);
        var requestor = new ApiRequestor(config, api);

        Learners = new LearnerService(requestor);
        WebhookSubscriptions = new WebhookSubscriptionService(requestor);
    }

    /// <summary>Learners: Members taking a Training, seen from the Organization's side.</summary>
    public LearnerService Learners { get; }

    /// <summary>Webhook subscriptions: where and for which Events the Organization's Deliveries are sent.</summary>
    public WebhookSubscriptionService WebhookSubscriptions { get; }
}
