namespace TeachMeHipaa.Generator;

/// <summary>
/// What the generator derives from the IR's types: each union's members and
/// fallback, the page envelopes that become <c>Page&lt;T&gt;</c>, and the
/// fields a method pins (Q25 <c>const</c> constraints).
/// </summary>
internal sealed class TypeModel
{
    public TypeModel(IrDocument ir)
    {
        Ir = ir;
        Types = ir.Types.ToDictionary(type => type.Name, StringComparer.Ordinal);
        Methods = ir.Resources.SelectMany(resource => resource.Methods.Select(method => (resource.Path, method))).ToList();

        foreach (var (_, method) in Methods.Where(entry => entry.Method.Response.Kind == IrResponseKind.Page))
        {
            var page = Type(Name(method.Response.Type));
            var item = method.Pagination?.ItemType
                ?? throw new GeneratorException($"{method.OperationId}: a page response without pagination");
            CheckPage(page, item);
            PageItems[page.Name] = item;
        }

        foreach (var union in ir.Types.Where(type => type.Kind == IrTypeKind.Union))
        {
            var discriminator = Discriminator(union);
            foreach (var (tag, variant) in discriminator.Mapping.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                AddMember(union, variant);
                if (discriminator.Kind == IrDiscriminatorKind.Property)
                {
                    if (!Tags.TryAdd(variant, tag))
                    {
                        throw new GeneratorException($"{variant} has two tags in {union.Name}");
                    }

                    var field = Fields(Type(variant)).FirstOrDefault(f => f.Name == discriminator.Property)
                        ?? throw new GeneratorException($"{variant} lacks {union.Name}'s discriminator {discriminator.Property}");
                    if (field.Type is not { Kind: TypeRefKind.Primitive, Type: "string" })
                    {
                        throw new GeneratorException($"{variant}.{field.Name} is not a string discriminator");
                    }
                }
            }

            if (Fallback(union) is { } fallback)
            {
                if (GeneratesUnknown(union))
                {
                    if (Types.ContainsKey(fallback))
                    {
                        throw new GeneratorException($"{fallback} collides with an IR type");
                    }

                    UnknownVariants[fallback] = union;
                    UnionOf[fallback] = union.Name;
                }
                else
                {
                    AddMember(union, fallback);
                }
            }
        }

        CollectPins();

        // A request body's own type is flattened into its method's options
        // class, so it gets no model unless something else refers to it.
        var referenced = ResponseTypes();
        referenced.UnionWith(ir.Types.Where(type => type.Kind == IrTypeKind.Object).SelectMany(type => Fields(type).SelectMany(field => References(field.Type))));
        referenced.UnionWith(UnionOf.Keys);
        BodyOnly.UnionWith(Methods.Select(entry => entry.Method.RequestBody?.Type).OfType<TypeRef>().Select(Name).Where(name => !referenced.Contains(name)));
    }

    public IrDocument Ir { get; }

    public Dictionary<string, IrType> Types { get; }

    public List<(IReadOnlyList<string> Path, IrMethod Method)> Methods { get; }

    /// <summary>Page envelope name → item type. Envelopes become <c>Page&lt;T&gt;</c>, not models.</summary>
    public Dictionary<string, TypeRef> PageItems { get; } = new(StringComparer.Ordinal);

    /// <summary>Union member → its union, which is the member's base record.</summary>
    public Dictionary<string, string> UnionOf { get; } = new(StringComparer.Ordinal);

    /// <summary>Property-union variant → its discriminator value.</summary>
    public Dictionary<string, string> Tags { get; } = new(StringComparer.Ordinal);

    /// <summary><c>Unknown&lt;Union&gt;</c> fallbacks the generator adds, by name.</summary>
    public Dictionary<string, IrType> UnknownVariants { get; } = new(StringComparer.Ordinal);

    /// <summary>Type → fields every method sending it pins; they are hidden from callers.</summary>
    public Dictionary<string, HashSet<string>> Pinned { get; } = new(StringComparer.Ordinal);

    /// <summary>Request body types used only as bodies; their fields become options properties.</summary>
    public HashSet<string> BodyOnly { get; } = new(StringComparer.Ordinal);

    /// <summary>Every IR type that becomes a model.</summary>
    public IEnumerable<IrType> Models =>
        Types.Values.Where(type => !PageItems.ContainsKey(type.Name) && !BodyOnly.Contains(type.Name));

    /// <summary>Every generated model, by C# name.</summary>
    public IEnumerable<string> ModelNames => Models.Select(type => type.Name).Concat(UnknownVariants.Keys);

    public IrType Type(string name) =>
        Types.TryGetValue(name, out var type) ? type : throw new GeneratorException($"no IR type {name}");

    public static string Name(TypeRef? type) =>
        type is { Kind: TypeRefKind.Ref, Name: { } name } ? name : throw new GeneratorException("expected a type reference");

    public static IReadOnlyList<IrField> Fields(IrType type) =>
        type.Kind == IrTypeKind.Object && type.Fields is { } fields ? fields : throw new GeneratorException($"{type.Name} is not an object");

    public static IrDiscriminator Discriminator(IrType union) =>
        union.Discriminator ?? throw new GeneratorException($"{union.Name} has no discriminator");

    /// <summary>The model an unknown member of <paramref name="union"/> parses as, if any.</summary>
    public string? Fallback(IrType union)
    {
        var discriminator = Discriminator(union);
        if (discriminator.Kind == IrDiscriminatorKind.Presence)
        {
            return discriminator.Fallback;
        }

        return union.Name == Ir.Events.Envelope ? Ir.Events.UnknownEvent : $"Unknown{union.Name}";
    }

    public bool GeneratesUnknown(IrType union) =>
        Discriminator(union).Kind == IrDiscriminatorKind.Property && union.Name != Ir.Events.Envelope;

    /// <summary>
    /// The fields every member of <paramref name="union"/> declares the same
    /// way, apart from the discriminator, so the union's base record can
    /// expose them: an <c>Event</c>'s <c>eventId</c> without a type test.
    /// </summary>
    public IReadOnlyList<IrField> SharedFields(IrType union)
    {
        if (GeneratesUnknown(union))
        {
            // The generated Unknown member carries only the discriminator.
            return [];
        }

        var discriminator = Discriminator(union);
        var members = discriminator.Mapping.Values.Concat(Fallback(union) is { } fallback ? [fallback] : [])
            .Distinct(StringComparer.Ordinal)
            .Select(member => Fields(Type(member)))
            .ToList();
        return members[0]
            .Where(field => field.Name != discriminator.Property
                && members.All(fields => fields.Any(other => other with { Description = null } == field with { Description = null })))
            .Select(field => field with
            {
                Description = members.SelectMany(fields => fields).FirstOrDefault(other => other.Name == field.Name && other.Description is not null)?.Description,
            })
            .ToList();
    }

    public bool IsEnum(TypeRef type) => type.Kind == TypeRefKind.Ref && Type(Name(type)).Kind == IrTypeKind.Enum;

    public bool IsUnion(TypeRef type) => type.Kind == TypeRefKind.Ref && Type(Name(type)).Kind == IrTypeKind.Union;

    /// <summary>Every type a response can contain, through fields and union members.</summary>
    public HashSet<string> ResponseTypes()
    {
        var roots = Methods.SelectMany(entry => new[] { entry.Method.Response.Type, entry.Method.Pagination?.ItemType })
            .OfType<TypeRef>();
        var reached = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>(roots.SelectMany(References));
        while (pending.TryPop(out var name))
        {
            if (!reached.Add(name))
            {
                continue;
            }

            var type = Type(name);
            var next = type.Kind switch
            {
                IrTypeKind.Object => Fields(type).SelectMany(field => References(field.Type)),
                IrTypeKind.Union => Discriminator(type).Mapping.Values.Concat(Fallback(type) is { } fallback && Types.ContainsKey(fallback) ? [fallback] : []),
                _ => [],
            };
            foreach (var dependency in next)
            {
                pending.Push(dependency);
            }
        }

        return reached;
    }

    public static IEnumerable<string> References(TypeRef type) => type.Kind switch
    {
        TypeRefKind.Ref => [type.Name!],
        TypeRefKind.Array => References(type.Items!),
        TypeRefKind.Map => References(type.Values!),
        _ => [],
    };

    private void AddMember(IrType union, string member)
    {
        if (Type(member).Kind != IrTypeKind.Object)
        {
            throw new GeneratorException($"{union.Name} member {member} is not an object");
        }

        if (UnionOf.TryGetValue(member, out var other) && other != union.Name)
        {
            throw new GeneratorException($"{member} is a member of both {other} and {union.Name}");
        }

        UnionOf[member] = union.Name;
    }

    private static void CheckPage(IrType page, TypeRef item)
    {
        var fields = Fields(page);
        var shape = fields.Select(field => (field.Name, field.Required, field.Nullable)).Order().ToList();
        var items = fields.FirstOrDefault(field => field.Name == "items");
        if (!shape.SequenceEqual([("items", true, false), ("nextCursor", true, true)])
            || items?.Type is not { Kind: TypeRefKind.Array, Items: { } listed }
            || listed != item)
        {
            throw new GeneratorException($"page {page.Name} is not {{items, nextCursor}} of its item type");
        }
    }

    private void CollectPins()
    {
        var responseTypes = ResponseTypes();
        var sentBy = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var (_, method) in Methods)
        {
            if (method.RequestBody is not { } body)
            {
                continue;
            }

            var pinsHere = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            foreach (var constraint in body.Constraints.Where(constraint => constraint.Kind == IrConstraintKind.Const))
            {
                if (constraint.Path.Count != 2)
                {
                    throw new GeneratorException($"{method.OperationId}: no rule for const at {string.Join('.', constraint.Path)}");
                }

                var field = Fields(Type(Name(body.Type))).FirstOrDefault(f => f.Name == constraint.Path[0])
                    ?? throw new GeneratorException($"{method.OperationId}: no body field {constraint.Path[0]}");
                var nested = Name(field.Type);
                if (responseTypes.Contains(nested))
                {
                    throw new GeneratorException($"{method.OperationId}: pins {nested}, which responses also use");
                }

                var pinned = Fields(Type(nested)).FirstOrDefault(f => f.Name == constraint.Path[1])
                    ?? throw new GeneratorException($"{method.OperationId}: {nested} has no field {constraint.Path[1]}");
                if (pinned.Type.Kind != TypeRefKind.Primitive)
                {
                    throw new GeneratorException($"{method.OperationId}: pins a non-primitive {nested}.{pinned.Name}");
                }

                pinsHere.TryAdd(nested, new HashSet<string>(StringComparer.Ordinal));
                pinsHere[nested].Add(pinned.Name);
            }

            foreach (var field in Fields(Type(Name(body.Type))))
            {
                foreach (var name in References(field.Type))
                {
                    var pins = pinsHere.TryGetValue(name, out var set) ? set : [];
                    if (Pinned.TryGetValue(name, out var known) && !known.SetEquals(pins))
                    {
                        throw new GeneratorException($"{name} is pinned differently by {method.OperationId} and {string.Join(", ", sentBy[name])}");
                    }

                    Pinned[name] = pins;
                    sentBy.TryAdd(name, []);
                    sentBy[name].Add(method.OperationId);
                }
            }
        }

        foreach (var name in Pinned.Where(pair => pair.Value.Count == 0).Select(pair => pair.Key).ToList())
        {
            Pinned.Remove(name);
        }
    }
}
