using System.Text;
using System.Text.RegularExpressions;

namespace TeachMeHipaa.Generator;

/// <summary>Builds one C# file, four spaces per level, LF line endings.</summary>
internal sealed partial class Code
{
    public const string Banner =
        "// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.\n" +
        "// Regenerate with `dotnet run --project tools/Generator`.\n";

    private readonly StringBuilder _text = new();
    private int _depth;

    public Code Line(string text = "")
    {
        if (text.Length == 0)
        {
            _text.Append('\n');
        }
        else
        {
            _text.Append(' ', _depth * 4).Append(text).Append('\n');
        }

        return this;
    }

    public Code Open(string header)
    {
        Line(header);
        Line("{");
        _depth++;
        return this;
    }

    public Code Close(string closing = "}")
    {
        _depth--;
        return Line(closing);
    }

    public Code Indent()
    {
        _depth++;
        return this;
    }

    public Code Outdent()
    {
        _depth--;
        return this;
    }

    public Code Summary(string text) => Line($"/// <summary>{text}</summary>");

    /// <summary>A whole file: banner, usings, file-scoped namespace, then the body.</summary>
    public static string File(IEnumerable<string> usings, Action<Code> body)
    {
        var code = new Code();
        code._text.Append(Banner).Append('\n');
        var sorted = usings.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        foreach (var name in sorted)
        {
            code.Line($"using {name};");
        }

        if (sorted.Count > 0)
        {
            code.Line();
        }

        code.Line("namespace TeachMeHipaa;").Line();
        body(code);
        return code._text.ToString();
    }

    /// <summary>
    /// IR prose as XML doc text: escaped, with <c>`code`</c> spans as
    /// <c>&lt;c&gt;</c> and a closing full stop.
    /// </summary>
    public static string Doc(string text)
    {
        var withCode = CodeSpan().Replace(Escape(text.Trim()), "<c>$1</c>");
        return withCode.EndsWith('.') || withCode.EndsWith('?') ? withCode : withCode + ".";
    }

    /// <summary>Text escaped for an XML doc comment.</summary>
    public static string Escape(string text) =>
        text.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);

    /// <summary>A C# string literal.</summary>
    public static string Literal(string value) =>
        "\"" + value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";

    [GeneratedRegex("`([^`]+)`")]
    private static partial Regex CodeSpan();
}
