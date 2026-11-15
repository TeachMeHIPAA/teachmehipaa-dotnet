using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TeachMeHipaa.ContractTests;

/// <summary>
/// Runs a Prism mock of the vendored developer spec for the whole suite.
/// </summary>
/// <remarks>
/// Prism serves static responses built from the spec's schemas and examples.
/// Neither <c>--errors</c> nor <c>--dynamic</c> is used: both make Prism fail
/// on its own generated responses (static strings ignore <c>pattern</c>;
/// dynamic numbers break OpenAPI 3.1 <c>exclusiveMinimum</c>), which says
/// nothing about the SDK. Prism still answers a request that breaks the spec
/// with a 4xx, which the SDK throws, and names each broken rule in the
/// <c>sl-violations</c> header, which <see cref="ViolationRecorder"/> collects.
/// </remarks>
public sealed class PrismMock : IAsyncLifetime, IDisposable
{
    private const string Prism = "@stoplight/prism-cli@5.16.0";
    private const string Spec = "openapi/openapi.developer.json";

    // The first run downloads Prism through npx.
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(180);

    private readonly StringBuilder _log = new();
    private Process? _prism;

    /// <summary>
    /// Prism serves the spec's paths without the server's <c>/v1</c> prefix,
    /// so the client's base URL is the bare host.
    /// </summary>
    public Uri BaseUrl { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var port = FreePort();
        var listening = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var start = new ProcessStartInfo("npx")
        {
            WorkingDirectory = RepoRoot(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in new[] { "--yes", Prism, "mock", Spec, "-h", "127.0.0.1", "-p", port.ToString(System.Globalization.CultureInfo.InvariantCulture) })
        {
            start.ArgumentList.Add(argument);
        }

        _prism = new Process { StartInfo = start, EnableRaisingEvents = true };
        DataReceivedEventHandler record = (_, line) =>
        {
            if (line.Data is null)
            {
                return;
            }

            lock (_log)
            {
                _log.AppendLine(line.Data);
            }

            if (line.Data.Contains("Prism is listening", StringComparison.Ordinal))
            {
                listening.TrySetResult();
            }
        };
        _prism.OutputDataReceived += record;
        _prism.ErrorDataReceived += record;
        _prism.Exited += (_, _) => listening.TrySetException(new InvalidOperationException($"Prism exited:\n{Log()}"));
        try
        {
            _prism.Start();
        }
        catch (System.ComponentModel.Win32Exception error)
        {
            throw new InvalidOperationException("The contract suite needs Node.js: npx is not on PATH.", error);
        }

        _prism.BeginOutputReadLine();
        _prism.BeginErrorReadLine();
        try
        {
            await listening.Task.WaitAsync(StartupTimeout);
        }
        catch (TimeoutException)
        {
            throw new InvalidOperationException($"Prism did not start within {StartupTimeout}:\n{Log()}");
        }

        BaseUrl = new Uri($"http://127.0.0.1:{port}");
    }

    public Task DisposeAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_prism is null)
        {
            return;
        }

        // npx runs Prism as a child process, so stop the whole tree.
        if (!_prism.HasExited)
        {
            _prism.Kill(entireProcessTree: true);
        }

        _prism.WaitForExit();
        _prism.Dispose();
        _prism = null;
    }

    private string Log()
    {
        lock (_log)
        {
            return _log.ToString();
        }
    }

    private static int FreePort()
    {
        // Releasing the port before Prism binds it is racy in theory, but
        // Prism binds within moments and the suite owns the machine in CI.
        using var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        return ((IPEndPoint)probe.LocalEndpoint).Port;
    }

    private static string RepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, Spec)))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException($"No {Spec} above {AppContext.BaseDirectory}.");
    }
}

[CollectionDefinition(Name)]
public sealed class PrismSuite : ICollectionFixture<PrismMock>
{
    public const string Name = "Prism";
}
