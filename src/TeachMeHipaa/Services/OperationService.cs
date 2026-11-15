// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Operations endpoints.</summary>
public sealed class OperationService
{
    private readonly ApiRequestor _requestor;

    internal OperationService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Retrieve an operation.</summary>
    /// <remarks>Retrieves a bulk operation and its result. Operations are kept for 24 hours.</remarks>
    /// <param name="operationId">The server-minted opaque operation id.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Operation"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Operation> GetAsync(
        string operationId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(operationId))
        {
            throw new ArgumentException("A operationId is required.", nameof(operationId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, $"/operations/{Uri.EscapeDataString(operationId)}")
            {
                OrganizationScoped = true,
                RetrySafe = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Operation,
            cancellationToken);
    }
}
