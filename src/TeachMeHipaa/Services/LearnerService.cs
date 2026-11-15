// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Learners endpoints.</summary>
public sealed class LearnerService
{
    private readonly ApiRequestor _requestor;

    internal LearnerService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Create many learners.</summary>
    /// <remarks>Creates or retrieves up to 500 learners in one request. Each item works like creating one learner and succeeds or fails on its own.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Operation"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set. Most are done when returned; <see cref="Operation.WaitAsync"/> polls until one is.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Operation> BatchUpsertAsync(
        LearnerBatchUpsertOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestOperationAsync(
            new ApiRequest(HttpMethod.Post, "/learners:batchUpsert")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.LearnerBatchUpsertOptions),
            },
            requestOptions,
            cancellationToken);
    }

    /// <summary>Create a learner.</summary>
    /// <remarks>Adds a person to the organization and invites them to a course in one call, using one seat. If the email or <c>externalId</c> already belongs to an active learner in the course, nothing is created. With <c>delivery: "none"</c>, no email is sent and the response includes a <c>joinUrl</c> for you to deliver.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="LearnerWriteResult"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set; <c>Created</c> is true when this call created it (201) and false when it already existed (200).</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<LearnerWriteResult> CreateAsync(
        LearnerCreateOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestWriteResultAsync(
            new ApiRequest(HttpMethod.Post, "/learners")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.LearnerCreateOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.LearnerWriteResult,
            static (result, created) => result with { Created = created },
            cancellationToken);
    }

    /// <summary>Create a launch link.</summary>
    /// <remarks>Creates a link that opens the course with the learner already signed in, for a "Start training" button in your product. The link works once and expires after 5 minutes: create it when the learner clicks, then redirect them to it. Never email, store or log it. Only for a learner who has accepted their invite.</remarks>
    /// <param name="learnerId">The learner's canonical subject id (sub_… ULID).</param>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="LearnerLaunchLink"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<LearnerLaunchLink> CreateLaunchLinkAsync(
        string learnerId,
        LearnerCreateLaunchLinkOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(learnerId))
        {
            throw new ArgumentException("A learnerId is required.", nameof(learnerId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, $"/learners/{Uri.EscapeDataString(learnerId)}:createLaunchLink")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Query = [new("courseId", options?.CourseId)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.LearnerLaunchLink,
            cancellationToken);
    }

    /// <summary>Export learners.</summary>
    /// <remarks>Exports the organization's learners in one course as CSV, optionally only those with one <c>status</c>. Requires the <c>learners:read</c> scope.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Operation"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set. Most are done when returned; <see cref="Operation.WaitAsync"/> polls until one is.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Operation> ExportAsync(
        LearnerExportOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestOperationAsync(
            new ApiRequest(HttpMethod.Post, "/learners:export")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.LearnerExportOptions),
            },
            requestOptions,
            cancellationToken);
    }

    /// <summary>Retrieve a learner.</summary>
    /// <remarks>Retrieves a learner's status in one course.</remarks>
    /// <param name="learnerId">The learner's canonical subject id (sub_… ULID).</param>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Learner"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Learner> GetAsync(
        string learnerId,
        LearnerGetOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(learnerId))
        {
            throw new ArgumentException("A learnerId is required.", nameof(learnerId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, $"/learners/{Uri.EscapeDataString(learnerId)}")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Query = [new("courseId", options?.CourseId)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Learner,
            cancellationToken);
    }

    /// <summary>List all learners.</summary>
    /// <remarks>Returns a list of the organization's learners in one course, with each learner's status, dates and current certificate. Filter by <c>status</c>, or find one learner by <c>email</c> or <c>externalId</c>.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of results, with <see cref="TeachMeHipaaEntity.LastResponse"/> set. Iterate it with <c>await foreach</c> for every item across pages.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Page<Learner>> ListAsync(
        LearnerListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestPageAsync(
            new ApiRequest(HttpMethod.Get, "/learners")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                Query = [new("limit", QueryValue.From(options?.Limit)), new("cursor", options?.Cursor), new("email", options?.Email), new("externalId", options?.ExternalId), new("status", options?.Status?.Value), new("courseId", options?.CourseId)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PageEnvelopeLearner,
            PagePosition<Learner>.Cursor("cursor"),
            cancellationToken);
    }

    /// <summary>List all learners. Iterates every page.</summary>
    /// <remarks>Iterates every item across pages, following <c>nextCursor</c> until it is null; the first page is fetched when iteration starts and each later page only when you reach it, so breaking out of the loop fetches nothing more. Equivalent to <c>(await ListAsync(...)).AutoPagingAsync()</c>.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the iteration with an <see cref="OperationCanceledException"/>.</param>
    /// <returns>Every <see cref="Learner"/> in the list, fetched lazily.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused a page's request or could not be reached.</exception>
    public IAsyncEnumerable<Learner> ListAutoPagingAsync(
        LearnerListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return AutoPaging.Of(token => ListAsync(options, requestOptions, token), cancellationToken);
    }

    /// <summary>Offboard a learner.</summary>
    /// <remarks>Removes a learner from the organization. A learner who hasn't started has their invite cancelled and the seat returned; a learner who has started keeps their training history and certificates. Releases the learner's <c>externalId</c>.</remarks>
    /// <param name="learnerId">The learner's canonical subject id (sub_… ULID).</param>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Learner"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Learner> OffboardAsync(
        string learnerId,
        LearnerOffboardOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(learnerId))
        {
            throw new ArgumentException("A learnerId is required.", nameof(learnerId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, $"/learners/{Uri.EscapeDataString(learnerId)}:offboard")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                IfMatch = options?.IfMatch?.Resolve("If-Match", nameof(options)),
                Query = [new("courseId", options?.CourseId)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Learner,
            cancellationToken);
    }

    /// <summary>Remind a learner.</summary>
    /// <remarks>Emails the learner a reminder: the invite again if they haven't started, or a nudge to finish if they have.</remarks>
    /// <param name="learnerId">The learner's canonical subject id (sub_… ULID).</param>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="LearnerRemindAccepted"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<LearnerRemindAccepted> RemindAsync(
        string learnerId,
        LearnerRemindOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(learnerId))
        {
            throw new ArgumentException("A learnerId is required.", nameof(learnerId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, $"/learners/{Uri.EscapeDataString(learnerId)}:remind")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Query = [new("courseId", options?.CourseId)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.LearnerRemindAccepted,
            cancellationToken);
    }

    /// <summary>Update a learner.</summary>
    /// <remarks>Updates a learner's <c>name</c> or <c>externalId</c>; parameters you don't send are left unchanged. Send the body as <c>application/merge-patch+json</c> with the learner's current <c>ETag</c> in <c>If-Match</c>.</remarks>
    /// <param name="learnerId">The learner's canonical subject id (sub_… ULID).</param>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Learner"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Learner> UpdateAsync(
        string learnerId,
        LearnerUpdateOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(learnerId))
        {
            throw new ArgumentException("A learnerId is required.", nameof(learnerId));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(new HttpMethod("PATCH"), $"/learners/{Uri.EscapeDataString(learnerId)}")
            {
                OrganizationScoped = true,
                IfMatch = options.IfMatch.Resolve("If-Match", nameof(options)),
                Query = [new("courseId", options.CourseId)],
                Body = RequestBody.MergePatch(options, TeachMeHipaaJsonContext.Default.LearnerUpdateOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Learner,
            cancellationToken);
    }

    /// <summary>Create or retrieve a learner by your id.</summary>
    /// <remarks>Makes sure a learner exists for your own id. Creates and invites the learner the first time, and returns them on every later call, so a sync job can replay it safely. Never uses a second seat.</remarks>
    /// <param name="externalId">Your id for the learner, URL-encoded.</param>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="LearnerWriteResult"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set; <c>Created</c> is true when this call created it (201) and false when it already existed (200).</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<LearnerWriteResult> UpsertByExternalIdAsync(
        string externalId,
        LearnerUpsertByExternalIdOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(externalId))
        {
            throw new ArgumentException("A externalId is required.", nameof(externalId));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestWriteResultAsync(
            new ApiRequest(HttpMethod.Put, $"/learners/by-external-id/{Uri.EscapeDataString(externalId)}")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.LearnerUpsertByExternalIdOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.LearnerWriteResult,
            static (result, created) => result with { Created = created },
            cancellationToken);
    }
}
