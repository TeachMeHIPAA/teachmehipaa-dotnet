using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

/// <summary>Enrollment CSV imports as multipart bodies, through the public client.</summary>
public sealed partial class EnrollmentImportTests : IDisposable
{
    private const string Route = $"POST /v1/organizations/{OrgId}/enrollments:import";

    // RFC 4180 with CRLF, a quoted comma and non-ASCII UTF-8.
    private const string CsvText = "email,name\r\nana@example.com,\"Example, Ana\"\r\njose@example.com,José Núñez\r\n";

    /// <summary>A <c>dev#/components/schemas/ImportPreview</c>.</summary>
    private const string PreviewJson = """
        {"totalRows":2,"validRows":2,"seatsRequired":2,"seatsAvailable":10,"rows":[
          {"rowNumber":2,"email":"ana@example.com","courseId":"hipaa-2026","status":"valid"},
          {"rowNumber":3,"email":"jose@example.com","courseId":"hipaa-2026","status":"valid"}]}
        """;

    /// <summary>A <c>dev#/components/schemas/Operation</c>, as a commit returns it (202).</summary>
    private const string OperationJson = """{"name":"op_01JD8H","kind":"enrollments.import","done":false}""";

    private static readonly byte[] Csv = Encoding.UTF8.GetBytes(CsvText);

    private static readonly ScriptedReply Accepted = new() { Status = HttpStatusCode.Accepted, Json = OperationJson };

    private readonly string _directory = Directory.CreateTempSubdirectory("tmh-import-").FullName;

    private string Roster
    {
        get
        {
            var path = Path.Combine(_directory, "roster.csv");
            File.WriteAllBytes(path, Csv);
            return path;
        }
    }

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    private static FakeHttpMessageHandler Api() => new FakeHttpMessageHandler().Script(TokenRoute, TokenReply());

    [Fact]
    public async Task APreviewSendsACsvPartAndAJsonOptionsPart()
    {
        var handler = Api().Script(Route, new ScriptedReply { Json = PreviewJson });
        var client = NewClient(handler);

        var preview = await client.Enrollments.PreviewImportAsync(
            new EnrollmentPreviewImportOptions(FileUpload.FromBytes(Csv)) { Options = new ImportOptions { CourseId = "hipaa-2026" } });

        Assert.Equal(2, preview.TotalRows);
        Assert.Equal(["ana@example.com", "jose@example.com"], preview.Rows.Select(row => row.Email));
        var parts = PartsOf(Assert.Single(handler.RequestsTo(Route)));
        Assert.Equal(["file", "options"], parts.Select(part => part.Name));
        Assert.Equal("text/csv", parts[0].ContentType);
        Assert.Equal("file.csv", parts[0].FileName);
        Assert.Equal(CsvText, parts[0].Content);
        Assert.Equal("application/json", parts[1].ContentType);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("""{"courseId":"hipaa-2026","validateOnly":true}"""), JsonNode.Parse(parts[1].Content)));
    }

    public static TheoryData<string, string> Sources => new()
    {
        { "a path string", "roster.csv" },
        { "FileUpload.FromPath", "roster.csv" },
        { "a byte array", "file.csv" },
        { "ReadOnlyMemory", "file.csv" },
        { "a MemoryStream", "file.csv" },
    };

    [Theory]
    [MemberData(nameof(Sources))]
    public async Task AnImportSendsTheBytesUnchangedWithAnIdempotencyKey(string source, string fileName)
    {
        var handler = Api().Script(Route, Accepted);
        var client = NewClient(handler);
        using var stream = new MemoryStream(Csv);
        var options = source switch
        {
            "a path string" => new EnrollmentImportOptions(Roster),
            "FileUpload.FromPath" => new EnrollmentImportOptions(FileUpload.FromPath(Roster)),
            "a byte array" => new EnrollmentImportOptions(Csv),
            "ReadOnlyMemory" => new EnrollmentImportOptions(FileUpload.FromBytes(new ReadOnlyMemory<byte>(Csv))),
            _ => new EnrollmentImportOptions(stream),
        };

        var operation = await client.Enrollments.ImportAsync(options);

        Assert.Equal("op_01JD8H", operation.Name);
        Assert.False(operation.Done);
        var request = Assert.Single(handler.RequestsTo(Route));
        Assert.Matches(UuidV4(), request.Header("Idempotency-Key"));
        var parts = PartsOf(request);
        Assert.Equal(fileName, parts[0].FileName);
        Assert.Equal("text/csv", parts[0].ContentType);
        Assert.Equal(CsvText, parts[0].Content);
        Assert.Equal("""{"validateOnly":false}""", parts[1].Content);
    }

    [Fact]
    public async Task SendsAnOpenFileUnderItsOwnNameAndLeavesItOpen()
    {
        var handler = Api().Script(Route, Accepted);
        var client = NewClient(handler);
        using var file = File.OpenRead(Roster);

        await client.Enrollments.ImportAsync(new EnrollmentImportOptions(file));

        var parts = PartsOf(Assert.Single(handler.RequestsTo(Route)));
        Assert.Equal("roster.csv", parts[0].FileName);
        Assert.Equal(CsvText, parts[0].Content);
        Assert.True(file.CanRead, "the caller's stream stays open");
    }

    [Fact]
    public async Task ARetriedImportResendsIdenticalBytesUnderTheSameKey()
    {
        var time = new TestTime();
        var handler = Api().Script(
            Route,
            ProblemReply(503, """{"title":"Service Unavailable","status":503,"code":"temporarily_unavailable"}"""),
            Accepted);
        var client = NewClient(handler, time: time);

        // A stream can be read only once, so this shows the file is read once per call.
        await time.RunAsync(client.Enrollments.ImportAsync(new EnrollmentImportOptions(new MemoryStream(Csv))));

        var requests = handler.RequestsTo(Route);
        Assert.Equal(2, requests.Count);
        Assert.Equal(requests[0].Header("Idempotency-Key"), requests[1].Header("Idempotency-Key"));
        Assert.Equal(CsvText, PartsOf(requests[0])[0].Content);
        Assert.Equal(CsvText, PartsOf(requests[1])[0].Content);
    }

    [Fact]
    public async Task RefusesCsvTextPassedAsAPathBeforeSending()
    {
        var handler = Api();
        var client = NewClient(handler);

        var error = await Assert.ThrowsAsync<ArgumentException>(
            () => client.Enrollments.PreviewImportAsync(new EnrollmentPreviewImportOptions(CsvText)));

        Assert.Contains("path", error.Message, StringComparison.Ordinal);
        Assert.Empty(handler.RequestsTo(Route));
    }

    [Fact]
    public async Task RefusesAMissingFileBeforeSending()
    {
        var handler = Api();
        var client = NewClient(handler);

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.Enrollments.ImportAsync(new EnrollmentImportOptions(FileUpload.FromBytes(Csv)) { File = null! }));

        Assert.Empty(handler.RequestsTo(Route));
    }

    /// <summary>The multipart parts in order, split the way a server would.</summary>
    private static List<Part> PartsOf(RecordedRequest request)
    {
        var contentType = MediaTypeHeaderValue.Parse(request.Header("Content-Type")!);
        Assert.Equal("multipart/form-data", contentType.MediaType);
        var boundary = contentType.Parameters.Single(parameter => parameter.Name == "boundary").Value!.Trim('"');
        var sections = request.Body!.Split("--" + boundary);
        Assert.StartsWith("--", sections[^1], StringComparison.Ordinal);
        return
        [
            .. sections[1..^1].Select(section =>
            {
                var split = section.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                var headers = section[2..split].Split("\r\n").ToDictionary(
                    line => line[..line.IndexOf(':', StringComparison.Ordinal)],
                    line => line[(line.IndexOf(':', StringComparison.Ordinal) + 1)..].Trim(),
                    StringComparer.OrdinalIgnoreCase);
                var disposition = ContentDispositionHeaderValue.Parse(headers["Content-Disposition"]);
                return new Part(
                    disposition.Name!.Trim('"'),
                    (disposition.FileNameStar ?? disposition.FileName)?.Trim('"'),
                    headers["Content-Type"].Split(';')[0],
                    section[(split + 4)..^2]);
            }),
        ];
    }

    [GeneratedRegex("^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$")]
    private static partial Regex UuidV4();

    private sealed record Part(string Name, string? FileName, string ContentType, string Content);
}
