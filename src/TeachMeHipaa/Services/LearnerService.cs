namespace TeachMeHipaa;

/// <summary>Learners: Members taking a Training, seen from the Organization's side.</summary>
public sealed class LearnerService
{
    private readonly ApiRequestor _requestor;

    internal LearnerService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Invites a Learner to a Course.</summary>
    /// <param name="options">Who to invite, and to which Course.</param>
    /// <param name="requestOptions">Per-call settings, such as your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Learner, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Learner> CreateAsync(
        LearnerCreateOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, "/learners")
            {
                OrganizationScoped = true,
                RetrySafe = true,
                IdempotencyKeyRequired = true,
                Body = RequestBody.Json(options, TeachMeHipaaJsonContext.Default.LearnerCreateOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Learner,
            cancellationToken);
    }

    /// <summary>Retrieves a Learner's status in one Course.</summary>
    /// <param name="learnerId">The Learner's id (<c>sub_…</c>).</param>
    /// <param name="options">Which Course to report on.</param>
    /// <param name="requestOptions">Per-call settings.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Learner, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Learner> GetAsync(
        string learnerId,
        LearnerGetOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(learnerId))
        {
            throw new ArgumentException("A Learner id is required.", nameof(learnerId));
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

    /// <summary>Changes a Learner's name or External ID.</summary>
    /// <param name="learnerId">The Learner's id (<c>sub_…</c>).</param>
    /// <param name="options">The fields to change.</param>
    /// <param name="requestOptions">Per-call settings.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The updated Learner, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Learner> UpdateAsync(
        string learnerId,
        LearnerUpdateOptions options,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(learnerId))
        {
            throw new ArgumentException("A Learner id is required.", nameof(learnerId));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return _requestor.RequestAsync(
            new ApiRequest(new HttpMethod("PATCH"), $"/learners/{Uri.EscapeDataString(learnerId)}")
            {
                OrganizationScoped = true,
                Body = RequestBody.MergePatch(options, TeachMeHipaaJsonContext.Default.LearnerUpdateOptions),
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Learner,
            cancellationToken);
    }

    /// <summary>Creates a Launch link that signs the Learner straight into their Training.</summary>
    /// <param name="learnerId">The Learner's id (<c>sub_…</c>).</param>
    /// <param name="requestOptions">Per-call settings.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The single-use link, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<LearnerLaunchLink> CreateLaunchLinkAsync(
        string learnerId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(learnerId))
        {
            throw new ArgumentException("A Learner id is required.", nameof(learnerId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Post, $"/learners/{Uri.EscapeDataString(learnerId)}:createLaunchLink")
            {
                OrganizationScoped = true,
                // A duplicate Launch link is harmless, so it needs no Idempotency-Key.
                RetrySafe = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.LearnerLaunchLink,
            cancellationToken);
    }
}
