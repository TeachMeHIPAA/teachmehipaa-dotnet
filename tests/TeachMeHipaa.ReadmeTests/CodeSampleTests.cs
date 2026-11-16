using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TeachMeHipaa.ReadmeTests;

/// <summary>
/// Compiles every sample in docs/reference/code-samples.json, the C# tab of
/// the API reference, as its own console program against the library, the
/// way a reader pastes it into <c>dotnet new console</c>. A sample that calls
/// a method or reads a property the library lacks fails here.
/// </summary>
public sealed class CodeSampleTests
{
    // The global usings a console project gets from <ImplicitUsings>.
    private const string ImplicitUsings = """
        global using System;
        global using System.Collections.Generic;
        global using System.IO;
        global using System.Linq;
        global using System.Net.Http;
        global using System.Threading;
        global using System.Threading.Tasks;
        """;

    // Assembly unification notes the .NET SDK silences in every project.
    private static readonly Dictionary<string, ReportDiagnostic> SdkNoWarn = new()
    {
        ["CS1701"] = ReportDiagnostic.Suppress,
        ["CS1702"] = ReportDiagnostic.Suppress,
    };

    private static readonly string Root = FindRoot();

    private static readonly JsonElement Document = JsonElement.Parse(File.ReadAllText(Path.Combine(Root, "docs", "reference", "code-samples.json")));

    private static readonly Lazy<MetadataReference[]> References = new(() =>
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path)),
        MetadataReference.CreateFromFile(typeof(TeachMeHipaaClient).Assembly.Location),
    ]);

    private static IEnumerable<string> SampleNames => Document.GetProperty("samples").EnumerateObject().Select(sample => sample.Name);

    public static TheoryData<string> OperationIds => [.. SampleNames];

    [Fact]
    public void NamesTheLanguageAndTheRelease()
    {
        Assert.Equal("csharp", Document.GetProperty("language").GetString());
        Assert.Equal(File.ReadAllText(Path.Combine(Root, "OPENAPI_VERSION")).Trim(), Document.GetProperty("release").GetString());
    }

    [Fact]
    public void CoversEveryOperationInTheIr()
    {
        using var ir = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root, "openapi", "sdk-ir.json")));
        var operations = ir.RootElement.GetProperty("resources").EnumerateArray()
            .SelectMany(resource => resource.GetProperty("methods").EnumerateArray())
            .Select(method => method.GetProperty("operationId").GetString()!)
            .Distinct()
            .Order(StringComparer.Ordinal);

        Assert.Equal(operations, SampleNames.Order(StringComparer.Ordinal));
    }

    [Theory]
    [MemberData(nameof(OperationIds))]
    public void SampleCompiles(string operationId)
    {
        var source = Document.GetProperty("samples").GetProperty(operationId).GetString()!;
        var parse = new CSharpParseOptions(LanguageVersion.Latest);
        var compilation = CSharpCompilation.Create(
            operationId,
            [
                CSharpSyntaxTree.ParseText(ImplicitUsings, parse, "GlobalUsings.cs"),
                CSharpSyntaxTree.ParseText(source, parse, $"{operationId}.cs"),
            ],
            References.Value,
            new CSharpCompilationOptions(
                OutputKind.ConsoleApplication,
                nullableContextOptions: NullableContextOptions.Enable,
                specificDiagnosticOptions: SdkNoWarn));

        using var output = new MemoryStream();
        var problems = compilation.Emit(output).Diagnostics
            .Where(diagnostic => diagnostic.Severity >= DiagnosticSeverity.Warning)
            .Select(diagnostic => diagnostic.ToString());

        Assert.Empty(problems);
    }

    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TeachMeHipaa.sln")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("TeachMeHipaa.sln not found above the test assembly.");
    }
}
