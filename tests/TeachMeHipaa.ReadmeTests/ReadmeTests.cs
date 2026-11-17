using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.ReadmeTests;

/// <summary>
/// Runs every sample in the README against the fake transport and checks
/// what it printed and sent. That the samples compile is checked by the
/// build.
/// </summary>
public class ReadmeTests
{
    private static readonly IReadOnlyDictionary<string, Func<ReadmeHarness, Task>> Checks = new Dictionary<string, Func<ReadmeHarness, Task>>
    {
        ["quickstart"] = Quickstart,
    };

    public static TheoryData<string> SampleIds => [.. ReadmeSamples.Ids];

    [Fact]
    public void EverySampleHasACheck()
    {
        Assert.Equal(ReadmeSamples.Ids.Order(), Checks.Keys.Order());
    }

    [Theory]
    [MemberData(nameof(SampleIds))]
    public async Task SampleRuns(string id)
    {
        await using var readme = new ReadmeHarness();

        await Checks[id](readme);
    }

    private static async Task Quickstart(ReadmeHarness readme)
    {
        readme.Handler
            .Script(TokenRoute, TokenReply())
            .Script(IntrospectionRoute, IntrospectionReply())
            .Script(LearnerRoute, LearnerReply());

        await ReadmeSamples.RunAsync("quickstart");

        Assert.Equal(["Ana Example in_progress"], readme.Printed);
        Assert.All(readme.Handler.Requests, request => Assert.Equal("sandbox.api.tme.so", request.Uri.Host));
    }
}
