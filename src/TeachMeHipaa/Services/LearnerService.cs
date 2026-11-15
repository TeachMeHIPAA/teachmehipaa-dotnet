namespace TeachMeHipaa;

/// <summary>Learners: Members taking a Training, seen from the Organization's side.</summary>
public sealed class LearnerService
{
    private readonly ApiRequestor _requestor;

    internal LearnerService(ApiRequestor requestor) => _requestor = requestor;

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
            HttpMethod.Get,
            $"/learners/{Uri.EscapeDataString(learnerId)}",
            organizationScoped: true,
            [new("courseId", options?.CourseId)],
            requestOptions,
            TeachMeHipaaJsonContext.Default.Learner,
            cancellationToken);
    }
}
