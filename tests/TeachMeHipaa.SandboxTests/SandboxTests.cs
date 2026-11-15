using System.Net;

namespace TeachMeHipaa.SandboxTests;

/// <summary>A fact that runs only when Sandbox credentials are in the environment.</summary>
public sealed class SandboxFactAttribute : FactAttribute
{
    public SandboxFactAttribute()
    {
        if (SandboxTests.ClientId is null || SandboxTests.ClientSecret is null)
        {
            Skip = "Set TMH_SANDBOX_CLIENT_ID and TMH_SANDBOX_CLIENT_SECRET to run the Sandbox smoke test.";
        }
    }
}

/// <summary>Reads from the real Sandbox Environment. It never writes.</summary>
public class SandboxTests
{
    internal static readonly string? ClientId = Credential("TMH_SANDBOX_CLIENT_ID");
    internal static readonly string? ClientSecret = Credential("TMH_SANDBOX_CLIENT_SECRET");

    private static string? Credential(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : null;

    private static TeachMeHipaaClient NewClient() => new(new TeachMeHipaaClientOptions
    {
        ClientId = ClientId!,
        ClientSecret = ClientSecret!,
        Environment = TeachMeHipaaEnvironment.Sandbox,
    });

    [SandboxFact]
    public async Task IntrospectsASandboxAccessTokenForAnOrganization()
    {
        var introspection = await NewClient().Auth.IntrospectAsync();

        // The spec: "`test` for a Sandbox client, `live` otherwise."
        Assert.Equal(TokenIntrospectionMode.Test, introspection.Mode);
        Assert.False(string.IsNullOrEmpty(introspection.OrgId));
    }

    [SandboxFact]
    public async Task ListsCourses()
    {
        var page = await NewClient().Courses.ListAsync(new CourseListOptions { Limit = 5 });

        Assert.Equal(HttpStatusCode.OK, page.LastResponse!.StatusCode);
    }

    [SandboxFact]
    public async Task ListsLearners()
    {
        var page = await NewClient().Learners.ListAsync(new LearnerListOptions { Limit = 5 });

        Assert.Equal(HttpStatusCode.OK, page.LastResponse!.StatusCode);
    }
}
