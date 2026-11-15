// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Webhook subscriptions endpoints.</summary>
public sealed class WebhookSubscriptionService
{
    private readonly ApiRequestor _requestor;

    internal WebhookSubscriptionService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Create a webhook subscription.</summary>
    /// <remarks>Subscribes an HTTPS endpoint to event types and creates its signing secret. Requires the <c>webhooks:manage</c> or <c>org:admin</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="WebhookSubscriptionCreated"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<WebhookSubscriptionCreated> CreateAsync(
        WebhookSubscriptionCreateOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, "/webhook-subscriptions")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.WebhookSubscriptionCreateOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.WebhookSubscriptionCreated,
            cancellationToken);
    }

    /// <summary>Delete a webhook subscription.</summary>
    /// <remarks>Deletes a webhook subscription and its signing secret. Requires the <c>webhooks:manage</c> or <c>org:admin</c> scope.</remarks>
    /// <param name="subscriptionId">The webhook subscription id.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>What the API said about the response: status, headers and request id.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<TeachMeHipaaResponse> DeleteAsync(
        string subscriptionId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(subscriptionId))
        {
            throw new ArgumentException("A subscriptionId is required.", nameof(subscriptionId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Delete, $"/webhook-subscriptions/{Uri.EscapeDataString(subscriptionId)}")
            {
                OrganizationScoped = true,
            },
            requestOptions,
            cancellationToken);
    }

    /// <summary>Retrieve a webhook subscription.</summary>
    /// <remarks>Retrieves a webhook subscription. Requires the <c>webhooks:manage</c> or <c>org:admin</c> scope.</remarks>
    /// <param name="subscriptionId">The webhook subscription id.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="WebhookSubscription"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<WebhookSubscription> GetAsync(
        string subscriptionId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(subscriptionId))
        {
            throw new ArgumentException("A subscriptionId is required.", nameof(subscriptionId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, $"/webhook-subscriptions/{Uri.EscapeDataString(subscriptionId)}")
            {
                OrganizationScoped = true,
                RetrySafe = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.WebhookSubscription,
            cancellationToken);
    }

    /// <summary>List all webhook subscriptions.</summary>
    /// <remarks>Returns a list of the organization's webhook subscriptions. Signing secrets are never returned. Requires the <c>webhooks:manage</c> or <c>org:admin</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of results, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Page<WebhookSubscription>> ListAsync(
        WebhookSubscriptionListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, "/webhook-subscriptions")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Query = [new("limit", QueryValue.From(options?.Limit)), new("cursor", options?.Cursor)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PageWebhookSubscription,
            cancellationToken);
    }

    /// <summary>Retry failed deliveries.</summary>
    /// <remarks>Delivers the subscription's failed events again. Requires the <c>webhooks:manage</c> or <c>org:admin</c> scope.</remarks>
    /// <param name="subscriptionId">The webhook subscription id.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Operation"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Operation> RedriveAsync(
        string subscriptionId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(subscriptionId))
        {
            throw new ArgumentException("A subscriptionId is required.", nameof(subscriptionId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, $"/webhook-subscriptions/{Uri.EscapeDataString(subscriptionId)}:redrive")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Operation,
            cancellationToken);
    }

    /// <summary>Send a test event.</summary>
    /// <remarks>Sends a signed test delivery to the subscription's endpoint, so you can check your verifier. Requires the <c>webhooks:manage</c> or <c>org:admin</c> scope.</remarks>
    /// <param name="subscriptionId">The webhook subscription id.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Operation"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Operation> TestAsync(
        string subscriptionId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(subscriptionId))
        {
            throw new ArgumentException("A subscriptionId is required.", nameof(subscriptionId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, $"/webhook-subscriptions/{Uri.EscapeDataString(subscriptionId)}:test")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Operation,
            cancellationToken);
    }

    /// <summary>Update a webhook subscription.</summary>
    /// <remarks>Updates a webhook subscription; parameters you don't send are left unchanged. Send its current <c>ETag</c> in <c>If-Match</c>. The signing secret doesn't change. Requires the <c>webhooks:manage</c> or <c>org:admin</c> scope.</remarks>
    /// <param name="subscriptionId">The webhook subscription id.</param>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="WebhookSubscription"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<WebhookSubscription> UpdateAsync(
        string subscriptionId,
        WebhookSubscriptionUpdateOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(subscriptionId))
        {
            throw new ArgumentException("A subscriptionId is required.", nameof(subscriptionId));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(new HttpMethod("PATCH"), $"/webhook-subscriptions/{Uri.EscapeDataString(subscriptionId)}")
            {
                OrganizationScoped = true,
                Body = RequestBody.MergePatch(options, TeachMeHipaaJsonContext.Default.WebhookSubscriptionUpdateOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.WebhookSubscription,
            cancellationToken);
    }
}
