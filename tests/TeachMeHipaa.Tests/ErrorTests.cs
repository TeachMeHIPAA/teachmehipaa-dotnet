using System.Net;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class ErrorTests
{
    // Retries are off so each scripted failure is the one the caller sees. The
    // step is scripted twice because a 401 re-mints and replays once.
    private static async Task<TeachMeHipaaException> GetFailingWith(ScriptedReply reply)
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply("jwt-1"), TokenReply("jwt-2"))
            .Script(LearnerRoute, reply, reply);
        var client = NewClient(handler, maxRetries: 0);
        return await Assert.ThrowsAnyAsync<TeachMeHipaaException>(() => client.Learners.GetAsync(LearnerId));
    }

    private static ScriptedReply Problem(int status, string code) => ProblemReply(
        status,
        $$"""{"title":"Problem","status":{{status}},"code":"{{code}}","detail":"A {{code}} problem."}""");

    [Fact]
    public async Task RaisesAProblemResponseWithItsCodeDetailAndRequestId()
    {
        var error = await GetFailingWith(ProblemReply(
            404,
            """{"title":"Not Found","status":404,"code":"not_found","detail":"No such learner."}""",
            new Dictionary<string, string> { ["x-request-id"] = "req-404-abc" }));

        Assert.IsType<NotFoundException>(error);
        Assert.Equal(HttpStatusCode.NotFound, error.StatusCode);
        Assert.Equal("not_found", error.Code);
        Assert.Equal("Not Found", error.Title);
        Assert.Equal("No such learner.", error.Detail);
        Assert.Equal("req-404-abc", error.RequestId);
        Assert.Equal("req-404-abc", error.Headers["X-Request-Id"]);
        Assert.Empty(error.Errors);
        Assert.Equal("Not Found: No such learner.", error.Message);
    }

    public static TheoryData<int, string, Type> StatusesAndTheirExceptions => new()
    {
        { 400, "validation_failed", typeof(InvalidRequestException) },
        { 401, "unauthorized", typeof(AuthenticationException) },
        { 403, "forbidden", typeof(PermissionDeniedException) },
        { 403, "spend_limit_exceeded", typeof(PermissionDeniedException) },
        { 404, "not_found", typeof(NotFoundException) },
        { 405, "method_not_allowed", typeof(InvalidRequestException) },
        { 406, "not_acceptable", typeof(InvalidRequestException) },
        { 409, "seat_unavailable", typeof(ConflictException) },
        { 412, "precondition_failed", typeof(PreconditionFailedException) },
        { 415, "unsupported_media_type", typeof(InvalidRequestException) },
        { 422, "batch_too_large", typeof(InvalidRequestException) },
        { 428, "precondition_required", typeof(PreconditionFailedException) },
        { 429, "rate_limited", typeof(RateLimitException) },
        { 500, "internal_error", typeof(ApiException) },
        { 502, "refund_failed", typeof(ApiException) },
        { 503, "temporarily_unavailable", typeof(ApiException) },
    };

    [Theory]
    [MemberData(nameof(StatusesAndTheirExceptions))]
    public async Task RaisesEachStatusAsItsException(int status, string code, Type exceptionType)
    {
        var error = await GetFailingWith(Problem(status, code));

        Assert.IsType(exceptionType, error);
        Assert.Equal((HttpStatusCode)status, error.StatusCode);
        Assert.Equal(code, error.Code);
        Assert.Equal($"A {code} problem.", error.Detail);
    }

    [Fact]
    public async Task RaisesAnUnlistedStatusAsAnApiException()
    {
        var error = await GetFailingWith(Problem(418, "teapot"));

        Assert.IsType<ApiException>(error);
        Assert.Equal((HttpStatusCode)418, error.StatusCode);
    }

    [Fact]
    public async Task RaisesIdempotencyKeyReuseAsAnIdempotencyExceptionWhichIsAnInvalidRequest()
    {
        var error = await GetFailingWith(Problem(422, "idempotency_key_reuse"));

        Assert.IsType<IdempotencyException>(error);
        Assert.IsAssignableFrom<InvalidRequestException>(error);
        Assert.Equal("idempotency_key_reuse", error.Code);
    }

    [Fact]
    public async Task KeepsAnUnknownProblemCodeAsAString()
    {
        var error = await GetFailingWith(Problem(409, "learner_on_sabbatical"));

        Assert.IsType<ConflictException>(error);
        Assert.Equal("learner_on_sabbatical", error.Code);
    }

    [Fact]
    public async Task GivesARateLimitExceptionTheRetryAfterDelay()
    {
        var error = await GetFailingWith(ProblemReply(
            429,
            """{"title":"Too Many Requests","status":429,"code":"rate_limited"}""",
            new Dictionary<string, string> { ["Retry-After"] = "7" }));

        Assert.Equal(TimeSpan.FromSeconds(7), Assert.IsType<RateLimitException>(error).RetryAfter);
    }

    [Fact]
    public async Task GivesARateLimitExceptionWithoutRetryAfterANullRetryAfter()
    {
        var error = await GetFailingWith(Problem(429, "rate_limited"));

        Assert.Null(Assert.IsType<RateLimitException>(error).RetryAfter);
    }

    [Fact]
    public async Task CarriesFieldErrorsFromAValidationProblem()
    {
        var error = await GetFailingWith(ProblemReply(
            422,
            """{"title":"Unprocessable Entity","status":422,"code":"validation_failed","errors":[{"path":"courseId","message":"Too long"}]}"""));

        Assert.IsType<InvalidRequestException>(error);
        Assert.Equal("validation_failed", error.Code);
        Assert.Null(error.Detail);
        Assert.Equal([new FieldError("courseId", "Too long")], error.Errors);
    }

    [Fact]
    public async Task RaisesA200HtmlPageAsAnApiExceptionInsteadOfAParseCrash()
    {
        const string html = "<!DOCTYPE html><html><body>Not here</body></html>";
        var error = await GetFailingWith(new ScriptedReply
        {
            Headers = new Dictionary<string, string> { ["Content-Type"] = "text/html; charset=utf-8" },
            Body = html,
        });

        Assert.IsType<ApiException>(error);
        Assert.Equal(HttpStatusCode.OK, error.StatusCode);
        Assert.Null(error.Code);
        Assert.Equal(html, error.RawBody);
        Assert.Contains("base URL", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RaisesABodyThatClaimsJsonButDoesNotParseAsAnApiException()
    {
        var error = await GetFailingWith(new ScriptedReply { Json = """{"learnerId": "sub_""" });

        Assert.IsType<ApiException>(error);
        Assert.Equal(HttpStatusCode.OK, error.StatusCode);
        Assert.Equal("""{"learnerId": "sub_""", error.RawBody);
    }

    [Fact]
    public async Task RaisesANonProblem503AsAnApiException()
    {
        // h3's default envelope, which apps/api sends when a store is down.
        var error = await GetFailingWith(new ScriptedReply
        {
            Status = HttpStatusCode.ServiceUnavailable,
            Json = """{"statusCode":503,"statusMessage":"Service Unavailable"}""",
        });

        Assert.IsType<ApiException>(error);
        Assert.Null(error.Code);
    }

    [Fact]
    public async Task RaisesAnErrorWithoutAJsonBodyUsingItsStatus()
    {
        var error = await GetFailingWith(new ScriptedReply
        {
            Status = HttpStatusCode.BadGateway,
            Headers = new Dictionary<string, string> { ["Content-Type"] = "text/plain" },
            Body = "upstream connect error",
        });

        Assert.IsType<ApiException>(error);
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Equal("upstream connect error", error.RawBody);
    }

    [Fact]
    public async Task RaisesARequestThatGotNoResponseAsAnApiConnectionException()
    {
        var cause = new HttpRequestException("Connection refused");
        var error = await GetFailingWith(new ScriptedReply { Throws = cause });

        Assert.IsType<ApiConnectionException>(error);
        Assert.Same(cause, error.InnerException);
        Assert.Null(error.StatusCode);
        Assert.Null(error.Code);
        Assert.Null(error.RequestId);
        Assert.Null(error.RawBody);
        Assert.Empty(error.Headers);
    }

    [Fact]
    public async Task RaisesABodyCutOffMidReadAsAnApiConnectionException()
    {
        var error = await GetFailingWith(new ScriptedReply { BodyThrows = new IOException("connection reset") });

        Assert.IsType<ApiConnectionException>(error);
        Assert.Null(error.StatusCode);
    }

    [Fact]
    public async Task RaisesARequestThatTimedOutAsAnApiTimeoutExceptionWhichIsAConnectionException()
    {
        var time = new TestTime();
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, ScriptedReply.NeverAnswers);
        var client = NewClient(handler, time: time, maxRetries: 0);

        var error = await Assert.ThrowsAnyAsync<TeachMeHipaaException>(() => time.RunAsync(
            client.Learners.GetAsync(LearnerId, requestOptions: new RequestOptions { Timeout = TimeSpan.FromSeconds(5) })));

        Assert.IsType<ApiTimeoutException>(error);
        Assert.IsAssignableFrom<ApiConnectionException>(error);
        Assert.Null(error.StatusCode);
        Assert.Contains("5 s", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RaisesTheTokenEndpointsErrorAsAnOAuthException()
    {
        var handler = new FakeHttpMessageHandler().Script(TokenRoute, new ScriptedReply
        {
            Status = HttpStatusCode.BadRequest,
            Headers = new Dictionary<string, string> { ["x-request-id"] = "req-token-9" },
            Json = """{"error":"invalid_scope","error_description":"The requested scope is not granted."}""",
        });
        var client = NewClient(handler);

        var error = await Assert.ThrowsAsync<OAuthException>(() => client.Learners.GetAsync(LearnerId));

        Assert.Equal(HttpStatusCode.BadRequest, error.StatusCode);
        Assert.Equal("invalid_scope", error.Code);
        Assert.Equal("The requested scope is not granted.", error.Detail);
        Assert.Equal("req-token-9", error.RequestId);
        Assert.Equal("invalid_scope: The requested scope is not granted.", error.Message);
    }

    [Fact]
    public async Task RaisesAnHtmlPageFromTheTokenEndpointAsAnApiException()
    {
        var handler = new FakeHttpMessageHandler().Script(TokenRoute, new ScriptedReply
        {
            Headers = new Dictionary<string, string> { ["Content-Type"] = "text/html" },
            Body = "<html>wrong host</html>",
        });
        var client = NewClient(handler);

        await Assert.ThrowsAsync<ApiException>(() => client.Learners.GetAsync(LearnerId));
    }
}
