// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Enrollments endpoints.</summary>
public sealed class EnrollmentService
{
    private readonly ApiRequestor _requestor;

    internal EnrollmentService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Update many enrollments.</summary>
    /// <remarks>Reminds, resends, cancels, renews, archives or unarchives up to 500 enrollments in one request, chosen by id or by filter. Each enrollment succeeds or fails on its own. Requires the <c>enrollments:write</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="EnrollmentsBatchResults"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<EnrollmentsBatchResults> BatchAsync(
        EnrollmentBatchOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, "/enrollments:batch")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.EnrollmentBatchOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.EnrollmentsBatchResults,
            cancellationToken);
    }

    /// <summary>Update many enrollments.</summary>
    /// <remarks>Reminds, resends, cancels, renews, archives or unarchives up to 500 enrollments in one request, chosen by id or by filter. Each enrollment succeeds or fails on its own. Requires the <c>enrollments:write</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Operation"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Operation> BatchByFilterAsync(
        EnrollmentBatchByFilterOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, "/enrollments:batch")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.EnrollmentBatchByFilterOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Operation,
            cancellationToken);
    }

    /// <summary>Cancel an enrollment.</summary>
    /// <remarks>Cancels an invite that hasn't been accepted yet and returns its seat to the pool. With <c>refund: true</c>, refunds the seat to the card that paid for it instead. The enrollment remains, with status <c>CANCELLED</c>. Requires the <c>enrollments:write</c> scope.</remarks>
    /// <param name="enrollmentId">The <c>enrollmentId</c>.</param>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="CancelEnrollmentResponse"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<CancelEnrollmentResponse> CancelAsync(
        string enrollmentId,
        EnrollmentCancelOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(enrollmentId))
        {
            throw new ArgumentException("A enrollmentId is required.", nameof(enrollmentId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, $"/enrollments/{Uri.EscapeDataString(enrollmentId)}:cancel")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = options is null ? null : RequestBody.Json(options, TeachMeHipaaJsonContext.Default.EnrollmentCancelOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.CancelEnrollmentResponse,
            cancellationToken);
    }

    /// <summary>Create an enrollment.</summary>
    /// <remarks>Invites a person to a course, creating their profile if needed, and uses one seat. The course defaults to <c>hipaa</c>. Requires the <c>enrollments:write</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Enrollment"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Enrollment> CreateAsync(
        EnrollmentCreateOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, "/enrollments")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.EnrollmentCreateOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Enrollment,
            cancellationToken);
    }

    /// <summary>Export enrollments.</summary>
    /// <remarks>Exports the organization's enrollments as CSV. Requires the <c>enrollments:read</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Operation"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Operation> ExportAsync(
        EnrollmentExportOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, "/enrollments:export")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.EnrollmentExportOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Operation,
            cancellationToken);
    }

    /// <summary>Retrieve an enrollment.</summary>
    /// <remarks>Retrieves an enrollment. Requires the <c>enrollments:read</c> scope.</remarks>
    /// <param name="enrollmentId">The <c>enrollmentId</c>.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Enrollment"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Enrollment> GetAsync(
        string enrollmentId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(enrollmentId))
        {
            throw new ArgumentException("A enrollmentId is required.", nameof(enrollmentId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, $"/enrollments/{Uri.EscapeDataString(enrollmentId)}")
            {
                OrganizationScoped = true,
                RetrySafe = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Enrollment,
            cancellationToken);
    }

    /// <summary>Retrieve an enrollment. Only if it changed.</summary>
    /// <remarks>Sends <paramref name="ifNoneMatch"/> as <c>If-None-Match</c>. When the <see cref="Enrollment"/> still has that ETag the API answers 304 and the result has no value, so keep using your copy: <c>var current = result.Value ?? cached;</c></remarks>
    /// <param name="enrollmentId">The <c>enrollmentId</c>.</param>
    /// <param name="ifNoneMatch">The ETag of your copy, or the copy itself.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Enrollment"/> if it changed, else <see cref="ConditionalResult{T}.IsNotModified"/>; <see cref="ConditionalResult{T}.LastResponse"/> is set either way.</returns>
    /// <exception cref="ArgumentException"><paramref name="ifNoneMatch"/> is an object whose response had no ETag.</exception>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<ConditionalResult<Enrollment>> GetIfModifiedAsync(
        string enrollmentId,
        ETag ifNoneMatch,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(enrollmentId))
        {
            throw new ArgumentException("A enrollmentId is required.", nameof(enrollmentId));
        }

        return _requestor.RequestConditionalAsync(
            new ApiRequest(HttpMethod.Get, $"/enrollments/{Uri.EscapeDataString(enrollmentId)}")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IfNoneMatch = ifNoneMatch.Resolve("If-None-Match", nameof(ifNoneMatch)),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Enrollment,
            cancellationToken);
    }

    /// <summary>Import learners from CSV.</summary>
    /// <remarks>Invites everyone in a CSV file with an <c>email</c> column (and optional <c>name</c> and <c>courseId</c>). Send <c>validateOnly: true</c> first to preview the file without inviting anyone. Up to 500 rows. Requires the <c>enrollments:write</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Operation"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public async Task<Operation> ImportAsync(
        EnrollmentImportOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.File is null)
        {
            throw new ArgumentException("options.File is required.", nameof(options));
        }

        var fileContents = await options.File.ReadAsync("file.csv", nameof(options), cancellationToken).ConfigureAwait(false);
        return await _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, "/enrollments:import")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Multipart(
                    MultipartPart.File("file", fileContents, "text/csv"),
                    MultipartPart.Json("options", (options.Options ?? new()) with { ValidateOnly = false }, TeachMeHipaaJsonContext.Default.ImportOptions)),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Operation,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>List all enrollments.</summary>
    /// <remarks>Returns a list of the organization's enrollments. Filter by <c>status</c>, <c>courseId</c> or <c>archived</c>. Requires the <c>enrollments:read</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of results, with <see cref="TeachMeHipaaEntity.LastResponse"/> set. Iterate it with <c>await foreach</c> for every item across pages.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Page<OrgEnrollmentListItem>> ListAsync(
        EnrollmentListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestPageAsync(
            new ApiRequest(HttpMethod.Get, "/enrollments")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Query = [new("limit", QueryValue.From(options?.Limit)), new("cursor", options?.Cursor), new("status", options?.Status?.Value), new("courseId", options?.CourseId), new("archived", options?.Archived?.Value), new("include", options?.Include?.Value)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PageEnvelopeOrgEnrollmentListItem,
            PagePosition<OrgEnrollmentListItem>.Cursor("cursor"),
            cancellationToken);
    }

    /// <summary>List all enrollments. Iterates every page.</summary>
    /// <remarks>Iterates every item across pages, following <c>nextCursor</c> until it is null; the first page is fetched when iteration starts and each later page only when you reach it, so breaking out of the loop fetches nothing more. Equivalent to <c>(await ListAsync(...)).AutoPagingAsync()</c>.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the iteration with an <see cref="OperationCanceledException"/>.</param>
    /// <returns>Every <see cref="OrgEnrollmentListItem"/> in the list, fetched lazily.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused a page's request or could not be reached.</exception>
    public IAsyncEnumerable<OrgEnrollmentListItem> ListAutoPagingAsync(
        EnrollmentListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return AutoPaging.Of(token => ListAsync(options, requestOptions, token), cancellationToken);
    }

    /// <summary>Import learners from CSV.</summary>
    /// <remarks>Invites everyone in a CSV file with an <c>email</c> column (and optional <c>name</c> and <c>courseId</c>). Send <c>validateOnly: true</c> first to preview the file without inviting anyone. Up to 500 rows. Requires the <c>enrollments:write</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="ImportPreview"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public async Task<ImportPreview> PreviewImportAsync(
        EnrollmentPreviewImportOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.File is null)
        {
            throw new ArgumentException("options.File is required.", nameof(options));
        }

        var fileContents = await options.File.ReadAsync("file.csv", nameof(options), cancellationToken).ConfigureAwait(false);
        return await _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, "/enrollments:import")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Multipart(
                    MultipartPart.File("file", fileContents, "text/csv"),
                    MultipartPart.Json("options", (options.Options ?? new()) with { ValidateOnly = true }, TeachMeHipaaJsonContext.Default.ImportOptions)),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.ImportPreview,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Remind an enrollment.</summary>
    /// <remarks>Emails the learner again: the invitation if they haven't joined, or a reminder linking to the training if they have. Requires the <c>enrollments:write</c> scope.</remarks>
    /// <param name="enrollmentId">The <c>enrollmentId</c>.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="EnrollmentActionAccepted"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<EnrollmentActionAccepted> RemindAsync(
        string enrollmentId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(enrollmentId))
        {
            throw new ArgumentException("A enrollmentId is required.", nameof(enrollmentId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, $"/enrollments/{Uri.EscapeDataString(enrollmentId)}:remind")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.EnrollmentActionAccepted,
            cancellationToken);
    }

    /// <summary>Renew an enrollment.</summary>
    /// <remarks>Starts the next certification cycle for a certified learner and invites them again, using one seat. Retrying never uses a second seat. Requires the <c>enrollments:write</c> scope.</remarks>
    /// <param name="enrollmentId">The <c>enrollmentId</c>.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Enrollment"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Enrollment> RenewAsync(
        string enrollmentId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(enrollmentId))
        {
            throw new ArgumentException("A enrollmentId is required.", nameof(enrollmentId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, $"/enrollments/{Uri.EscapeDataString(enrollmentId)}:renew")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Enrollment,
            cancellationToken);
    }

    /// <summary>Resend an invite.</summary>
    /// <remarks>Sends a pending invite again, within the same cycle. To invite a certified learner again, renew the enrollment. Requires the <c>enrollments:write</c> scope.</remarks>
    /// <param name="enrollmentId">The <c>enrollmentId</c>.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="EnrollmentActionAccepted"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<EnrollmentActionAccepted> ResendInviteAsync(
        string enrollmentId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(enrollmentId))
        {
            throw new ArgumentException("A enrollmentId is required.", nameof(enrollmentId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, $"/enrollments/{Uri.EscapeDataString(enrollmentId)}:resendInvite")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.EnrollmentActionAccepted,
            cancellationToken);
    }
}
