using System.Net;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class ErrorTests
{
    private static async Task<TeachMeHipaaException> GetFailingWith(ScriptedReply reply)
    {
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, reply);
        var client = NewClient(handler);
        return await Assert.ThrowsAsync<TeachMeHipaaException>(() => client.Learners.GetAsync(LearnerId));
    }

    [Fact]
    public async Task RaisesAProblemResponseAsATeachMeHipaaException()
    {
        var error = await GetFailingWith(ProblemReply(
            404,
            """{"title":"Not Found","status":404,"code":"not_found","detail":"No such learner."}""",
            new Dictionary<string, string> { ["x-request-id"] = "req-404-abc" }));

        Assert.Equal(HttpStatusCode.NotFound, error.StatusCode);
        Assert.Equal("not_found", error.Code);
        Assert.Equal("Not Found", error.Title);
        Assert.Equal("No such learner.", error.Detail);
        Assert.Equal("req-404-abc", error.RequestId);
        Assert.Equal("req-404-abc", error.Headers["X-Request-Id"]);
        Assert.Empty(error.Errors);
        Assert.Equal("Not Found: No such learner.", error.Message);
    }

    [Fact]
    public async Task CarriesFieldErrorsFromAValidationProblem()
    {
        var error = await GetFailingWith(ProblemReply(
            422,
            """{"title":"Unprocessable Entity","status":422,"code":"validation_failed","errors":[{"path":"courseId","message":"Too long"}]}"""));

        Assert.Equal((HttpStatusCode)422, error.StatusCode);
        Assert.Equal("validation_failed", error.Code);
        Assert.Null(error.Detail);
        Assert.Equal([new FieldError("courseId", "Too long")], error.Errors);
    }

    [Fact]
    public async Task KeepsAnUnknownProblemCodeAsAString()
    {
        var error = await GetFailingWith(ProblemReply(
            409,
            """{"title":"Conflict","status":409,"code":"learner_on_sabbatical"}"""));

        Assert.Equal("learner_on_sabbatical", error.Code);
    }

    [Fact]
    public async Task RaisesA200HtmlPageAsATeachMeHipaaExceptionInsteadOfAParseCrash()
    {
        const string html = "<!DOCTYPE html><html><body>Not here</body></html>";
        var error = await GetFailingWith(new ScriptedReply
        {
            Headers = new Dictionary<string, string> { ["Content-Type"] = "text/html; charset=utf-8" },
            Body = html,
        });

        Assert.Equal(HttpStatusCode.OK, error.StatusCode);
        Assert.Null(error.Code);
        Assert.Equal(html, error.RawBody);
        Assert.Contains("text/html", error.Message, StringComparison.Ordinal);
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

        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Equal("upstream connect error", error.RawBody);
    }
}
