// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Certificates endpoints.</summary>
public sealed class CertificateService
{
    private readonly ApiRequestor _requestor;

    internal CertificateService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Download a certificate.</summary>
    /// <remarks>Downloads the certificate as a PDF. Revoked and expired certificates are watermarked. Requires the <c>certificates:read</c> scope.</remarks>
    /// <param name="certId">The <c>certId</c>.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The file's bytes, content type and file name.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<BinaryResponse> DownloadPdfAsync(
        string certId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(certId))
        {
            throw new ArgumentException("A certId is required.", nameof(certId));
        }

        return _requestor.RequestBinaryAsync(
            new ApiRequest(HttpMethod.Get, $"/certificates/{Uri.EscapeDataString(certId)}/content")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Accept = "application/pdf",
            },
            requestOptions,
            cancellationToken);
    }

    /// <summary>Export certificates.</summary>
    /// <remarks>Exports the organization's certificate records as CSV. Requires the <c>certificates:read</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Operation"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Operation> ExportAsync(
        CertificateExportOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, "/certificates:export")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.CertificateExportOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Operation,
            cancellationToken);
    }

    /// <summary>Retrieve a certificate.</summary>
    /// <remarks>Retrieves a certificate.</remarks>
    /// <param name="certId">The <c>certId</c>.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Certificate"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Certificate> GetAsync(
        string certId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(certId))
        {
            throw new ArgumentException("A certId is required.", nameof(certId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, $"/certificates/{Uri.EscapeDataString(certId)}")
            {
                OrganizationScoped = true,
                RetrySafe = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Certificate,
            cancellationToken);
    }

    /// <summary>List all certificates.</summary>
    /// <remarks>Returns a list of the organization's certificates. Pass <c>subjectId</c> to list one learner's.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of results, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Page<Certificate>> ListAsync(
        CertificateListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, "/certificates")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Query = [new("limit", QueryValue.From(options?.Limit)), new("cursor", options?.Cursor), new("subjectId", options?.SubjectId)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PageCertificate,
            cancellationToken);
    }

    /// <summary>Retrieve certificate links.</summary>
    /// <remarks>Retrieves the certificate's LinkedIn add-to-profile link and its public <c>tme.so</c> verification link. Requires the <c>certificates:read</c> scope.</remarks>
    /// <param name="certId">The <c>certId</c>.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="CertificateLinks"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<CertificateLinks> RetrieveLinksAsync(
        string certId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(certId))
        {
            throw new ArgumentException("A certId is required.", nameof(certId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, $"/certificates/{Uri.EscapeDataString(certId)}/credentials")
            {
                OrganizationScoped = true,
                RetrySafe = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.CertificateLinks,
            cancellationToken);
    }
}
