// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Partner organizations endpoints.</summary>
public sealed class PartnerOrganizationService
{
    private readonly ApiRequestor _requestor;

    internal PartnerOrganizationService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Create a customer organization.</summary>
    /// <remarks>Creates an organization for a customer with your app already connected, and emails its administrator a link to sign in. The organization has no seats until seats are bought for it. Requires the <c>partner:orgs:write</c> scope.</remarks>
    /// <param name="partnerId">The partner account, <c>ptn_…</c>. Must be the partner the token was issued for.</param>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="PartnerOrganizationCreated"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<PartnerOrganizationCreated> CreateAsync(
        string partnerId,
        PartnerOrganizationCreateOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(partnerId))
        {
            throw new ArgumentException("A partnerId is required.", nameof(partnerId));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, $"/partners/{Uri.EscapeDataString(partnerId)}/organizations")
            {
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.PartnerOrganizationCreateOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PartnerOrganizationCreated,
            cancellationToken);
    }

    /// <summary>List all customer organizations.</summary>
    /// <remarks>Returns a list of the organizations you created. <c>grantActive</c> is <c>false</c> once a customer disconnects your app. Requires the <c>partner:orgs:read</c> scope.</remarks>
    /// <param name="partnerId">The partner account, <c>ptn_…</c>. Must be the partner the token was issued for.</param>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of results, with <see cref="TeachMeHipaaEntity.LastResponse"/> set. Iterate it with <c>await foreach</c> for every item across pages.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Page<PartnerOrganization>> ListAsync(
        string partnerId,
        PartnerOrganizationListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(partnerId))
        {
            throw new ArgumentException("A partnerId is required.", nameof(partnerId));
        }

        return _requestor.RequestPageAsync(
            new ApiRequest(HttpMethod.Get, $"/partners/{Uri.EscapeDataString(partnerId)}/organizations")
            {
                RetrySafe = true,
                Query = [new("limit", QueryValue.From(options?.Limit)), new("cursor", options?.Cursor)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PageEnvelopePartnerOrganization,
            PagePosition<PartnerOrganization>.Cursor("cursor"),
            cancellationToken);
    }

    /// <summary>List all customer organizations. Iterates every page.</summary>
    /// <remarks>Iterates every item across pages, following <c>nextCursor</c> until it is null; the first page is fetched when iteration starts and each later page only when you reach it, so breaking out of the loop fetches nothing more. Equivalent to <c>(await ListAsync(...)).AutoPagingAsync()</c>.</remarks>
    /// <param name="partnerId">The partner account, <c>ptn_…</c>. Must be the partner the token was issued for.</param>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the iteration with an <see cref="OperationCanceledException"/>.</param>
    /// <returns>Every <see cref="PartnerOrganization"/> in the list, fetched lazily.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused a page's request or could not be reached.</exception>
    public IAsyncEnumerable<PartnerOrganization> ListAutoPagingAsync(
        string partnerId,
        PartnerOrganizationListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(partnerId))
        {
            throw new ArgumentException("A partnerId is required.", nameof(partnerId));
        }

        return AutoPaging.Of(token => ListAsync(partnerId, options, requestOptions, token), cancellationToken);
    }
}
