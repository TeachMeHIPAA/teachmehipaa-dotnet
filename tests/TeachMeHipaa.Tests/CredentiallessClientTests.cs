using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

/// <summary>
/// A client built with neither credentials nor a token provider, for public
/// lookups such as Certificate verification (spec story 27).
/// </summary>
public class CredentiallessClientTests
{
    private const string VerificationRoute = "GET /v1/certificate-verifications/Ab3dE";

    private static TeachMeHipaaClient PublicClient(FakeHttpMessageHandler handler) =>
        new(new TeachMeHipaaClientOptions
        {
            Environment = TeachMeHipaaEnvironment.Sandbox,
            HttpMessageHandler = handler,
        });

    [Fact]
    public async Task VerifiesACertificateAndListsCoursesWithoutCredentials()
    {
        var handler = new FakeHttpMessageHandler()
            .Script(VerificationRoute, new ScriptedReply
            {
                Json = """
                    {"status":"valid","learnerName":"Ana Example","courseTitle":"HIPAA Training 2026","issuedAt":"2026-11-03T08:00:00.000Z","expiresAt":"2027-11-03T08:00:00.000Z","issuer":"TeachMeHIPAA"}
                    """,
            })
            .Script("GET /v1/courses", new ScriptedReply
            {
                Json = """
                    {"items":[{"courseId":"hipaa-2026","title":"HIPAA Training 2026","courseVersion":"2026.1","passingThreshold":0.8}],"nextCursor":null}
                    """,
            });
        var client = PublicClient(handler);

        var verification = await client.CertificateVerifications.GetAsync("Ab3dE");
        var courses = await client.Courses.ListAsync();

        Assert.Equal(CertificateVerificationStatus.Valid, verification.Status);
        Assert.Equal("hipaa-2026", Assert.Single(courses.Items).CourseId);
        Assert.Equal(
            ["https://sandbox.api.tme.so/v1/certificate-verifications/Ab3dE", "https://sandbox.api.tme.so/v1/courses"],
            handler.Requests.Select(request => request.Uri.AbsoluteUri));
        Assert.All(handler.Requests, request => Assert.Null(request.Header("Authorization")));
    }

    [Fact]
    public async Task RefusesAnAuthenticatedCallBeforeSendingAnythingAndNamesTheEndpoint()
    {
        var handler = new FakeHttpMessageHandler();
        var client = PublicClient(handler);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => client.Learners.GetAsync(LearnerId));

        Assert.Contains($"GET /organizations/{{orgId}}/learners/{LearnerId}", error.Message, StringComparison.Ordinal);
        Assert.Contains("ClientId and ClientSecret", error.Message, StringComparison.Ordinal);
        Assert.Contains("TokenProvider", error.Message, StringComparison.Ordinal);
        // Not even the Organization lookup, which needs a token too.
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task RefusesAnAuthenticatedCallThatIsNotOrganizationScoped()
    {
        var handler = new FakeHttpMessageHandler();
        var client = PublicClient(handler);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => client.Auth.IntrospectAsync());

        Assert.Contains("GET /auth/introspection", error.Message, StringComparison.Ordinal);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task RefusesAnAuthenticatedCallEvenWithAnOrganizationId()
    {
        var handler = new FakeHttpMessageHandler();
        var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            OrganizationId = OrgId,
            HttpMessageHandler = handler,
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.Compliance.GetAsync());

        Assert.Empty(handler.Requests);
    }

    public static TheoryData<string, TeachMeHipaaClientOptions> PartialCredentials => new()
    {
        { "ClientSecret", new TeachMeHipaaClientOptions { ClientId = ClientId } },
        { "ClientId", new TeachMeHipaaClientOptions { ClientSecret = ClientSecret } },
        { "ClientId", new TeachMeHipaaClientOptions { Scopes = ["learners:read"] } },
    };

    [Theory]
    [MemberData(nameof(PartialCredentials))]
    public void StillRejectsHalfACredentialPairWhenConstructed(string option, TeachMeHipaaClientOptions options)
    {
        var error = Assert.ThrowsAny<ArgumentException>(() => new TeachMeHipaaClient(options));

        Assert.Equal(option, error.ParamName);
    }
}
