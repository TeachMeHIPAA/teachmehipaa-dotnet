// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Seats endpoints.</summary>
public sealed class SeatService
{
    private readonly ApiRequestor _requestor;

    internal SeatService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Retrieve the seat inventory.</summary>
    /// <remarks>Retrieves how many seats the organization has, how many are assigned and how many are available for new invites. Requires the <c>enrollments:read</c> scope.</remarks>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="SeatsInventory"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<SeatsInventory> GetAsync(
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, "/seats")
            {
                OrganizationScoped = true,
                RetrySafe = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.SeatsInventory,
            cancellationToken);
    }
}
