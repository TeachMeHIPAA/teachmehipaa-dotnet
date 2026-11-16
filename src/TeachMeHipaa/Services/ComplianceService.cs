// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Compliance endpoints.</summary>
public sealed class ComplianceService
{
    private readonly ApiRequestor _requestor;

    internal ComplianceService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Retrieve the compliance summary.</summary>
    /// <remarks>Counts the organization's learners in one course by status. Removed learners aren't counted.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="ComplianceSummary"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<ComplianceSummary> GetAsync(
        ComplianceGetOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, "/compliance")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Query = [new("courseId", options?.CourseId)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.ComplianceSummary,
            cancellationToken);
    }
}
