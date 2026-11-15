// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Auth endpoints.</summary>
public sealed class AuthService
{
    private readonly ApiRequestor _requestor;

    internal AuthService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Retrieve the current token.</summary>
    /// <remarks>Describes the access token you call with: whether it belongs to an API client or a person, its organization, its scopes and when it expires. Needs no scope.</remarks>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="TokenIntrospection"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<TokenIntrospection> IntrospectAsync(
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, "/auth/introspection")
            {
                RetrySafe = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.TokenIntrospection,
            cancellationToken);
    }
}
