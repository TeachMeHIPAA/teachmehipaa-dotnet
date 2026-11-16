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

    /// <summary>Download a certificate. Streams the file.</summary>
    /// <remarks>Returns once the response headers arrive; read the file from <see cref="BinaryStreamResponse.Content"/> as it comes, then dispose the response. <see cref="DownloadPdfAsync"/> returns the whole file in memory instead.</remarks>
    /// <param name="certId">The <c>certId</c>.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call until the headers arrive.</param>
    /// <returns>The file as a stream, with its content type and file name. Dispose it to release the connection.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<BinaryStreamResponse> DownloadPdfStreamingAsync(
        string certId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(certId))
        {
            throw new ArgumentException("A certId is required.", nameof(certId));
        }

        return _requestor.RequestBinaryStreamAsync(
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
    /// <returns>The <see cref="Operation"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set. Most are done when returned; <see cref="Operation.WaitAsync"/> polls until one is.</returns>
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

        return _requestor.RequestOperationAsync(
            new ApiRequest(HttpMethod.Post, "/certificates:export")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.CertificateExportOptions),
            },
            requestOptions,
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
    /// <returns>One page of results, with <see cref="TeachMeHipaaEntity.LastResponse"/> set. Iterate it with <c>await foreach</c> for every item across pages.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Page<Certificate>> ListAsync(
        CertificateListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestPageAsync(
            new ApiRequest(HttpMethod.Get, "/certificates")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Query = [new("limit", QueryValue.From(options?.Limit)), new("cursor", options?.Cursor), new("subjectId", options?.SubjectId)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PageEnvelopeCertificate,
            PagePosition<Certificate>.Cursor("cursor"),
            cancellationToken);
    }

    /// <summary>List all certificates. Iterates every page.</summary>
    /// <remarks>Iterates every item across pages, following <c>nextCursor</c> until it is null; the first page is fetched when iteration starts and each later page only when you reach it, so breaking out of the loop fetches nothing more. Equivalent to <c>(await ListAsync(...)).AutoPagingAsync()</c>.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the iteration with an <see cref="OperationCanceledException"/>.</param>
    /// <returns>Every <see cref="Certificate"/> in the list, fetched lazily.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused a page's request or could not be reached.</exception>
    public IAsyncEnumerable<Certificate> ListAutoPagingAsync(
        CertificateListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return AutoPaging.Of(token => ListAsync(options, requestOptions, token), cancellationToken);
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
