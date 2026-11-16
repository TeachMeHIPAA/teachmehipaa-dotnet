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
            "The service also has a hand-written `client.Events.TailAsync(EventTailOptions, CancellationToken)`, " +
            "which reads the Event log from a saved position and can keep polling for new Events. See " +
            "[Read and follow the Event log](../articles/event-log.md#follow-the-log-from-a-background-service).",
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
            "# TeachMeHIPAA resource reference",
            "",
            "Each service on `TeachMeHipaaClient` and each of its methods: the C# signature, the HTTP call, " +
            "the Scopes the Access token needs and how the client retries the call. Generated from the SDK IR " +
            $"for Developer API document release `{ir.Source.Release}`.",
            "",
            "## Conventions",
            "",
            "- Every method also takes two optional trailing parameters, which the signatures below leave out: " +
            "`RequestOptions? requestOptions = null` sets a request id, a timeout, the retry count or your own " +
            "`Idempotency-Key` (see [Handle errors and retries](../articles/handle-errors.md)), and " +
            "`CancellationToken cancellationToken = default` cancels the call and its retries.",
            "- Path ids come first, then the options object. Options properties you leave unset aren't sent.",
            "- Models, options classes and enums live in the `TeachMeHipaa` namespace. Every object a method " +
            "returns exposes the HTTP response as `LastResponse`.",
            "- A path that contains `{orgId}` is Organization-scoped. The client fills the id in from its " +
            "Organization binding, so no method takes it.",
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
            lines.AddRange(["", $"## {dotted}", "", $"Reach it as `client.{dotted}`, an instance of `{node.ClassName}`."]);
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
            lines.AddRange(["", $"Options (`{plan.OptionsClass}`):", ""]);
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
            return "none; the method is public and sends no Access token";
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
            IrRequirement.Required => "**Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.",
            IrRequirement.Optional => "**Idempotency:** sends an `Idempotency-Key` only when you set `RequestOptions.IdempotencyKey`.",
            _ => "",
        });
        notes.Add(method.RetrySafe
            ? "**Retries:** on network errors, 409 `conflict`, 429 and 5xx."
            : "**Retries:** none, because replaying the call isn't safe.");
        notes.Add(plan.IfMatch switch
        {
            IrRequirement.Required => "**Concurrency:** the options constructor takes an `ETag` (the ETag string, or the object you read) and sends it as `If-Match`. A stale ETag throws `PreconditionFailedException`.",
            IrRequirement.Optional => "**Concurrency:** set `IfMatch` (the ETag string, or the object you read) to send `If-Match`, so the call fails if the object changed.",
            _ => "",
        });
        if (plan.IfNoneMatch)
        {
            notes.Add($"**Conditional read:** `{stem}IfModifiedAsync` sends `If-None-Match`. Its `ConditionalResult` has a `null` `Value` when your copy is current.");
        }

        if (plan.ContentType == "application/merge-patch+json")
        {
            notes.Add("**Body:** a JSON merge patch. Unset `Optional<T>` properties aren't sent, and a property set to `null` clears the field.");
        }

        if (plan.Fields.Any(field => field.Kind == FieldKind.File))
        {
            notes.Add("**File:** pass it to the options constructor as a `FileUpload`, from a path, a `byte[]` or a `Stream`.");
        }

        notes.Add(method.Response.Kind switch
        {
            IrResponseKind.Page => $"**Paging:** `await foreach` over the returned page, or over `{stem}AutoPagingAsync`, reads every item across pages.",
            IrResponseKind.Operation => "**Long-running:** returns an `Operation`. Call `WaitAsync` to poll until it finishes.",
            IrResponseKind.Binary => $"**Streaming:** `{stem}StreamingAsync` returns once the headers arrive, so you read the file as it downloads.",
            IrResponseKind.WriteResult => "**Created:** `Created` on the result is `true` when this call created the record.",
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
            "`Events.ListAsync`, `Events.TailAsync` and `WebhookUtility.ConstructEvent` return subclasses of " +
            $"`{envelope.Name}`, one per Event type. Match on the class with a type pattern. An Event type this " +
            $"version of the library doesn't know arrives as `{ir.Events.UnknownEvent}`.",
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
