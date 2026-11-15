// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Events endpoints.</summary>
public sealed class EventService
{
    private readonly ApiRequestor _requestor;

    internal EventService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>List all events.</summary>
    /// <remarks>Returns a list of the organization's events from the last 30 days, oldest first. Pass the last <c>eventId</c> you processed as <c>after</c> to resume where you stopped, and <c>type</c> to list one event type.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of results, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Page<Event>> ListAsync(
        EventListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, "/events")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Query = [new("limit", QueryValue.From(options?.Limit)), new("after", options?.After), new("type", options?.Type?.Value)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PageEvent,
            cancellationToken);
    }
}
