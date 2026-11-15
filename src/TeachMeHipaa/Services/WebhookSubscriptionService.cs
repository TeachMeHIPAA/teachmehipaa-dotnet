namespace TeachMeHipaa;

/// <summary>Webhook subscriptions: where and for which Events the Organization's Deliveries are sent.</summary>
public sealed class WebhookSubscriptionService
{
    private readonly ApiRequestor _requestor;

    internal WebhookSubscriptionService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Deletes a Webhook subscription, so no more Deliveries are sent to it.</summary>
    /// <param name="subscriptionId">The subscription's id (<c>whs_…</c>).</param>
    /// <param name="requestOptions">Per-call settings.</param>
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
            throw new ArgumentException("A Webhook subscription id is required.", nameof(subscriptionId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Delete, $"/webhook-subscriptions/{Uri.EscapeDataString(subscriptionId)}")
            {
                OrganizationScoped = true,
            },
            requestOptions,
            cancellationToken);
    }
}
