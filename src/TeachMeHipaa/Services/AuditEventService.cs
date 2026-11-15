// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Audit events endpoints.</summary>
public sealed class AuditEventService
{
    private readonly ApiRequestor _requestor;

    internal AuditEventService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>List all audit events.</summary>
    /// <remarks>Returns a list of the organization's audit events, newest first. Filter by <c>type</c>, <c>transition</c>, <c>subjectId</c>, or a <c>since</c> and <c>until</c> range.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of results, with <see cref="TeachMeHipaaEntity.LastResponse"/> set. Iterate it with <c>await foreach</c> for every item across pages.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Page<AuditEvent>> ListAsync(
        AuditEventListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestPageAsync(
            new ApiRequest(HttpMethod.Get, "/audit-events")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Query = [new("limit", QueryValue.From(options?.Limit)), new("cursor", options?.Cursor), new("type", options?.Type?.Value), new("transition", options?.Transition?.Value), new("subjectId", options?.SubjectId), new("since", QueryValue.From(options?.Since)), new("until", QueryValue.From(options?.Until))],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PageEnvelopeAuditEvent,
            PagePosition<AuditEvent>.Cursor("cursor"),
            cancellationToken);
    }

    /// <summary>List all audit events. Iterates every page.</summary>
    /// <remarks>Iterates every item across pages, following <c>nextCursor</c> until it is null; the first page is fetched when iteration starts and each later page only when you reach it, so breaking out of the loop fetches nothing more. Equivalent to <c>(await ListAsync(...)).AutoPagingAsync()</c>.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the iteration with an <see cref="OperationCanceledException"/>.</param>
    /// <returns>Every <see cref="AuditEvent"/> in the list, fetched lazily.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused a page's request or could not be reached.</exception>
    public IAsyncEnumerable<AuditEvent> ListAutoPagingAsync(
        AuditEventListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return AutoPaging.Of(token => ListAsync(options, requestOptions, token), cancellationToken);
    }
}
