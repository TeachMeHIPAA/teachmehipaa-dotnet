// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Partners endpoints.</summary>
public sealed class PartnerService
{
    private readonly ApiRequestor _requestor;

    internal PartnerService(ApiRequestor requestor)
    {
        _requestor = requestor;
        Organizations = new PartnerOrganizationService(requestor);
    }

    /// <summary>The Developer API's Partner organizations endpoints.</summary>
    public PartnerOrganizationService Organizations { get; }

    /// <summary>Retrieve your partner account.</summary>
    /// <remarks>Retrieves your partner account, including how many organizations you can create. Requires the <c>partner:orgs:read</c> scope.</remarks>
    /// <param name="partnerId">The partner account, <c>ptn_…</c>. Must be the partner the token was issued for.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Partner"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Partner> GetAsync(
        string partnerId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(partnerId))
        {
            throw new ArgumentException("A partnerId is required.", nameof(partnerId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, $"/partners/{Uri.EscapeDataString(partnerId)}")
            {
                RetrySafe = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Partner,
            cancellationToken);
    }
}
