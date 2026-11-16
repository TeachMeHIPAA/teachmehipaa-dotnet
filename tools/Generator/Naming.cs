using System.Text;
using System.Text.RegularExpressions;

namespace TeachMeHipaa.Generator;

internal static partial class Naming
{
    // Every C# keyword, escaped with @ when an IR name is one.
    private static readonly HashSet<string> Keywords =
    [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
        "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit",
        "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int",
        "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out",
        "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
        "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try",
        "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
    ];

    /// <summary>
    /// PascalCase from a camelCase, snake_case, dotted or SHOUTING wire name:
    /// <c>not_started</c> → <c>NotStarted</c>, <c>enrollments:read</c> →
    /// <c>EnrollmentsRead</c>, <c>UNUSED</c> → <c>Unused</c>. Mixed-case words
    /// keep their capitals (<c>TeachMeHIPAA</c>).
    /// </summary>
    public static string Pascal(string name)
    {
        var builder = new StringBuilder();
        foreach (var word in WordSeparator().Split(name).Where(word => word.Length > 0))
        {
            var shouting = word.Length > 1 && word.Any(char.IsLetter) && word.Where(char.IsLetter).All(char.IsUpper);
            builder.Append(char.ToUpperInvariant(word[0]))
                .Append(shouting ? word[1..].ToLowerInvariant() : word[1..]);
        }

        var pascal = builder.ToString();
        if (pascal.Length == 0 || !char.IsLetter(pascal[0]))
        {
            throw new GeneratorException($"`{name}` has no C# name");
        }

        return pascal;
    }

    /// <summary>A parameter name: the IR's camelCase name, escaped if it is a keyword.</summary>
    public static string Parameter(string name)
    {
        if (!Identifier().IsMatch(name))
        {
            throw new GeneratorException($"`{name}` is not a C# parameter name");
        }

        return Keywords.Contains(name) ? "@" + name : name;
    }

    /// <summary>The singular of one resource path segment: drop a trailing "s".</summary>
    public static string Singular(string segment) =>
        segment.EndsWith('s') ? segment[..^1] : segment;

    /// <summary><c>["webhookSubscriptions", "deliveries"]</c> → <c>WebhookSubscriptionDelivery</c>.</summary>
    public static string SingularResource(IEnumerable<string> path) =>
        string.Concat(path.Select(segment => Pascal(Singular(segment))));

    /// <summary>The IR's CRUD verbs take Stripe .NET's names; custom verbs keep theirs.</summary>
    public static string MethodStem(string irName) => irName switch
    {
        "retrieve" => "Get",
        _ => Pascal(irName),
    };

    [GeneratedRegex("[^A-Za-z0-9]+")]
    private static partial Regex WordSeparator();

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex Identifier();
}

/// <summary>The IR has a shape this generator has no rule for.</summary>
public sealed class GeneratorException(string message) : Exception(message);
