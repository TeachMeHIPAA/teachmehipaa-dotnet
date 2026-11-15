// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Members endpoints.</summary>
public sealed class MemberService
{
    private readonly ApiRequestor _requestor;

    internal MemberService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Retrieve a member.</summary>
    /// <remarks>Retrieves a member of the organization.</remarks>
    /// <param name="subjectId">The member's id.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Member"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Member> GetAsync(
        string subjectId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(subjectId))
        {
            throw new ArgumentException("A subjectId is required.", nameof(subjectId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, $"/members/{Uri.EscapeDataString(subjectId)}")
            {
                OrganizationScoped = true,
                RetrySafe = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Member,
            cancellationToken);
    }

    /// <summary>List all members.</summary>
    /// <remarks>Returns a list of the organization's members, its admins and learners. Pass <c>role=admin</c> to list only admins.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of results, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Page<MemberListItem>> ListAsync(
        MemberListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, "/members")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Query = [new("limit", QueryValue.From(options?.Limit)), new("cursor", options?.Cursor), new("include", options?.Include), new("role", options?.Role?.Value)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PageMemberListItem,
            cancellationToken);
    }
}
