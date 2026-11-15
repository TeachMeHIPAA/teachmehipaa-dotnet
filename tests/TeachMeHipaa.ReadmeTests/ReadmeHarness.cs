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
/// What a README sample runs against: the fake transport, the variables
/// samples presume (<c>client</c>, <c>builder</c>, <c>app</c>, <c>grants</c>),
/// a captured console and a scratch working directory for the files samples
/// read and write. Generated sample code reaches it through <see cref="Current"/>.
/// </summary>
public sealed class ReadmeHarness : IAsyncDisposable
{
    public const string WebhookSecret = "readme-test-signing-secret";

    private readonly TextWriter _console = Console.Out;
    private readonly StringWriter _output = new();
    private readonly string _workingDirectory = Directory.GetCurrentDirectory();
    private readonly string _scratch = Directory.CreateTempSubdirectory("readme-").FullName;
    private readonly HttpClient _httpClient;
    private TeachMeHipaaClient? _client;
    private WebApplicationBuilder? _builder;
    private WebApplication? _app;

    public ReadmeHarness()
    {
        _httpClient = new HttpClient(Handler, disposeHandler: false);
        Current = this;
        Console.SetOut(TextWriter.Synchronized(_output));
        Directory.SetCurrentDirectory(_scratch);
        Environment.SetEnvironmentVariable("TMH_CLIENT_ID", ClientId);
        Environment.SetEnvironmentVariable("TMH_CLIENT_SECRET", ClientSecret);
        Environment.SetEnvironmentVariable("TMH_APP_ID", AppId);
        Environment.SetEnvironmentVariable("TMH_APP_SECRET", AppSecret);
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

    public ConsentSessions Sessions { get; } = new();

    public GrantDatabase Grants { get; } = new();

    public CursorStore Cursors { get; } = new();

    public IReadOnlyList<string> Printed => _output.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);

    /// <summary>Stands in for <see cref="TeachMeHipaaOAuth.ExchangeCodeAsync"/>, sending through the fake.</summary>
    public static Task<GrantTokens> ExchangeCodeAsync(ExchangeCodeOptions options, CancellationToken cancellationToken = default) =>
        TeachMeHipaaOAuth.ExchangeCodeAsync(options, Current._httpClient, cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }

        _httpClient.Dispose();
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
        });
        return builder;
    }
}

/// <summary>Stands in for the admin's server-side session in the consent sample.</summary>
public sealed class ConsentSessions
{
    private (string State, string CodeVerifier)? _saved;

    public void Save(string state, string codeVerifier) => _saved = (state, codeVerifier);

    public (string State, string CodeVerifier) Load() =>
        _saved ?? throw new InvalidOperationException("No consent is in progress.");
}

/// <summary>Stands in for the Partner's database of Grants' Refresh tokens.</summary>
public sealed class GrantDatabase
{
    public Dictionary<string, string> RefreshTokens { get; } = [];

    public Task<string> LoadAsync(string orgId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(RefreshTokens[orgId]);
    }

    public Task SaveAsync(string orgId, string refreshToken, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RefreshTokens[orgId] = refreshToken;
        return Task.CompletedTask;
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
