// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Payment method endpoints.</summary>
public sealed class PaymentMethodService
{
    private readonly ApiRequestor _requestor;

    internal PaymentMethodService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Finish saving a card.</summary>
    /// <remarks>Makes the card from a confirmed SetupIntent the organization's default for seat orders. Safe to repeat. Requires the <c>seats:purchase</c> or <c>billing:write</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="BillingPaymentMethod"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<BillingPaymentMethod> ConfirmAsync(
        PaymentMethodConfirmOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, "/billing/payment-method:confirm")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.PaymentMethodConfirmOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.BillingPaymentMethod,
            cancellationToken);
    }

    /// <summary>Delete the saved card.</summary>
    /// <remarks>Removes the saved card. Seats already bought aren't affected. Requires the <c>seats:purchase</c> or <c>billing:write</c> scope.</remarks>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>What the API said about the response: status, headers and request id.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<TeachMeHipaaResponse> DeleteAsync(
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Delete, "/billing/payment-method")
            {
                OrganizationScoped = true,
            },
            requestOptions,
            cancellationToken);
    }

    /// <summary>Retrieve the saved card.</summary>
    /// <remarks>Retrieves the card seat orders charge: its brand, last four digits and expiry. Card numbers are never returned. Requires the <c>seats:purchase</c> or <c>billing:write</c> scope.</remarks>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="BillingPaymentMethod"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<BillingPaymentMethod> GetAsync(
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, "/billing/payment-method")
            {
                OrganizationScoped = true,
                RetrySafe = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.BillingPaymentMethod,
            cancellationToken);
    }

    /// <summary>Start saving a card.</summary>
    /// <remarks>Creates a Stripe SetupIntent for a card form in your own page. Confirm it with Stripe.js, then finish saving the card. Saving a card never charges it. Requires the <c>seats:purchase</c> or <c>billing:write</c> scope.</remarks>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="PaymentMethodPrepare"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<PaymentMethodPrepare> PrepareAsync(
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, "/billing/payment-method:prepare")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PaymentMethodPrepare,
            cancellationToken);
    }

    /// <summary>Save a card with Checkout.</summary>
    /// <remarks>Creates a Stripe-hosted page where the buyer saves the card that seat orders charge. The card becomes the organization's default. Requires the <c>seats:purchase</c> or <c>billing:write</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="PaymentMethodSetup"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<PaymentMethodSetup> SetupAsync(
        PaymentMethodSetupOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, "/billing/payment-method:setup")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.PaymentMethodSetupOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PaymentMethodSetup,
            cancellationToken);
    }
}
