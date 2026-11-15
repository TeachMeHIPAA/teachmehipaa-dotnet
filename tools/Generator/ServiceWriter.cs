using System.Text.Json;
using System.Text.RegularExpressions;

namespace TeachMeHipaa.Generator;

/// <summary>
/// One service class per IR resource, one options class per method that
/// takes parameters, and the client's service properties.
/// </summary>
internal sealed partial class ServiceWriter
{
    private const string OrganizationPrefix = "/organizations/{orgId}";

    private readonly TypeModel _model;
    private readonly ModelWriter _types;
    private readonly SortedDictionary<string, ResourceNode> _roots = new(StringComparer.Ordinal);
    private readonly List<string> _serialized = [];

    public ServiceWriter(IrDocument ir, TypeModel model)
    {
        _model = model;
        _types = new ModelWriter(model);
        foreach (var resource in ir.Resources)
        {
            var node = _roots.TryGetValue(resource.Path[0], out var root) ? root : _roots[resource.Path[0]] = new ResourceNode(resource.Path.Take(1).ToList());
            foreach (var depth in Enumerable.Range(1, resource.Path.Count - 1))
            {
                var segment = resource.Path[depth];
                node = node.Children.TryGetValue(segment, out var child)
                    ? child
                    : node.Children[segment] = new ResourceNode(resource.Path.Take(depth + 1).ToList());
            }

            node.Methods.AddRange(resource.Methods.Select(method => Plan(resource.Path, method)));
        }

        var generated = _roots.Values.SelectMany(root => root.Walk())
            .SelectMany(node => node.Methods.Select(method => method.OptionsClass).Append(node.ClassName))
            .OfType<string>()
            .ToList();
        var clash = generated.GroupBy(name => name, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1 || _model.Types.ContainsKey(group.Key));
        if (clash is not null)
        {
            throw new GeneratorException($"{clash.Key} is generated twice or collides with an IR type");
        }
    }

    /// <summary>Options classes sent as JSON and the pages returned, for the JSON context.</summary>
    public IEnumerable<string> SerializedTypes => _serialized;

    public IEnumerable<(string Name, string Contents)> Files()
    {
        foreach (var node in _roots.Values.SelectMany(root => root.Walk()))
        {
            yield return ($"{node.ClassName}.cs", ServiceFile(node));
            foreach (var method in node.Methods.Where(method => method.OptionsClass is not null))
            {
                yield return ($"{method.OptionsClass}.cs", OptionsFile(node, method));
            }
        }

        yield return ("TeachMeHipaaClient.Services.cs", ClientFile());
    }

    private MethodPlan Plan(IReadOnlyList<string> path, IrMethod method)
    {
        var operation = method.OperationId;
        var template = method.Path.Template;
        if (method.OrgScoped != template.StartsWith(OrganizationPrefix + "/", StringComparison.Ordinal)
            || (!method.OrgScoped && template.Contains("{orgId}", StringComparison.Ordinal)))
        {
            throw new GeneratorException($"{operation}: orgScoped disagrees with {template}");
        }

        if (method.HeaderParams.Count > 0)
        {
            throw new GeneratorException($"{operation}: header parameters have no rule");
        }

        var relative = method.OrgScoped ? template[OrganizationPrefix.Length..] : template;
        var placeholders = Placeholder().Matches(relative).Select(match => match.Groups[1].Value).ToList();
        if (!placeholders.SequenceEqual(method.PathParams.Select(parameter => parameter.Name)))
        {
            throw new GeneratorException($"{operation}: path parameters disagree with {template}");
        }

        foreach (var parameter in method.PathParams)
        {
            if (parameter.Type is not { Kind: "primitive", Type: "string" } || !parameter.Required)
            {
                throw new GeneratorException($"{operation}: path parameter {parameter.Name} is not a required string");
            }
        }

        var stem = Naming.MethodStem(method.Name);
        var plan = new MethodPlan(method, $"{stem}Async")
        {
            OptionsClass = $"{Naming.SingularResource(path)}{stem}Options",
            PathExpression = PathExpression(relative + (method.Path.CustomVerb is { } verb ? ":" + verb : "")),
        };

        if (method.RequestBody is { } body)
        {
            AddBodyFields(plan, body);
        }

        foreach (var parameter in method.QueryParams)
        {
            if (parameter.Type.Kind == "ref" ? !_model.IsEnum(parameter.Type) : parameter.Type.Kind != "primitive")
            {
                throw new GeneratorException($"{operation}: query parameter {parameter.Name} has no rule");
            }

            plan.Fields.Add(new OptionField(parameter.Name, ModelWriter.Property(parameter.Name, plan.OptionsClass), parameter.Type, parameter.Required, false, parameter.Description, FieldKind.Query));
        }

        var names = plan.Fields.Select(field => field.Property).ToList();
        if (names.Distinct(StringComparer.Ordinal).Count() != names.Count)
        {
            throw new GeneratorException($"{operation}: options property names collide");
        }

        var parameters = method.PathParams.Select(parameter => Naming.Parameter(parameter.Name))
            .Concat(["options", "requestOptions", "cancellationToken"]).ToList();
        if (parameters.Distinct(StringComparer.Ordinal).Count() != parameters.Count)
        {
            throw new GeneratorException($"{operation}: parameter names collide");
        }

        if (plan.Fields.Count == 0)
        {
            plan.OptionsClass = null;
        }

        plan.OptionsRequired = (method.RequestBody?.Required ?? false) || method.QueryParams.Any(parameter => parameter.Required);
        if (plan.ContentType is "application/json" or "application/merge-patch+json")
        {
            _serialized.Add(plan.OptionsClass!);
        }

        if (method.Response.Kind == "page")
        {
            _serialized.Add($"Page<{PageItem(method)}>");
        }

        return plan;
    }

    private void AddBodyFields(MethodPlan plan, IrRequestBody body)
    {
        var operation = plan.Method.OperationId;
        if (body.ContentType is not ("application/json" or "application/merge-patch+json" or "multipart/form-data"))
        {
            throw new GeneratorException($"{operation}: no rule for {body.ContentType} bodies");
        }

        var bodyType = _model.Type(TypeModel.Name(body.Type));
        if (bodyType.Kind != "object")
        {
            throw new GeneratorException($"{operation}: the body is not an object");
        }

        plan.ContentType = body.ContentType;
        var multipart = body.ContentType == "multipart/form-data";
        if (multipart && !body.Required)
        {
            throw new GeneratorException($"{operation}: an optional multipart body has no rule");
        }

        var narrowed = new Dictionary<string, TypeRef>(StringComparer.Ordinal);
        var pins = new Dictionary<string, List<(string Field, JsonElement Value)>>(StringComparer.Ordinal);
        foreach (var constraint in body.Constraints)
        {
            var top = TypeModel.Fields(bodyType).FirstOrDefault(field => field.Name == constraint.Path[0])
                ?? throw new GeneratorException($"{operation}: no body field {constraint.Path[0]}");
            if (constraint is { Kind: "variant", Path.Count: 1, RequiredProperty: { } property })
            {
                var union = _model.Type(TypeModel.Name(top.Type));
                var variant = TypeModel.Discriminator(union).Mapping.GetValueOrDefault(property)
                    ?? throw new GeneratorException($"{operation}: {union.Name} has no member for {property}");
                narrowed[top.Name] = new TypeRef("ref", null, variant, null, null);
            }
            else if (constraint is { Kind: "const", Path.Count: 2, Value: { } value } && multipart)
            {
                pins.TryAdd(top.Name, []);
                pins[top.Name].Add((constraint.Path[1], value));
            }
            else
            {
                throw new GeneratorException($"{operation}: no rule for a {constraint.Kind} constraint at {string.Join('.', constraint.Path)}");
            }
        }

        foreach (var field in TypeModel.Fields(bodyType))
        {
            var binary = body.BinaryParts.FirstOrDefault(part => part.Name == field.Name);
            var kind = binary is not null ? FieldKind.File : FieldKind.Body;
            if ((binary is not null) != field.Type is { Kind: "primitive", Type: "binary" })
            {
                throw new GeneratorException($"{operation}: {field.Name} disagrees with the binary parts");
            }

            if (multipart && binary is null && (field.Type.Kind != "ref" || _model.Type(field.Type.Name!).Kind != "object"))
            {
                throw new GeneratorException($"{operation}: multipart field {field.Name} is not a file or an object");
            }

            if (binary is not null && !field.Required)
            {
                throw new GeneratorException($"{operation}: an optional file part has no rule");
            }

            plan.Fields.Add(new OptionField(
                field.Name,
                ModelWriter.Property(field.Name, plan.OptionsClass!),
                narrowed.GetValueOrDefault(field.Name) ?? field.Type,
                field.Required,
                field.Nullable,
                field.Description,
                kind)
            {
                FileContentType = binary?.ContentType,
                Pins = pins.GetValueOrDefault(field.Name) ?? [],
            });
        }
    }

    private static string PathExpression(string path)
    {
        if (!path.Contains('{', StringComparison.Ordinal))
        {
            return Code.Literal(path);
        }

        return "$" + Code.Literal(Placeholder().Replace(path, match => $"{{Uri.EscapeDataString({Naming.Parameter(match.Groups[1].Value)})}}"));
    }

    private string PageItem(IrMethod method)
    {
        var item = method.Pagination?.ItemType ?? throw new GeneratorException($"{method.OperationId}: no item type");
        return item.Kind == "ref" ? _types.CsType(item) : throw new GeneratorException($"{method.OperationId}: page items are not a model");
    }

    private string ServiceFile(ResourceNode node) => Code.File([], code =>
    {
        code.Summary($"Calls the Developer API's {node.Title} endpoints.");
        code.Open($"public sealed class {node.ClassName}");
        code.Line("private readonly ApiRequestor _requestor;").Line();
        if (node.Children.Count == 0)
        {
            code.Line($"internal {node.ClassName}(ApiRequestor requestor) => _requestor = requestor;");
        }
        else
        {
            code.Open($"internal {node.ClassName}(ApiRequestor requestor)");
            code.Line("_requestor = requestor;");
            foreach (var (segment, child) in node.Children)
            {
                code.Line($"{Naming.Pascal(segment)} = new {child.ClassName}(requestor);");
            }

            code.Close();
            foreach (var (segment, child) in node.Children)
            {
                code.Line();
                code.Summary($"The Developer API's {child.Title} endpoints.");
                code.Line($"public {child.ClassName} {Naming.Pascal(segment)} {{ get; }}");
            }
        }

        foreach (var method in node.Methods.OrderBy(method => method.Name, StringComparer.Ordinal))
        {
            code.Line();
            Method(code, method);
        }

        code.Close();
    });

    private void Method(Code code, MethodPlan plan)
    {
        var method = plan.Method;
        var (returnType, call) = Response(plan);
        code.Summary(method.Summary is null ? $"Calls <c>{method.OperationId}</c>." : Code.Doc(method.Summary));
        if (method.Description is not null)
        {
            code.Line($"/// <remarks>{Code.Doc(method.Description)}</remarks>");
        }

        foreach (var parameter in method.PathParams)
        {
            var description = parameter.Description is null ? $"The <c>{parameter.Name}</c>." : Code.Doc(parameter.Description);
            code.Line($"/// <param name=\"{parameter.Name}\">{description}</param>");
        }

        if (plan.OptionsClass is not null)
        {
            code.Line("/// <param name=\"options\">The call's parameters.</param>");
        }

        code.Line("/// <param name=\"requestOptions\">Per-call settings, such as a request id or your own Idempotency-Key.</param>");
        code.Line("/// <param name=\"cancellationToken\">Cancels the call.</param>");
        code.Line($"/// <returns>{Returns(plan, returnType)}</returns>");
        code.Line("/// <exception cref=\"TeachMeHipaaException\">The API refused the request or could not be reached.</exception>");
        code.Line($"public Task<{returnType}> {plan.Name}(");
        code.Indent();
        foreach (var parameter in method.PathParams)
        {
            code.Line($"string {Naming.Parameter(parameter.Name)},");
        }

        if (plan.OptionsClass is not null)
        {
            code.Line(plan.OptionsRequired ? $"{plan.OptionsClass} options," : $"{plan.OptionsClass}? options = null,");
        }

        code.Line("RequestOptions? requestOptions = null,");
        code.Line("CancellationToken cancellationToken = default)");
        code.Outdent();
        code.Line("{");
        code.Indent();
        foreach (var parameter in method.PathParams)
        {
            var name = Naming.Parameter(parameter.Name);
            code.Open($"if (string.IsNullOrEmpty({name}))");
            code.Line($"throw new ArgumentException(\"A {parameter.Name} is required.\", nameof({name}));");
            code.Close();
            code.Line();
        }

        if (plan.OptionsClass is not null && plan.OptionsRequired)
        {
            code.Open("if (options is null)");
            code.Line("throw new ArgumentNullException(nameof(options));");
            code.Close();
            code.Line();
        }

        code.Line($"return _requestor.{call.Method}(");
        code.Indent();
        Request(code, plan);
        code.Line("requestOptions,");
        foreach (var argument in call.Arguments)
        {
            code.Line($"{argument},");
        }

        code.Line("cancellationToken);");
        code.Outdent();
        code.Close();
    }

    private void Request(Code code, MethodPlan plan)
    {
        var method = plan.Method;
        var initializers = new List<string>();
        if (method.OrgScoped)
        {
            initializers.Add("OrganizationScoped = true");
        }

        initializers.Add(method.Security.Kind switch
        {
            "bearer" => "",
            "none" => "Authenticated = false",
            _ => throw new GeneratorException($"{method.OperationId}: no rule for {method.Security.Kind} security"),
        });
        if (method.RetrySafe)
        {
            initializers.Add("RetrySafe = true");
        }

        initializers.Add(method.IdempotencyKey switch
        {
            "required" => "IdempotencyKeyRequired = true",
            // An optional key is the caller's to send through RequestOptions; it never makes the call retryable.
            "optional" or "none" => "",
            _ => throw new GeneratorException($"{method.OperationId}: no rule for idempotencyKey {method.IdempotencyKey}"),
        });
        if (method.Response.Kind == "binary")
        {
            initializers.Add($"Accept = {Code.Literal(method.Response.ContentType ?? throw new GeneratorException($"{method.OperationId}: a binary response without a content type"))}");
        }

        var access = plan.OptionsRequired ? "options." : "options?.";
        var query = plan.Fields.Where(field => field.Kind == FieldKind.Query).ToList();
        if (query.Count > 0)
        {
            var pairs = query.Select(field => $"new({Code.Literal(field.Wire)}, {QueryValue(field, access + field.Property)})");
            initializers.Add($"Query = [{string.Join(", ", pairs)}]");
        }

        var httpMethod = method.HttpMethod switch
        {
            "GET" => "HttpMethod.Get",
            "POST" => "HttpMethod.Post",
            "PUT" => "HttpMethod.Put",
            "DELETE" => "HttpMethod.Delete",
            // HttpMethod.Patch doesn't exist on netstandard2.0.
            "PATCH" => "new HttpMethod(\"PATCH\")",
            _ => throw new GeneratorException($"{method.OperationId}: no rule for {method.HttpMethod}"),
        };
        var constructor = $"new ApiRequest({httpMethod}, {plan.PathExpression})";
        var body = Body(plan);
        initializers.RemoveAll(initializer => initializer.Length == 0);
        if (initializers.Count == 0 && body.Count == 0)
        {
            code.Line(constructor + ",");
            return;
        }

        code.Line(constructor);
        code.Line("{");
        code.Indent();
        foreach (var initializer in initializers)
        {
            code.Line(initializer + ",");
        }

        foreach (var line in body)
        {
            code.Line(line);
        }

        code.Outdent();
        code.Line("},");
    }

    private List<string> Body(MethodPlan plan)
    {
        if (plan.ContentType is null)
        {
            return [];
        }

        if (plan.ContentType != "multipart/form-data")
        {
            var factory = plan.ContentType == "application/merge-patch+json" ? "MergePatch" : "Json";
            var serialized = $"RequestBody.{factory}(options, TeachMeHipaaJsonContext.Default.{plan.OptionsClass})";
            return [$"Body = {(plan.OptionsRequired ? serialized : $"options is null ? null : {serialized}")},"];
        }

        var parts = plan.Fields.Where(field => field.Kind != FieldKind.Query).Select(field =>
        {
            var value = "options." + field.Property;
            if (field.Kind == FieldKind.File)
            {
                return $"MultipartPart.File({Code.Literal(field.Wire)}, {value}, {Code.Literal(field.FileContentType!)})";
            }

            var typeInfo = $"TeachMeHipaaJsonContext.Default.{TypeModel.Name(field.Type)}";
            if (field.Pins.Count > 0)
            {
                var nested = _model.Type(TypeModel.Name(field.Type));
                var assignments = field.Pins.Select(pin => $"{ModelWriter.Property(pin.Field, nested.Name)} = {Literal(pin.Value)}");
                return $"MultipartPart.Json({Code.Literal(field.Wire)}, ({value} ?? new()) with {{ {string.Join(", ", assignments)} }}, {typeInfo})";
            }

            var part = $"MultipartPart.Json({Code.Literal(field.Wire)}, {value}, {typeInfo})";
            return field.Required && !field.Nullable ? part : $"{value} is null ? null : {part}";
        }).ToList();
        var lines = new List<string> { "Body = RequestBody.Multipart(" };
        lines.AddRange(parts.Select((part, index) => "    " + part + (index == parts.Count - 1 ? ")," : ",")));
        return lines;
    }

    private static string Literal(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.String => Code.Literal(value.GetString()!),
        JsonValueKind.Number => value.GetRawText(),
        _ => throw new GeneratorException($"no rule for a pinned {value.ValueKind}"),
    };

    private string QueryValue(OptionField field, string access)
    {
        var optional = field.Nullable || !field.Required;
        return field.Type switch
        {
            { Kind: "primitive", Type: "string" } => access,
            { Kind: "primitive", Type: "integer" or "dateTime" } => $"QueryValue.From({access})",
            { Kind: "ref" } when _model.IsEnum(field.Type) => access + (optional ? "?.Value" : ".Value"),
            _ => throw new GeneratorException($"no query rule for {field.Type.Kind} {field.Type.Type}"),
        };
    }

    private (string ReturnType, Call Call) Response(MethodPlan plan)
    {
        var method = plan.Method;
        var response = method.Response;
        switch (response.Kind)
        {
            case "model":
            case "operation":
                var type = _model.Type(TypeModel.Name(response.Type));
                if (type.Kind is not ("object" or "union"))
                {
                    throw new GeneratorException($"{method.OperationId}: returns a {type.Kind}");
                }

                return (type.Name, new Call("RequestAsync", [$"TeachMeHipaaJsonContext.Default.{type.Name}"]));
            case "page":
                var item = PageItem(method);
                return ($"Page<{item}>", new Call("RequestAsync", [$"TeachMeHipaaJsonContext.Default.Page{item}"]));
            case "writeResult":
                var result = _model.Type(TypeModel.Name(response.Type));
                if (!TypeModel.Fields(result).Any(field => field is { Name: "created", Type: { Kind: "primitive", Type: "boolean" } }))
                {
                    throw new GeneratorException($"write result {result.Name} has no boolean `created`");
                }

                return (result.Name, new Call(
                    "RequestWriteResultAsync",
                    [$"TeachMeHipaaJsonContext.Default.{result.Name}", "static (result, created) => result with { Created = created }"]));
            case "empty":
                return ("TeachMeHipaaResponse", new Call("RequestAsync", []));
            case "binary":
                return ("BinaryResponse", new Call("RequestBinaryAsync", []));
            default:
                throw new GeneratorException($"{method.OperationId}: no rule for {response.Kind} responses");
        }
    }

    private static string Returns(MethodPlan plan, string returnType) => plan.Method.Response.Kind switch
    {
        "page" => "One page of results, with <see cref=\"TeachMeHipaaEntity.LastResponse\"/> set.",
        "writeResult" => $"The <see cref=\"{returnType}\"/>, with <see cref=\"TeachMeHipaaEntity.LastResponse\"/> set; <c>Created</c> is true when this call created it (201) and false when it already existed (200).",
        "empty" => "What the API said about the response: status, headers and request id.",
        "binary" => "The file's bytes, content type and file name.",
        _ => $"The <see cref=\"{returnType}\"/>, with <see cref=\"TeachMeHipaaEntity.LastResponse\"/> set.",
    };

    private string OptionsFile(ResourceNode node, MethodPlan plan)
    {
        var json = plan.ContentType is "application/json" or "application/merge-patch+json";
        return Code.File(json ? ["System.Text.Json.Serialization"] : [], code =>
        {
            code.Summary($"Parameters for <see cref=\"{node.ClassName}.{plan.Name}\"/>.");
            if (plan.ContentType == "application/merge-patch+json")
            {
                code.Line("/// <remarks>Sent as a JSON merge patch. A property left null is left out, so the field stays unchanged; clearing a field is not supported yet.</remarks>");
            }

            code.Open($"public sealed class {plan.OptionsClass}");
            var first = true;
            foreach (var field in plan.Fields)
            {
                if (!first)
                {
                    code.Line();
                }

                first = false;
                var description = field.Description is null ? $"The <c>{field.Wire}</c> parameter." : Code.Doc(field.Description);
                code.Summary(field.Required ? description + " Required." : description);
                if (json)
                {
                    code.Line(field.Kind == FieldKind.Query ? "[JsonIgnore]" : $"[JsonPropertyName({Code.Literal(field.Wire)})]");
                }

                code.Line($"public {_types.Declaration(field.Type, field.Required, field.Nullable, field.Property, "get; set;", request: true)}");
            }

            code.Close();
        });
    }

    private string ClientFile() => Code.File([], code =>
    {
        code.Open("public sealed partial class TeachMeHipaaClient");
        code.Open("private TeachMeHipaaClient(ApiRequestor requestor)");
        foreach (var (segment, node) in _roots)
        {
            code.Line($"{Naming.Pascal(segment)} = new {node.ClassName}(requestor);");
        }

        code.Close();
        foreach (var (segment, node) in _roots)
        {
            code.Line();
            code.Summary($"The Developer API's {node.Title} endpoints.");
            code.Line($"public {node.ClassName} {Naming.Pascal(segment)} {{ get; }}");
        }

        code.Close();
    });

    [GeneratedRegex(@"\{([^}]+)\}")]
    private static partial Regex Placeholder();

    private enum FieldKind
    {
        Body,
        Query,
        File,
    }

    private sealed record OptionField(string Wire, string Property, TypeRef Type, bool Required, bool Nullable, string? Description, FieldKind Kind)
    {
        public string? FileContentType { get; init; }

        public List<(string Field, JsonElement Value)> Pins { get; init; } = [];
    }

    private sealed record Call(string Method, IReadOnlyList<string> Arguments);

    private sealed class MethodPlan(IrMethod method, string name)
    {
        public IrMethod Method { get; } = method;

        public string Name { get; } = name;

        public string? OptionsClass { get; set; }

        public bool OptionsRequired { get; set; }

        public string PathExpression { get; set; } = "";

        public string? ContentType { get; set; }

        public List<OptionField> Fields { get; } = [];
    }

    private sealed class ResourceNode(IReadOnlyList<string> path)
    {
        public IReadOnlyList<string> Path { get; } = path;

        public List<MethodPlan> Methods { get; } = [];

        public SortedDictionary<string, ResourceNode> Children { get; } = new(StringComparer.Ordinal);

        public string ClassName => Naming.SingularResource(Path) + "Service";

        /// <summary><c>webhookSubscriptions</c> → "Webhook subscriptions"; <c>partners.organizations</c> → "Partner organizations".</summary>
        public string Title
        {
            get
            {
                var segments = Path.Take(Path.Count - 1).Select(Naming.Singular).Append(Path[^1]);
                var words = segments.SelectMany(segment => Words().Matches(Naming.Pascal(segment)).Select(match => match.Value.ToLowerInvariant())).ToList();
                return char.ToUpperInvariant(words[0][0]) + string.Join(' ', words)[1..];
            }
        }

        public IEnumerable<ResourceNode> Walk() => Children.Values.SelectMany(child => child.Walk()).Prepend(this);
    }

    [GeneratedRegex("[A-Z][a-z0-9]*")]
    private static partial Regex Words();
}
