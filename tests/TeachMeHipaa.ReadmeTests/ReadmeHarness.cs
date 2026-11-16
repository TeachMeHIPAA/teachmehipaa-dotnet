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
/// <c>accessToken</c>, <c>signingSecrets</c>), a captured console and a
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

    public CursorStore Cursors { get; } = new();

    public SigningSecretStore SigningSecrets { get; } = new();

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

/// <summary>Stands in for where an Event log follower keeps its position.</summary>
public sealed class CursorStore
{
    private readonly List<string?> _saved = [];
    private readonly TaskCompletionSource _enoughSaved = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public string? Cursor { get; set; }

    /// <summary>How many saves <see cref="EnoughSaved"/> waits for.</summary>
    public int Expected { get; set; } = int.MaxValue;

    public IReadOnlyList<string?> Saved
    {
        get
        {
            lock (_saved)
            {
                return [.. _saved];
            }
        }
    }

    public Task EnoughSaved => _enoughSaved.Task;

    public Task<string?> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Cursor);
    }

    public Task SaveAsync(string? eventId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_saved)
        {
            _saved.Add(eventId);
            if (_saved.Count >= Expected)
            {
                _enoughSaved.TrySetResult();
            }
        }

        return Task.CompletedTask;
    }
}

/// <summary>Stands in for the secret manager a Webhook subscription's Signing secret goes into.</summary>
public sealed class SigningSecretStore
{
    private readonly Dictionary<string, string> _secrets = [];

    public IReadOnlyDictionary<string, string> Secrets => _secrets;

    public Task SaveAsync(string subscriptionId, string secret, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _secrets[subscriptionId] = secret;
        return Task.CompletedTask;
    }
}
