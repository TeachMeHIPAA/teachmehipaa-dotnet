using System.Text.Json;

namespace TeachMeHipaa.ContractTests;

/// <summary>
/// Collects the request violations Prism reports in <c>sl-violations</c>, as
/// <c>METHOD url: location message</c>. Violations in Prism's own generated
/// responses are left out: they come from its example generator, not from
/// anything the SDK sent.
/// </summary>
public sealed class ViolationRecorder : DelegatingHandler
{
    private readonly List<string> _found = [];

    public ViolationRecorder()
        : base(new HttpClientHandler())
    {
    }

    public IReadOnlyList<string> Found
    {
        get
        {
            lock (_found)
            {
                return [.. _found];
            }
        }
    }

    public void Clear()
    {
        lock (_found)
        {
            _found.Clear();
        }
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.Headers.TryGetValues("sl-violations", out var headers))
        {
            var found = headers
                .SelectMany(header => JsonDocument.Parse(header).RootElement.EnumerateArray())
                .Select(violation => (
                    Location: violation.GetProperty("location").EnumerateArray().Select(part => part.ToString()).ToArray(),
                    Message: violation.GetProperty("message").GetString()))
                .Where(violation => violation.Location.FirstOrDefault() == "request")
                .Select(violation => $"{request.Method} {request.RequestUri}: {string.Join('.', violation.Location)} {violation.Message}");
            lock (_found)
            {
                _found.AddRange(found);
            }
        }

        return response;
    }
}
