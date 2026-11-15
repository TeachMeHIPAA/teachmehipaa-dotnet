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

    /// <summary>Retrieve a course. Only if it changed.</summary>
    /// <remarks>Sends <paramref name="ifNoneMatch"/> as <c>If-None-Match</c>. When the <see cref="Course"/> still has that ETag the API answers 304 and the result has no value, so keep using your copy: <c>var current = result.Value ?? cached;</c></remarks>
    /// <param name="courseId">The <c>courseId</c>.</param>
    /// <param name="ifNoneMatch">The ETag of your copy, or the copy itself.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The <see cref="Course"/> if it changed, else <see cref="ConditionalResult{T}.IsNotModified"/>; <see cref="ConditionalResult{T}.LastResponse"/> is set either way.</returns>
    /// <exception cref="ArgumentException"><paramref name="ifNoneMatch"/> is an object whose response had no ETag.</exception>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<ConditionalResult<Course>> GetIfModifiedAsync(
        string courseId,
        ETag ifNoneMatch,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(courseId))
        {
            throw new ArgumentException("A courseId is required.", nameof(courseId));
        }

        return _requestor.RequestConditionalAsync(
            new ApiRequest(HttpMethod.Get, $"/courses/{Uri.EscapeDataString(courseId)}")
            {
                Authenticated = false,
                RetrySafe = true,
                IfNoneMatch = ifNoneMatch.Resolve("If-None-Match", nameof(ifNoneMatch)),
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
    /// <returns>One page of results, with <see cref="TeachMeHipaaEntity.LastResponse"/> set. Iterate it with <c>await foreach</c> for every item across pages.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused the request or could not be reached.</exception>
    public Task<Page<Course>> ListAsync(
        CourseListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return _requestor.RequestPageAsync(
            new ApiRequest(HttpMethod.Get, "/courses")
            {
                Authenticated = false,
                RetrySafe = true,
                Query = [new("limit", QueryValue.From(options?.Limit)), new("cursor", options?.Cursor)],
            },
            requestOptions,
            TeachMeHipaaJsonContext.Default.PageEnvelopeCourse,
            PagePosition<Course>.Cursor("cursor"),
            cancellationToken);
    }

    /// <summary>List all courses. Iterates every page.</summary>
    /// <remarks>Iterates every item across pages, following <c>nextCursor</c> until it is null; the first page is fetched when iteration starts and each later page only when you reach it, so breaking out of the loop fetches nothing more. Equivalent to <c>(await ListAsync(...)).AutoPagingAsync()</c>.</remarks>
    /// <param name="options">The call's parameters.</param>
    /// <param name="requestOptions">Per-call settings, such as a request id or your own Idempotency-Key.</param>
    /// <param name="cancellationToken">Cancels the iteration with an <see cref="OperationCanceledException"/>.</param>
    /// <returns>Every <see cref="Course"/> in the list, fetched lazily.</returns>
    /// <exception cref="TeachMeHipaaException">The API refused a page's request or could not be reached.</exception>
    public IAsyncEnumerable<Course> ListAutoPagingAsync(
        CourseListOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        return AutoPaging.Of(token => ListAsync(options, requestOptions, token), cancellationToken);
    }
}
