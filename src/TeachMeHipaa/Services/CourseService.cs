// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Calls the Developer API's Courses endpoints.</summary>
public sealed class CourseService
{
    private readonly ApiRequestor _requestor;

    internal CourseService(ApiRequestor requestor) => _requestor = requestor;

    /// <summary>Retrieve a course.</summary>
    /// <remarks>Retrieves a course from the public catalog.</remarks>
    /// <param name="courseId">The <c>courseId</c>.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Course"/>, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Course> GetAsync(
        string courseId,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(courseId))
        {
            throw new ArgumentException("A courseId is required.", nameof(courseId));
        }

        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, $"/courses/{Uri.EscapeDataString(courseId)}")
            {
                Authenticated = false,
                RetrySafe = true,
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.Course,
            cancellationToken);
    }

    /// <summary>List all courses.</summary>
    /// <remarks>Returns a list of the courses in the public catalog. Needs no access token.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of results, with <see cref="TeachMeHipaaEntity.LastResponse"/> set.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Page<Course>> ListAsync(
        CourseListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestAsync(
            new ApiRequest(HttpMethod.Get, "/courses")
            {
                Authenticated = false,
                RetrySafe = true,
                Query = [new("limit", QueryValue.From(options?.Limit)), new("cursor", options?.Cursor)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PageCourse,
            cancellationToken);
    }
}
