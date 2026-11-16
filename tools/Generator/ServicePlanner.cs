using System.Text.Json;
using System.Text.RegularExpressions;

namespace TeachMeHipaa.Generator;

/// <summary>
/// Plans the generated services from the IR: the resource tree, each
/// method's options class, fields and headers, and the types the JSON
/// context needs. It checks every shape it has a rule for and throws on any
/// other; <see cref="ServiceWriter"/> turns the plan into C#.
/// </summary>
internal sealed partial class ServicePlanner
{
    private const string OrganizationPrefix = "/organizations/{orgId}";

    // The file name sent for a stream or bytes that bring none: the part's name plus this.
    private static readonly Dictionary<string, string> FileExtensions = new(StringComparer.Ordinal) { ["text/csv"] = ".csv" };

    /// <summary>
    /// Services a hand-written partial in src/TeachMeHipaa/Extensions extends,
    /// outside the directories the generator owns.
    /// </summary>
    internal static readonly HashSet<string> ExtendedServices = new(StringComparer.Ordinal) { "EventService" };

    private readonly TypeModel _model;
    private readonly ModelWriter _types;
    private readonly SortedDictionary<string, ResourceNode> _roots = new(StringComparer.Ordinal);
    private readonly List<string> _serialized = [];

    public ServicePlanner(IrDocument ir, TypeModel model, ModelWriter types)
    {
        _model = model;
        _types = types;
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
        var services = _roots.Values.SelectMany(root => root.Walk()).Select(node => node.ClassName).ToHashSet(StringComparer.Ordinal);
        if (ExtendedServices.FirstOrDefault(service => !services.Contains(service)) is { } missing)
        {
            throw new GeneratorException($"ExtendedServices names {missing}, which the IR no longer has");
        }

        var clash = generated.GroupBy(name => name, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1 || _model.Types.ContainsKey(group.Key));
        if (clash is not null)
        {
            throw new GeneratorException($"{clash.Key} is generated twice or collides with an IR type");
        }
    }

    /// <summary>The top-level resources, by path segment.</summary>
    public SortedDictionary<string, ResourceNode> Roots => _roots;

    /// <summary>Options classes sent as JSON and the pages returned, for the JSON context.</summary>
    public IEnumerable<string> SerializedTypes => _serialized;

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
            if (parameter.Type is not { Kind: TypeRefKind.Primitive, Type: "string" } || !parameter.Required)
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
            if (parameter.Type.Kind == TypeRefKind.Ref ? !_model.IsEnum(parameter.Type) : parameter.Type.Kind != TypeRefKind.Primitive)
            {
                throw new GeneratorException($"{operation}: query parameter {parameter.Name} has no rule");
            }

            plan.Fields.Add(new OptionField(parameter.Name, ModelWriter.Property(parameter.Name, plan.OptionsClass), parameter.Type, parameter.Required, false, parameter.Description, FieldKind.Query));
        }

        plan.IfMatch = method.IfMatch;
        plan.IfNoneMatch = method.IfNoneMatch switch
        {
            IrRequirement.None => false,
            IrRequirement.Optional when method.Response.Kind == IrResponseKind.Model => true,
            _ => throw new GeneratorException($"{operation}: no rule for ifNoneMatch {method.IfNoneMatch} on a {method.Response.Kind} response"),
        };

        var names = plan.Fields.Select(field => field.Property)
            .Concat(plan.IfMatch == IrRequirement.None ? [] : [MethodPlan.IfMatchProperty])
            .ToList();
        if (names.Distinct(StringComparer.Ordinal).Count() != names.Count)
        {
            throw new GeneratorException($"{operation}: options property names collide");
        }

        var parameters = method.PathParams.Select(parameter => Naming.Parameter(parameter.Name))
            .Concat(["ifNoneMatch", "options", "requestOptions", "cancellationToken"])
            .Concat(plan.Fields.Where(field => field.Kind == FieldKind.File).Select(field => field.Local))
            .ToList();
        if (parameters.Distinct(StringComparer.Ordinal).Count() != parameters.Count)
        {
            throw new GeneratorException($"{operation}: parameter names collide");
        }

        if (plan.Fields.Count == 0 && plan.IfMatch == IrRequirement.None)
        {
            plan.OptionsClass = null;
        }

        plan.OptionsRequired = (method.RequestBody?.Required ?? false)
            || method.QueryParams.Any(parameter => parameter.Required)
            || plan.IfMatch == IrRequirement.Required;
        if (plan.ContentType is "application/json" or "application/merge-patch+json")
        {
            _serialized.Add(plan.OptionsClass!);
        }

        // OptionalJsonConverter<T> looks up each patch field's type in the context.
        if (plan.ContentType == "application/merge-patch+json")
        {
            _serialized.AddRange(plan.Fields.Where(field => field.Kind == FieldKind.Body).Select(field => _types.PatchValueType(field.Type, field.Nullable, forTypeof: true)));
        }

        if (method.Response.Kind == IrResponseKind.Page)
        {
            var pagination = method.Pagination!;
            if (!plan.Fields.Any(field => field is { Kind: FieldKind.Query, Type: { Kind: TypeRefKind.Primitive, Type: "string" } } && field.Wire == pagination.CursorParam))
            {
                throw new GeneratorException($"{operation}: the cursor parameter {pagination.CursorParam} is not a string query parameter");
            }

            plan.PageItem = PageItem(method);
            _serialized.Add($"PageEnvelope<{plan.PageItem}>");
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
        if (bodyType.Kind != IrTypeKind.Object)
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
            if (constraint is { Kind: IrConstraintKind.Variant, Path.Count: 1, RequiredProperty: { } property })
            {
                var union = _model.Type(TypeModel.Name(top.Type));
                var variant = TypeModel.Discriminator(union).Mapping.GetValueOrDefault(property)
                    ?? throw new GeneratorException($"{operation}: {union.Name} has no member for {property}");
                narrowed[top.Name] = new TypeRef(TypeRefKind.Ref, null, variant, null, null);
            }
            else if (constraint is { Kind: IrConstraintKind.Const, Path.Count: 2, Value: { } value } && multipart)
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
            if ((binary is not null) != field.Type is { Kind: TypeRefKind.Primitive, Type: "binary" })
            {
                throw new GeneratorException($"{operation}: {field.Name} disagrees with the binary parts");
            }

            if (multipart && binary is null && (field.Type.Kind != TypeRefKind.Ref || _model.Type(field.Type.Name!).Kind != IrTypeKind.Object))
            {
                throw new GeneratorException($"{operation}: multipart field {field.Name} is not a file or an object");
            }

            if (binary is not null && !field.Required)
            {
                throw new GeneratorException($"{operation}: an optional file part has no rule");
            }

            if (binary is not null && !FileExtensions.ContainsKey(binary.ContentType))
            {
                throw new GeneratorException($"{operation}: no default file name for a {binary.ContentType} part");
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
                DefaultFileName = binary is null ? null : field.Name + FileExtensions[binary.ContentType],
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
        return item.Kind == TypeRefKind.Ref ? _types.CsType(item) : throw new GeneratorException($"{method.OperationId}: page items are not a model");
    }

    [GeneratedRegex(@"\{([^}]+)\}")]
    private static partial Regex Placeholder();
}

internal enum FieldKind
{
    Body,
    Query,
    File,
}

/// <summary>One property of a method's options class: a body field, a query parameter or a file.</summary>
internal sealed record OptionField(string Wire, string Property, TypeRef Type, bool Required, bool Nullable, string? Description, FieldKind Kind)
{
    public string? FileContentType { get; init; }

    /// <summary>The file name sent for a file part whose upload brings none.</summary>
    public string? DefaultFileName { get; init; }

    public List<(string Field, JsonElement Value)> Pins { get; init; } = [];

    /// <summary>The local a file part's bytes are read into before sending.</summary>
    public string Local => Naming.Parameter(Wire + "Contents");
}

/// <summary>What <see cref="ServiceWriter"/> emits for one IR method.</summary>
internal sealed class MethodPlan(IrMethod method, string name)
{
    public const string IfMatchProperty = "IfMatch";

    public IrMethod Method { get; } = method;

    public string Name { get; } = name;

    public string? OptionsClass { get; set; }

    public bool OptionsRequired { get; set; }

    public string PathExpression { get; set; } = "";

    public string? ContentType { get; set; }

    /// <summary>The IR's <c>ifMatch</c>.</summary>
    public IrRequirement IfMatch { get; set; } = IrRequirement.None;

    /// <summary>Whether the IR allows <c>If-None-Match</c>, which adds an <c>…IfModifiedAsync</c> sibling.</summary>
    public bool IfNoneMatch { get; set; }

    /// <summary>A list method's page item type, as C#.</summary>
    public string? PageItem { get; set; }

    public List<OptionField> Fields { get; } = [];
}

/// <summary>One IR resource: a service class, its methods and its sub-resources.</summary>
internal sealed partial class ResourceNode(IReadOnlyList<string> path)
{
    public IReadOnlyList<string> Path { get; } = path;

    public List<MethodPlan> Methods { get; } = [];

    public SortedDictionary<string, ResourceNode> Children { get; } = new(StringComparer.Ordinal);

    public string ClassName => Naming.SingularResource(Path) + "Service";

    /// <summary><c>widgets</c> → "Widgets"; <c>widgets.parts</c> → "Widget parts".</summary>
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

    [GeneratedRegex("[A-Z][a-z0-9]*")]
    private static partial Regex Words();
}
