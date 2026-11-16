using System.Text.RegularExpressions;

namespace TeachMeHipaa.Generator;

/// <summary>
/// Writes <c>docs/reference/resources.md</c>: every service and method of the
/// client, as C# signatures, from the same plan the services are written from,
/// so the reference can't drift from the code.
/// </summary>
internal sealed partial class ReferenceWriter(IrDocument ir, TypeModel model, ServiceWriter services)
{
    public const string Path = "docs/reference/resources.md";

    private const string Banner = "<!-- Generated from openapi/sdk-ir.json by tools/Generator. Do not edit. -->";

    // Signatures longer than this put each parameter on its own line.
    private const int LineLength = 100;

    /// <summary>
    /// What each hand-written partial in src/TeachMeHipaa/Extensions adds to a
    /// generated service; the IR doesn't know these methods.
    /// </summary>
    internal static readonly Dictionary<string, string> ExtensionDocs = new(StringComparer.Ordinal)
    {
        ["EventService"] =
            "`client.Events.TailAsync(...)` is hand-written: it reads the Event log from a saved position, " +
            "once or following new Events. See " +
            "[Pagination and Events](../guides/pagination-and-events.md#tailing-the-event-log).",
    };

    private readonly ModelWriter _types = new(model);

    public string Write()
    {
        if (ServicePlanner.ExtendedServices.FirstOrDefault(service => !ExtensionDocs.ContainsKey(service)) is { } undocumented)
        {
            throw new GeneratorException($"{undocumented} is extended but has no ExtensionDocs entry");
        }

        var nodes = services.Plan.Roots.Values.SelectMany(root => root.Walk()).ToList();
        var lines = new List<string>
        {
            Banner,
            "",
            "# Resource reference",
            "",
            "Every service and method of `TeachMeHipaaClient`, generated from the SDK IR of the Developer API " +
            $"document release `{ir.Source.Release}`.",
            "",
            "Every method is async and ends with two optional parameters, left out of the signatures below: " +
            "`RequestOptions? requestOptions = null`, for a request id, retries, a timeout or your own " +
            "`Idempotency-Key` (see [Errors and retries](../guides/errors-and-retries.md)), and " +
            "`CancellationToken cancellationToken = default`. Ids in the path come first, then the method's " +
            "options object, whose unset properties are not sent. Models, options and enums are in the " +
            "`TeachMeHipaa` namespace, and every object a method returns has a `LastResponse`.",
            "",
            "Organization-scoped paths contain `{orgId}`, which the client fills in from its Organization binding.",
            "",
            "## Services",
            "",
        };

        foreach (var node in nodes)
        {
            var dotted = Dotted(node);
            var methods = Sorted(node).Select(method => $"[`{method.Name}`](#{Anchor(dotted + method.Name)})");
            lines.Add($"- [`{dotted}`](#{Anchor(dotted)}): {string.Join(", ", methods)}");
        }

        foreach (var node in nodes)
        {
            var dotted = Dotted(node);
            lines.AddRange(["", $"## {dotted}", "", $"`client.{dotted}`: `{node.ClassName}`."]);
            if (ExtensionDocs.TryGetValue(node.ClassName, out var extension))
            {
                lines.AddRange(["", extension]);
            }

            foreach (var method in Sorted(node))
            {
                lines.Add("");
                lines.AddRange(Method(dotted, method));
            }
        }

        lines.Add("");
        lines.AddRange(Events());
        return string.Join("\n", lines) + "\n";
    }

    private static string Dotted(ResourceNode node) => string.Join(".", node.Path.Select(Naming.Pascal));

    private static IEnumerable<MethodPlan> Sorted(ResourceNode node) =>
        node.Methods.OrderBy(method => method.Name, StringComparer.Ordinal);

    /// <summary>GitHub's heading anchor: lowercase, without punctuation other than <c>-</c> and <c>_</c>.</summary>
    private static string Anchor(string heading) =>
        NotInAnchor().Replace(heading.ToLowerInvariant(), "").Replace(' ', '-');

    private List<string> Method(string dotted, MethodPlan plan)
    {
        var method = plan.Method;
        var returnType = services.ReturnType(plan);
        var stem = plan.Name[..^"Async".Length];
        var signatures = new List<string> { Signature($"Task<{returnType}>", plan.Name, Parameters(plan)) };
        if (plan.IfNoneMatch)
        {
            signatures.Add(Signature($"Task<ConditionalResult<{returnType}>>", stem + "IfModifiedAsync", Parameters(plan, ifNoneMatch: true)));
        }

        if (method.Response.Kind == IrResponseKind.Binary)
        {
            signatures.Add(Signature("Task<BinaryStreamResponse>", stem + "StreamingAsync", Parameters(plan)));
        }

        if (method.Response.Kind == IrResponseKind.Page)
        {
            signatures.Add(Signature($"IAsyncEnumerable<{plan.PageItem}>", stem + "AutoPagingAsync", Parameters(plan)));
        }

        var lines = new List<string>
        {
            $"### `{dotted}.{plan.Name}`",
            "",
            "```csharp",
            string.Join("\n", signatures),
            "```",
            "",
            $"`{method.HttpMethod} {method.Path.Template}{(method.Path.CustomVerb is { } verb ? ":" + verb : "")}`",
            "",
            Sentence(method.Summary ?? $"Calls `{method.OperationId}`"),
        };
        if (method.Description is not null)
        {
            lines.AddRange(["", Prose(method.Description)]);
        }

        lines.AddRange(["", $"- **Scopes:** {Scopes(method)}", $"- **Returns:** `{returnType}`"]);
        lines.AddRange(Notes(plan, stem).Select(note => "- " + note));
        if (plan.OptionsClass is not null)
        {
            lines.AddRange(["", $"`{plan.OptionsClass}`:", ""]);
            lines.AddRange(Options(plan));
        }

        return lines;
    }

    private static List<string> Parameters(MethodPlan plan, bool ifNoneMatch = false)
    {
        var parameters = plan.Method.PathParams.Select(parameter => $"string {Naming.Parameter(parameter.Name)}").ToList();
        if (ifNoneMatch)
        {
            parameters.Add("ETag ifNoneMatch");
        }

        if (plan.OptionsClass is not null)
        {
            parameters.Add(plan.OptionsRequired ? $"{plan.OptionsClass} options" : $"{plan.OptionsClass}? options = null");
        }

        return parameters;
    }

    private static string Signature(string returnType, string name, List<string> parameters)
    {
        var line = $"{returnType} {name}({string.Join(", ", parameters)});";
        return line.Length <= LineLength
            ? line
            : $"{returnType} {name}(\n" + string.Join(",\n", parameters.Select(parameter => "    " + parameter)) + ");";
    }

    private static string Scopes(IrMethod method)
    {
        if (method.Security.Kind == IrSecurityKind.None)
        {
            return "none: a public method, sent without an Access token";
        }

        var alternatives = (method.Security.AnyOf ?? []).Where(scopes => scopes.Count > 0).ToList();
        return alternatives.Count == 0
            ? "any Access token"
            : string.Join(" or ", alternatives.Select(scopes => string.Join(" and ", scopes.Select(scope => $"`{scope}`"))));
    }

    private static List<string> Notes(MethodPlan plan, string stem)
    {
        var method = plan.Method;
        var notes = new List<string>();
        notes.Add(method.IdempotencyKey switch
        {
            IrRequirement.Required => "Sends a generated `Idempotency-Key`, reused by its retries; set `RequestOptions.IdempotencyKey` to send your own.",
            IrRequirement.Optional => "Sends an `Idempotency-Key` only when you set `RequestOptions.IdempotencyKey`.",
            _ => "",
        });
        notes.Add(method.RetrySafe
            ? "Retried on network errors, 409 `conflict`, 429 and 5xx."
            : "Never retried: it is not safe to replay.");
        notes.Add(plan.IfMatch switch
        {
            IrRequirement.Required => "Requires an `ETag` in the options' constructor (the ETag string or the object you read), sent as `If-Match`; a stale one throws `PreconditionFailedException`.",
            IrRequirement.Optional => "Takes `IfMatch` (an ETag string or the object you read), sent as `If-Match`, to refuse the call if the object changed.",
            _ => "",
        });
        if (plan.IfNoneMatch)
        {
            notes.Add($"`{stem}IfModifiedAsync` sends `If-None-Match` and returns a `ConditionalResult` with no `Value` when your copy is current.");
        }

        if (plan.ContentType == "application/merge-patch+json")
        {
            notes.Add("A JSON merge patch: `Optional<T>` properties left unset are not sent, and set to `null` they clear the field.");
        }

        if (plan.Fields.Any(field => field.Kind == FieldKind.File))
        {
            notes.Add("Takes the file as a `FileUpload` in the options' constructor: a path, a `byte[]` or a `Stream`.");
        }

        notes.Add(method.Response.Kind switch
        {
            IrResponseKind.Page => $"`await foreach` over the page, or `{stem}AutoPagingAsync`, yields every item across pages.",
            IrResponseKind.Operation => "Returns an `Operation`: `WaitAsync` polls until it is done.",
            IrResponseKind.Binary => $"`{stem}StreamingAsync` returns once the headers arrive, to read the file as it comes.",
            IrResponseKind.WriteResult => "`Created` says whether this call created it.",
            _ => "",
        });
        return [.. notes.Where(note => note.Length > 0)];
    }

    private List<string> Options(MethodPlan plan)
    {
        var lines = new List<string>();
        if (plan.IfMatch != IrRequirement.None)
        {
            var how = plan.IfMatch == IrRequirement.Required ? "constructor argument, required" : "optional";
            lines.Add($"- `IfMatch` (`ETag`, {how}): The ETag you read the object with, or the object itself.");
        }

        foreach (var field in plan.Fields)
        {
            var patch = plan.ContentType == "application/merge-patch+json" && field.Kind == FieldKind.Body;
            var type = field.Kind == FieldKind.File ? "FileUpload"
                : patch ? $"Optional<{_types.PatchValueType(field.Type, field.Nullable)}>"
                : _types.CsType(field.Type, request: true) + (field.Nullable || !field.Required ? "?" : "");
            var how = field.Kind == FieldKind.File ? ", constructor argument" : field.Required ? ", required" : "";
            var description = field.Description is null ? "" : ": " + Sentence(field.Description);
            lines.Add($"- `{field.Property}` (`{type}`{how}){description}");
        }

        return lines;
    }

    private List<string> Events()
    {
        var envelope = model.Type(ir.Events.Envelope);
        var mapping = TypeModel.Discriminator(envelope).Mapping;
        var lines = new List<string>
        {
            "## Event types",
            "",
            "Events from `Events.ListAsync`, `Events.TailAsync` and `WebhookUtility.ConstructEvent` are " +
            $"`{envelope.Name}` subclasses: match on these. An Event type this version doesn't know arrives as " +
            $"`{ir.Events.UnknownEvent}`.",
            "",
            "| Event type | Class | `Data` |",
            "| --- | --- | --- |",
        };
        foreach (var entry in ir.Events.Catalog.OrderBy(entry => entry.Type, StringComparer.Ordinal))
        {
            var type = mapping.GetValueOrDefault(entry.Type)
                ?? throw new GeneratorException($"the Event type {entry.Type} has no class in {envelope.Name}");
            lines.Add($"| `{entry.Type}` | `{type}` | `{_types.CsType(entry.Data)}` |");
        }

        return lines;
    }

    private static string Prose(string text) => Whitespace().Replace(text.Trim(), " ");

    private static string Sentence(string text)
    {
        var prose = Prose(text);
        return prose.EndsWith('.') || prose.EndsWith('?') ? prose : prose + ".";
    }

    [GeneratedRegex(@"[^a-z0-9_\- ]")]
    private static partial Regex NotInAnchor();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
