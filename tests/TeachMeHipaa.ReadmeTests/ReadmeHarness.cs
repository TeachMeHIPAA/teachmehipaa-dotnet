using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

// The samples share the console, the environment and the working directory.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TeachMeHipaa.ReadmeTests;

/// <summary>
/// What a doc sample runs against: the fake transport, the variables
/// samples presume (<c>client</c>, <c>builder</c>, <c>app</c>,
/// <c>accessToken</c>), a captured console and a
/// scratch working directory for the files samples read and write.
/// Generated sample code reaches it through <see cref="Current"/>.
/// </summary>
public sealed class ReadmeHarness : IAsyncDisposable
{
    public const string WebhookSecret = "readme-test-signing-secret";

    /// <summary>The token samples hand to <c>StaticTokenProvider</c>.</summary>
    public string AccessToken { get; } = "tmh_at_readme_static";

    private readonly TextWriter _console = Console.Out;
    private readonly StringWriter _output = new();
    private readonly string _workingDirectory = Directory.GetCurrentDirectory();
    private readonly string _scratch = Directory.CreateTempSubdirectory("readme-").FullName;
    private TeachMeHipaaClient? _client;
    private WebApplicationBuilder? _builder;
    private WebApplication? _app;

    public ReadmeHarness()
    {
        Current = this;
        Console.SetOut(TextWriter.Synchronized(_output));
        Directory.SetCurrentDirectory(_scratch);
        Environment.SetEnvironmentVariable("TMH_CLIENT_ID", ClientId);
        Environment.SetEnvironmentVariable("TMH_CLIENT_SECRET", ClientSecret);
        Environment.SetEnvironmentVariable("TMH_WEBHOOK_SECRET", WebhookSecret);
    }

    public static ReadmeHarness Current { get; private set; } = null!;

    public FakeHttpMessageHandler Handler { get; } = new();

    /// <summary>The Quickstart's client, bound to an Organization so no lookup is scripted.</summary>
    public TeachMeHipaaClient Client => _client ??= new TeachMeHipaaClient(new TeachMeHipaaClientOptions
    {
        ClientId = ClientId,
        ClientSecret = ClientSecret,
        Environment = TeachMeHipaaEnvironment.Sandbox,
        OrganizationId = OrgId,
        HttpMessageHandler = Handler,
    });

    /// <summary>An ASP.NET Core app builder served in memory, with the settings the samples read.</summary>
    public WebApplicationBuilder Builder => _builder ??= NewBuilder();

    public WebApplication App => _app ??= Builder.Build();

    public IReadOnlyList<string> Printed => _output.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }

        Console.SetOut(_console);
        Directory.SetCurrentDirectory(_workingDirectory);
        Directory.Delete(_scratch, recursive: true);
    }

    private static WebApplicationBuilder NewBuilder()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["TeachMeHipaa:ClientId"] = ClientId,
            ["TeachMeHipaa:ClientSecret"] = ClientSecret,
            ["TeachMeHipaa:WebhookSecret"] = WebhookSecret,
        });
        return builder;
    }
}
