// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Certificate verifications endpoints.</summary>
public sealed class CertificateVerificationService
{
    private readonly ApiRequestor _requestor;

    internal CertificateVerificationService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Verify a certificate.</summary>
    /// <remarks>Verifies the certificate behind a <c>https://tme.so/{tinyId}</c> link. Needs no access token. Limited to 60 requests a minute per IP address.</remarks>
    /// <param name="tinyId">The id at the end of the certificate's tme.so link.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="CertificateVerification"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<CertificateVerification> GetAsync(
        string tinyId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(tinyId))
        {
            throw new ArgumentException("A tinyId is required.", nameof(tinyId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, $"/certificate-verifications/{Uri.EscapeDataString(tinyId)}")
            {
                Authenticated = false,
                RetrySafe = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.CertificateVerification,
            cancellationToken);
    }
}
