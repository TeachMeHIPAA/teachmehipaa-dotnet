using System.Text.RegularExpressions;

namespace TeachMeHipaa.ReadmeTests;

/// <summary>
/// Every relative link in the published Markdown points at a file that
/// exists, and at a heading that exists when it has an anchor, so moving a
/// guide or renaming a heading fails the build rather than a reader.
/// </summary>
public partial class DocLinkTests
{
    private static readonly string Root = FindRoot();

    public static TheoryData<string> Documents =>
    [
        "README.md",
        "CONTRIBUTING.md",
        "docs/README.md",
        "examples/README.md",
    ];

    [Theory]
    [MemberData(nameof(Documents))]
    public void RelativeLinksResolve(string document)
    {
        var path = Path.Combine(Root, document);
        var broken = new List<string>();
        foreach (Match link in Link().Matches(WithoutCode(File.ReadAllText(path))))
        {
            var target = link.Groups[1].Value;
            var hash = target.IndexOf('#', StringComparison.Ordinal);
            var file = hash < 0 ? target : target[..hash];
            var anchor = hash < 0 ? null : target[(hash + 1)..];
            var linked = file.Length == 0 ? path : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, file));
            var exists = File.Exists(linked) || Directory.Exists(linked);
            if (!exists || (anchor is not null && linked.EndsWith(".md", StringComparison.Ordinal) && !Anchors(File.ReadAllText(linked)).Contains(anchor)))
            {
                broken.Add(target);
            }
        }

        Assert.Empty(broken);
    }

    [Fact]
    public void AnchorsFollowGitHub()
    {
        Assert.Equal(
            ["5-use-the-client-from-aspnet-core", "learnerscreateasync", "conditional-updates-with-if-match", "audit_events"],
            Anchors("## 5. Use the client from ASP.NET Core\n### `Learners.CreateAsync`\n## Conditional updates with `If-Match`\n## audit_events"));
    }

    /// <summary>GitHub's heading anchors: lowercased, punctuation other than <c>-</c> and <c>_</c> dropped, spaces turned into hyphens.</summary>
    private static HashSet<string> Anchors(string markdown) =>
        Heading().Matches(WithoutCode(markdown))
            .Select(heading => NotInAnchor().Replace(heading.Groups[1].Value.Trim().ToLowerInvariant(), "").Replace(' ', '-'))
            .ToHashSet(StringComparer.Ordinal);

    // Fenced code holds C# such as `items[0](...)` and `# comments`, which aren't links or headings.
    private static string WithoutCode(string markdown) => FencedCode().Replace(markdown, "");

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

    [GeneratedRegex(@"\]\((?!https?:|mailto:|xref:)([^)\s]+)\)")]
    private static partial Regex Link();

    [GeneratedRegex(@"^#{1,6} (.+)$", RegexOptions.Multiline)]
    private static partial Regex Heading();

    [GeneratedRegex(@"[^\w\- ]")]
    private static partial Regex NotInAnchor();

    [GeneratedRegex(@"^```.*?^```", RegexOptions.Multiline | RegexOptions.Singleline)]
    private static partial Regex FencedCode();
}
