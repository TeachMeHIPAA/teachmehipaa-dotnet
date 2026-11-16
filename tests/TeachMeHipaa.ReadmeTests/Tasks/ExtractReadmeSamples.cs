// An MSBuild task, compiled by RoslynCodeTaskFactory against netstandard, so
// it sticks to C# that needs nothing newer.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

/// <summary>
/// Writes every ```csharp block of the README and the guides into one C#
/// file the doc tests compile. Each block must follow a
/// <c>&lt;!-- sample: id --&gt;</c> line, with ids unique across all the
/// documents. A block's leading usings go to the top of the file, its
/// statements into a method named after the id, and any types it declares
/// after them (as in a top-level program) into the namespace. <c>#line</c>
/// directives point compiler errors at the document.
/// </summary>
/// <remarks>
/// Samples run against the test harness's fake transport, so the extractor
/// makes two edits, neither of which changes line numbers:
/// <list type="bullet">
/// <item>Variables a sample presumes, such as <c>client</c> or <c>app</c>,
/// are declared from <c>ReadmeHarness.Current</c> above its statements.</item>
/// <item>Each <c>new TeachMeHipaaClientOptions</c> initializer also sets
/// <c>HttpMessageHandler</c> to the fake, except in samples that use
/// <c>AddTeachMeHipaa</c>, whose harness configures the factory client, and
/// samples that bring their own <c>HttpMessageHandler</c> or
/// <c>HttpClient</c>.</item>
/// </list>
/// </remarks>
public sealed class ExtractReadmeSamples : Task
{
    private const string Harness = "global::TeachMeHipaa.ReadmeTests.ReadmeHarness";

    private static readonly Regex Tag = new Regex(@"^<!-- sample: ([a-z0-9-]+) -->$");
    private static readonly Regex UsingDirective = new Regex(@"^using [A-Za-z0-9_.]+;$");
    private static readonly Regex TypeDeclaration = new Regex(@"^(public |internal )?((sealed|static|abstract|partial) )*(class|record|struct|interface) ");
    private static readonly Regex OptionsInitializer = new Regex(@"new TeachMeHipaaClientOptions$");

    // Variables samples may use without declaring, and the harness property each comes from.
    private static readonly KeyValuePair<string, string>[] Presumed =
    {
        new KeyValuePair<string, string>("client", "Client"),
        new KeyValuePair<string, string>("builder", "Builder"),
        new KeyValuePair<string, string>("app", "App"),
        new KeyValuePair<string, string>("accessToken", "AccessToken"),
        new KeyValuePair<string, string>("signingSecrets", "SigningSecrets"),
    };

    [Required]
    public ITaskItem[] Documents { get; set; } = Array.Empty<ITaskItem>();

    [Required]
    public string Output { get; set; } = string.Empty;

    public override bool Execute()
    {
        var samples = new List<Sample>();
        var documents = Documents.Select(item => Path.GetFullPath(item.ItemSpec)).Distinct().OrderBy(path => path, StringComparer.Ordinal);
        foreach (var document in documents)
        {
            if (!Read(document, samples))
            {
                return false;
            }
        }

        if (Log.HasLoggedErrors)
        {
            return false;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(Output)));
        File.WriteAllText(Output, Render(samples));
        return true;
    }

    private bool Read(string document, List<Sample> samples)
    {
        var name = Path.GetFileName(document);
        var lines = File.ReadAllLines(document);
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "```csharp")
            {
                continue;
            }

            var tag = i > 0 ? Tag.Match(lines[i - 1]) : Match.Empty;
            var end = Array.FindIndex(lines, i + 1, line => line == "```");
            if (end < 0)
            {
                Log.LogError(null, null, null, document, i + 1, 0, 0, 0, "{0}: a ```csharp block is never closed.", name);
                return false;
            }

            if (!tag.Success)
            {
                Log.LogError(null, null, null, document, i + 1, 0, 0, 0, "{0}: a ```csharp block has no <!-- sample: id --> line above it.", name);
            }
            else if (samples.FirstOrDefault(sample => sample.Id == tag.Groups[1].Value) is Sample other)
            {
                Log.LogError(null, null, null, document, i, 0, 0, 0, "{0}: sample id '{1}' is already used in {2}.", name, tag.Groups[1].Value, Path.GetFileName(other.Document));
            }
            else
            {
                samples.Add(new Sample(tag.Groups[1].Value, document, i + 2, lines.Skip(i + 1).Take(end - i - 1).ToArray()));
            }

            i = end;
        }

        return true;
    }

    private string Render(List<Sample> samples)
    {
        var usings = new List<string>();
        var methods = new StringBuilder();
        var types = new StringBuilder();
        foreach (var sample in samples)
        {
            var code = Rewrite(sample);
            var firstStatement = code.TakeWhile(line => line.Length == 0 || UsingDirective.IsMatch(line)).Count();
            usings.AddRange(code.Take(firstStatement).Where(line => line.Length > 0));
            var firstType = FirstType(code, firstStatement);
            var statements = code.Skip(firstStatement).Take(firstType - firstStatement).ToArray();

            methods.AppendLine("    public static async Task " + MethodName(sample.Id) + "()");
            methods.AppendLine("    {");
            methods.AppendLine("        await Task.CompletedTask;");
            var body = string.Join("\n", statements);
            foreach (var presumed in Presumed)
            {
                if (Regex.IsMatch(body, @"\b" + presumed.Key + @"\b") && !Regex.IsMatch(body, @"\bvar " + presumed.Key + @"\b"))
                {
                    methods.AppendLine("        var " + presumed.Key + " = " + Harness + ".Current." + presumed.Value + ";");
                }
            }

            AppendMapped(methods, sample.Document, sample.FirstLine + firstStatement, statements);
            methods.AppendLine("    }");
            methods.AppendLine();
            AppendMapped(types, sample.Document, sample.FirstLine + firstType, code.Skip(firstType).ToArray());
        }

        var file = new StringBuilder();
        file.AppendLine("// <auto-generated/>");
        file.AppendLine("// Generated from README.md and docs/guides/*.md by tests/TeachMeHipaa.ReadmeTests/Tasks/ExtractReadmeSamples.cs. Do not edit.");
        file.AppendLine("#nullable enable");
        foreach (var directive in usings.Distinct())
        {
            file.AppendLine(directive);
        }

        file.AppendLine();
        file.AppendLine("namespace TeachMeHipaa.ReadmeTests;");
        file.AppendLine();
        file.AppendLine("public static partial class ReadmeSamples");
        file.AppendLine("{");
        file.AppendLine("    public static readonly string[] Ids = { " + string.Join(", ", samples.Select(sample => "\"" + sample.Id + "\"")) + " };");
        file.AppendLine();
        file.AppendLine("    public static Task RunAsync(string id) => id switch");
        file.AppendLine("    {");
        foreach (var sample in samples)
        {
            file.AppendLine("        \"" + sample.Id + "\" => " + MethodName(sample.Id) + "(),");
        }

        file.AppendLine("        _ => throw new ArgumentException(\"No doc sample \" + id, nameof(id)),");
        file.AppendLine("    };");
        file.AppendLine();
        file.Append(methods);
        file.AppendLine("}");
        file.AppendLine();
        file.Append(types);
        return file.ToString();
    }

    private static string[] Rewrite(Sample sample)
    {
        var injectTransport = !sample.Code.Any(line =>
            line.Contains("AddTeachMeHipaa") || line.Contains("HttpMessageHandler =") || line.Contains("HttpClient ="));
        return sample.Code
            .Select((line, index) =>
                injectTransport && line.Trim() == "{" && index > 0 && OptionsInitializer.IsMatch(sample.Code[index - 1].TrimEnd())
                    ? line.Replace("{", "{ HttpMessageHandler = " + Harness + ".Current.Handler,")
                    : line)
            .ToArray();
    }

    // Types follow the statements, as in a top-level program; a type's doc
    // comment and attributes belong to it.
    private static int FirstType(string[] code, int from)
    {
        var index = Array.FindIndex(code, from, line => TypeDeclaration.IsMatch(line));
        if (index < 0)
        {
            return code.Length;
        }

        while (index > from && (code[index - 1].StartsWith("///", StringComparison.Ordinal) || code[index - 1].StartsWith("[", StringComparison.Ordinal)))
        {
            index--;
        }

        return index;
    }

    private static void AppendMapped(StringBuilder builder, string document, int firstLine, string[] code)
    {
        if (code.All(line => line.Trim().Length == 0))
        {
            return;
        }

        builder.AppendLine("#line " + firstLine + " \"" + document + "\"");
        foreach (var line in code)
        {
            builder.AppendLine(line);
        }

        builder.AppendLine("#line default");
    }

    private static string MethodName(string id) =>
        string.Concat(id.Split('-').Select(part => char.ToUpperInvariant(part[0]) + part.Substring(1)));

    private sealed class Sample
    {
        public Sample(string id, string document, int firstLine, string[] code)
        {
            Id = id;
            Document = document;
            FirstLine = firstLine;
            Code = code;
        }

        public string Id { get; }

        public string Document { get; }

        // The document's line number of the block's first line of code.
        public int FirstLine { get; }

        public string[] Code { get; }
    }
}
