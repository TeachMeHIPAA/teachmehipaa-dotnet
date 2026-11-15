using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace TeachMeHipaa.Tests.Support;

/// <summary>A reply the fake sends back for one scripted request.</summary>
public sealed record ScriptedReply
{
    public HttpStatusCode Status { get; init; } = HttpStatusCode.OK;

    public IReadOnlyDictionary<string, string> Headers { get; init; } =
        new Dictionary<string, string>();

    /// <summary>Raw body text. Sent as <c>application/json</c> unless a content type header is given.</summary>
    public string? Json { get; init; }

    /// <summary>Raw body text with no default content type.</summary>
    public string? Body { get; init; }
}

/// <summary>What the client put on the wire, captured when it was sent.</summary>
public sealed record RecordedRequest(
    HttpMethod Method,
    Uri Uri,
    IReadOnlyDictionary<string, string> Headers,
    string? Body)
{
    public string Route => $"{Method.Method} {Uri.AbsolutePath}";

    public string? Header(string name) => Headers.TryGetValue(name, out var value) ? value : null;
}

/// <summary>
/// The test adapter for the HTTP seam. Each route (<c>"GET /v1/auth/introspection"</c>)
/// has a queue of replies; every matching request consumes the next one, and an
/// unscripted request fails the test.
/// </summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<Func<RecordedRequest, CancellationToken, Task<ScriptedReply>>>> _scripts = new();
    private readonly ConcurrentQueue<RecordedRequest> _requests = new();

    public IReadOnlyList<RecordedRequest> Requests => _requests.ToArray();

    public FakeHttpMessageHandler Script(string route, params ScriptedReply[] replies)
    {
        foreach (var reply in replies)
        {
            Script(route, (_, _) => Task.FromResult(reply));
        }

        return this;
    }

    public FakeHttpMessageHandler Script(string route, Func<RecordedRequest, CancellationToken, Task<ScriptedReply>> step)
    {
        _scripts.GetOrAdd(route, _ => new()).Enqueue(step);
        return this;
    }

    public IReadOnlyList<RecordedRequest> RequestsTo(string route) =>
        Requests.Where(request => request.Route == route).ToArray();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var recorded = await RecordAsync(request);
        _requests.Enqueue(recorded);
        if (!_scripts.TryGetValue(recorded.Route, out var queue) || !queue.TryDequeue(out var step))
        {
            throw new InvalidOperationException($"Fake handler: unscripted request {recorded.Route}");
        }

        var reply = await step(recorded, cancellationToken);
        return ToResponse(reply, request);
    }

    private static async Task<RecordedRequest> RecordAsync(HttpRequestMessage request)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in request.Headers)
        {
            // HttpClient writes User-Agent products separated by spaces, not commas.
            var separator = string.Equals(header.Key, "User-Agent", StringComparison.OrdinalIgnoreCase) ? " " : ", ";
            headers[header.Key] = string.Join(separator, header.Value);
        }

        string? body = null;
        if (request.Content is not null)
        {
            foreach (var header in request.Content.Headers)
            {
                headers[header.Key] = string.Join(", ", header.Value);
            }

            body = await request.Content.ReadAsStringAsync();
        }

        return new RecordedRequest(request.Method, request.RequestUri!, headers, body);
    }

    private static HttpResponseMessage ToResponse(ScriptedReply reply, HttpRequestMessage request)
    {
        var text = reply.Json ?? reply.Body;
        var response = new HttpResponseMessage(reply.Status)
        {
            RequestMessage = request,
            Content = new ByteArrayContent(text is null ? [] : Encoding.UTF8.GetBytes(text)),
        };
        if (reply.Json is not null && !reply.Headers.Keys.Any(IsContentType))
        {
            response.Content.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        }

        foreach (var (name, value) in reply.Headers)
        {
            if (!response.Headers.TryAddWithoutValidation(name, value))
            {
                response.Content.Headers.TryAddWithoutValidation(name, value);
            }
        }

        return response;
    }

    private static bool IsContentType(string name) =>
        string.Equals(name, "Content-Type", StringComparison.OrdinalIgnoreCase);
}
