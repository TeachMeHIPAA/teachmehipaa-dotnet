// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Seat orders endpoints.</summary>
public sealed class SeatOrderService
{
    private readonly ApiRequestor _requestor;

    internal SeatOrderService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Create a seat order.</summary>
    /// <remarks>Buys seats at list price. Pay with a Stripe-hosted checkout, the organization's saved card, or an invoice when it's enabled. Send the <c>expectedTotal</c> and <c>currency</c> from the quote you showed the buyer: if the price changed, nothing is charged. Seats are granted once payment succeeds. Requires the <c>seats:purchase</c> or <c>billing:write</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="SeatOrderCreated"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<SeatOrderCreated> CreateAsync(
        SeatOrderCreateOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, "/seat-orders")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.SeatOrderCreateOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.SeatOrderCreated,
            cancellationToken);
    }

    /// <summary>Retrieve a seat order.</summary>
    /// <remarks>Retrieves a seat order, its status and the seats it granted. Requires the <c>seats:purchase</c> or <c>billing:write</c> scope.</remarks>
    /// <param name="seatOrderId">The seat order id (<c>so_</c> + ULID).</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="SeatOrder"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<SeatOrder> GetAsync(
        string seatOrderId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(seatOrderId))
        {
            throw new ArgumentException("A seatOrderId is required.", nameof(seatOrderId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, $"/seat-orders/{Uri.EscapeDataString(seatOrderId)}")
            {
                OrganizationScoped = true,
                RetrySafe = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.SeatOrder,
            cancellationToken);
    }

    /// <summary>List all seat orders.</summary>
    /// <remarks>Returns a list of the organization's seat orders, oldest first. Requires the <c>seats:purchase</c> or <c>billing:write</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of results, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Page<SeatOrder>> ListAsync(
        SeatOrderListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, "/seat-orders")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Query = [new("limit", QueryValue.From(options?.Limit)), new("cursor", options?.Cursor)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PageSeatOrder,
            cancellationToken);
    }

    /// <summary>Quote a seat order.</summary>
    /// <remarks>Gets the list price for a number of seats in the organization's currency, before tax. The quote is for display and expires after 15 minutes. Requires the <c>seats:purchase</c> or <c>billing:write</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="SeatOrderQuote"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<SeatOrderQuote> QuoteAsync(
        SeatOrderQuoteOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, "/seat-orders:quote")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.SeatOrderQuoteOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.SeatOrderQuote,
            cancellationToken);
    }
}
