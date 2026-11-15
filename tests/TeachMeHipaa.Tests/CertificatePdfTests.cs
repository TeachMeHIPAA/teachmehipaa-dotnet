using System.Net;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

/// <summary>Certificate PDF downloads, whole and streamed, through the public client.</summary>
public class CertificatePdfTests
{
    private const string CertificateId = "cert_01JD8F";
    private const string Route = $"GET /v1/organizations/{OrgId}/certificates/{CertificateId}/content";
    private const string Accept = "application/pdf, application/problem+json";

    // Every byte value, so any text decoding along the way would show.
    private static readonly byte[] EveryByte = [.. Enumerable.Range(0, 256).Select(value => (byte)value)];

    private static readonly ScriptedReply NotFound = ProblemReply(
        404,
        """{"title":"Not found","status":404,"code":"not_found","detail":"No such certificate."}""",
        new Dictionary<string, string> { ["x-request-id"] = "req_7f3c2a" });

    private static readonly ScriptedReply HtmlPage = new()
    {
        Body = "<!doctype html><title>Not found</title>",
        Headers = new Dictionary<string, string> { ["Content-Type"] = "text/html" },
    };

    private static ScriptedReply Pdf(string disposition = "attachment; filename=\"certificate-ana.pdf\"") => new()
    {
        Bytes = EveryByte,
        Headers = new Dictionary<string, string> { ["Content-Type"] = "application/pdf", ["Content-Disposition"] = disposition },
    };

    private static FakeHttpMessageHandler Api() => new FakeHttpMessageHandler().Script(TokenRoute, TokenReply());

    [Fact]
    public async Task ReturnsEveryByteUnchangedWithItsTypeAndFileName()
    {
        var handler = Api().Script(Route, Pdf());
        var client = NewClient(handler);

        var pdf = await client.Certificates.DownloadPdfAsync(CertificateId);

        Assert.Equal(EveryByte, pdf.Data);
        Assert.Equal("application/pdf", pdf.ContentType);
        Assert.Equal("certificate-ana.pdf", pdf.FileName);
        Assert.Equal(HttpStatusCode.OK, pdf.LastResponse?.StatusCode);
        Assert.Equal(Accept, Assert.Single(handler.RequestsTo(Route)).Header("Accept"));
    }

    [Fact]
    public async Task PrefersTheUtf8FileName()
    {
        var handler = Api().Script(Route, Pdf("attachment; filename=\"certificate-jose.pdf\"; filename*=UTF-8''certificate-jos%C3%A9.pdf"));
        var client = NewClient(handler);

        var pdf = await client.Certificates.DownloadPdfAsync(CertificateId);

        Assert.Equal("certificate-josé.pdf", pdf.FileName);
    }

    [Fact]
    public async Task RaisesTheTypedExceptionForAProblemResponse()
    {
        var handler = Api().Script(Route, NotFound);
        var client = NewClient(handler);

        var error = await Assert.ThrowsAsync<NotFoundException>(() => client.Certificates.DownloadPdfAsync(CertificateId));

        Assert.Equal("not_found", error.Code);
        Assert.Equal("req_7f3c2a", error.RequestId);
    }

    [Fact]
    public async Task DoesNotHandBackAnHtmlPageAsAPdf()
    {
        var handler = Api().Script(Route, HtmlPage);
        var client = NewClient(handler);

        await Assert.ThrowsAsync<ApiException>(() => client.Certificates.DownloadPdfAsync(CertificateId));
    }

    [Fact]
    public async Task StreamsEveryByteUnchangedWithItsTypeAndFileName()
    {
        var handler = Api().Script(Route, Pdf());
        var client = NewClient(handler);

        byte[] received;
        await using (var pdf = await client.Certificates.DownloadPdfStreamingAsync(CertificateId))
        {
            Assert.Equal("application/pdf", pdf.ContentType);
            Assert.Equal("certificate-ana.pdf", pdf.FileName);
            Assert.Equal(HttpStatusCode.OK, pdf.LastResponse.StatusCode);
            using var copy = new MemoryStream();
            await pdf.Content.CopyToAsync(copy);
            received = copy.ToArray();
        }

        Assert.Equal(EveryByte, received);
        Assert.Equal(Accept, Assert.Single(handler.RequestsTo(Route)).Header("Accept"));
    }

    public static TheoryData<bool> Transports => new() { false, true };

    [Theory]
    [MemberData(nameof(Transports))]
    public async Task HandsBackTheStreamBeforeTheWholeFileHasArrived(bool throughHttpClient)
    {
        using var body = new GatedStream(EveryByte, firstChunk: 16);
        var handler = Api().Script(Route, new ScriptedReply
        {
            BodyStream = body,
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/pdf" },
        });
        using var httpClient = new HttpClient(handler);
        var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            OrganizationId = OrgId,
            HttpMessageHandler = throughHttpClient ? null : handler,
            HttpClient = throughHttpClient ? httpClient : null,
        });

        var download = client.Certificates.DownloadPdfStreamingAsync(CertificateId);
        var first = await Task.WhenAny(download, Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.Same(download, first);
        await using var pdf = await download;
        var start = new byte[16];
        Assert.Equal(16, await pdf.Content.ReadAsync(start));
        Assert.Equal(EveryByte[..16], start);
        body.Open();
        using var rest = new MemoryStream();
        await pdf.Content.CopyToAsync(rest);
        Assert.Equal(EveryByte[16..], rest.ToArray());
    }

    [Fact]
    public async Task RetriesAStreamedDownloadUntilTheFileStarts()
    {
        var time = new TestTime();
        var handler = Api().Script(
            Route,
            ProblemReply(503, """{"title":"Service Unavailable","status":503,"code":"temporarily_unavailable"}"""),
            Pdf());
        var client = NewClient(handler, time: time);

        await using var pdf = await time.RunAsync(client.Certificates.DownloadPdfStreamingAsync(CertificateId));
        using var copy = new MemoryStream();
        await pdf.Content.CopyToAsync(copy);

        Assert.Equal(EveryByte, copy.ToArray());
        Assert.Equal(2, handler.RequestsTo(Route).Count);
    }

    [Fact]
    public async Task RaisesTheTypedExceptionForAStreamedProblemResponse()
    {
        var handler = Api().Script(Route, NotFound);
        var client = NewClient(handler);

        await Assert.ThrowsAsync<NotFoundException>(() => client.Certificates.DownloadPdfStreamingAsync(CertificateId));
    }

    [Fact]
    public async Task RaisesBeforeStreamingAnHtmlPage()
    {
        var handler = Api().Script(Route, HtmlPage);
        var client = NewClient(handler);

        await Assert.ThrowsAsync<ApiException>(() => client.Certificates.DownloadPdfStreamingAsync(CertificateId));
    }

    /// <summary>A body whose first bytes arrive at once and the rest only after <see cref="Open"/>.</summary>
    private sealed class GatedStream(byte[] data, int firstChunk) : Stream
    {
        private readonly TaskCompletionSource _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _position;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => _position; set => throw new NotSupportedException(); }

        public void Open() => _gate.TrySetResult();

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_position >= firstChunk)
            {
                await _gate.Task.WaitAsync(cancellationToken);
            }

            var end = _position < firstChunk ? firstChunk : data.Length;
            var count = Math.Min(buffer.Length, end - _position);
            data.AsMemory(_position, count).CopyTo(buffer);
            _position += count;
            return count;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
