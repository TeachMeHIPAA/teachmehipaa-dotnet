namespace TeachMeHipaa.Generator;

/// <summary>
/// One file per model: sealed records for objects, open-enum
/// <c>readonly record struct</c>s, and abstract records for unions with a
/// converter that never throws on an unknown member.
/// </summary>
internal sealed class ModelWriter(TypeModel model)
{
    private const string ExtensionDataDoc = "Fields this version of the library doesn't know yet, kept as sent.";

    // Members every record already has, which a generated property would hide.
    private static readonly HashSet<string> ReservedProperties = ["ExtensionData", "LastResponse", "EqualityContract"];

    // Models a hand-written partial in src/TeachMeHipaa/Extensions extends,
    // outside the directories the generator owns.
    private static readonly HashSet<string> ExtendedModels = new(StringComparer.Ordinal) { "Operation" };

    // Members every open enum already has.
    private static readonly HashSet<string> ReservedEnumValues = ["Value", "ToString", "Equals", "GetHashCode", "GetType"];

    public IEnumerable<(string Name, string Contents)> Files()
    {
        if (ExtendedModels.FirstOrDefault(name => !model.Types.TryGetValue(name, out var type) || type.Kind != IrTypeKind.Object) is { } missing)
        {
            throw new GeneratorException($"ExtendedModels names {missing}, which the IR no longer has as an object");
        }

        foreach (var type in model.Models)
        {
            yield return ($"{type.Name}.cs", type.Kind switch
            {
                IrTypeKind.Object => ObjectFile(type),
                IrTypeKind.Enum => EnumFile(type),
                IrTypeKind.Union => UnionFile(type),
                _ => throw new GeneratorException($"{type.Name}: no rule for {type.Kind} types"),
            });
        }

        foreach (var (name, union) in model.UnknownVariants)
        {
            yield return ($"{name}.cs", UnknownFile(name, union));
        }
    }

    /// <summary>The C# type of a model field (<paramref name="request"/>: of an options property).</summary>
    public string CsType(TypeRef type, bool request = false) => type.Kind switch
    {
        TypeRefKind.Primitive => type.Type switch
        {
            "string" => "string",
            "integer" => "long",
            "number" => "double",
            "boolean" => "bool",
            "dateTime" => "DateTimeOffset",
            "json" when !request => "JsonElement",
            "binary" when request => "FileUpload",
            _ => throw new GeneratorException($"no {(request ? "request" : "model")} type for {type.Type}"),
        },
        TypeRefKind.Ref when model.PageItems.ContainsKey(type.Name!) => throw new GeneratorException($"{type.Name} is a page envelope"),
        TypeRefKind.Ref => model.Type(type.Name!).Name,
        TypeRefKind.Array => $"IReadOnlyList<{CsType(type.Items!, request)}>",
        TypeRefKind.Map when !request => $"IReadOnlyDictionary<string, {CsType(type.Values!)}>",
        _ => throw new GeneratorException($"no {(request ? "request" : "model")} type for {type.Kind}"),
    };

    /// <summary>
    /// The <c>T</c> of a merge-patch field's <c>Optional&lt;T&gt;</c>: nullable
    /// only where the IR lets the field be cleared. <paramref name="forTypeof"/>
    /// drops a reference type's <c>?</c>, which <c>typeof</c> rejects.
    /// </summary>
    public string PatchValueType(TypeRef type, bool nullable, bool forTypeof = false) =>
        CsType(type, request: true) + (nullable && (!forTypeof || IsValueType(type)) ? "?" : "");

    /// <summary>
    /// A property declaration's type and initializer, by the field's presence
    /// rules. A <paramref name="mergePatch"/> field is an <c>Optional&lt;T&gt;</c>,
    /// so unset (left out) and null (cleared) are different values.
    /// </summary>
    public string Declaration(TypeRef type, bool required, bool nullable, string name, string accessors, bool request = false, bool mergePatch = false)
    {
        if (mergePatch)
        {
            return $"Optional<{PatchValueType(type, nullable)}> {name} {{ {accessors} }}";
        }

        var csType = CsType(type, request);
        if (nullable || !required)
        {
            return $"{csType}? {name} {{ {accessors} }}";
        }

        var initializer = type switch
        {
            { Kind: TypeRefKind.Primitive, Type: "string" } => "string.Empty",
            // A file has no empty default; its options class takes it in the constructor.
            { Kind: TypeRefKind.Primitive, Type: "binary" } => null,
            { Kind: TypeRefKind.Array } => "[]",
            { Kind: TypeRefKind.Map } => $"new Dictionary<string, {CsType(type.Values!)}>()",
            // A union is abstract; a missing one stays null, as the API never omits it.
            { Kind: TypeRefKind.Ref } when model.IsUnion(type) => "null!",
            { Kind: TypeRefKind.Ref } when !model.IsEnum(type) => "new()",
            _ => null,
        };
        return initializer is null ? $"{csType} {name} {{ {accessors} }}" : $"{csType} {name} {{ {accessors} }} = {initializer};";
    }

    private bool IsValueType(TypeRef type) => type switch
    {
        { Kind: TypeRefKind.Primitive, Type: "integer" or "number" or "boolean" or "dateTime" } => true,
        { Kind: TypeRefKind.Ref } => model.IsEnum(type),
        _ => false,
    };

    public static string Property(string wireName, string owner)
    {
        var name = Naming.Pascal(wireName);
        if (name == owner || ReservedProperties.Contains(name))
        {
            throw new GeneratorException($"{owner}.{wireName} would be named {name}, which C# or the base record reserves");
        }

        return name;
    }

    private static string Describe(string? description, string fallback) =>
        description is null ? fallback : Code.Doc(description);

    private string ObjectFile(IrType type) => Code.File(["System.Text.Json", "System.Text.Json.Serialization"], code =>
    {
        var fields = TypeModel.Fields(type);
        var baseType = model.UnionOf.TryGetValue(type.Name, out var union) ? union : "TeachMeHipaaEntity";
        var discriminator = union is null ? null : model.Type(union).Discriminator;
        var tag = model.Tags.GetValueOrDefault(type.Name);
        var pinned = model.Pinned.GetValueOrDefault(type.Name) ?? [];
        var shared = union is null ? [] : model.SharedFields(model.Type(union)).Select(field => field.Name).ToHashSet(StringComparer.Ordinal);

        code.Summary(Describe(type.Description, $"The API's <c>{type.Name}</c> object."));
        if (union is not null)
        {
            code.Line($"/// <remarks>One kind of <see cref=\"{union}\"/>.</remarks>");
        }

        code.Open($"public sealed {(ExtendedModels.Contains(type.Name) ? "partial " : "")}record {type.Name} : {baseType}");
        CheckUnique(type.Name, fields.Select(field => Property(field.Name, type.Name)));
        // Fields every member of the union shares live on the union's base record.
        foreach (var field in fields.Where(field => !shared.Contains(field.Name)))
        {
            var name = Property(field.Name, type.Name);
            if (tag is not null && field.Name == discriminator?.Property)
            {
                code.Summary($"Always <c>{tag}</c>.");
                code.Line($"[JsonPropertyName({Code.Literal(field.Name)})]");
                code.Line($"public string {name} {{ get; }} = {Code.Literal(tag)};");
            }
            else if (pinned.Contains(field.Name))
            {
                // Each method that sends this type sets the field itself (a Q25 const).
                code.Line("[JsonInclude]");
                code.Line($"[JsonPropertyName({Code.Literal(field.Name)})]");
                code.Line($"internal {Declaration(field.Type, field.Required, field.Nullable, name, "get; init;")}");
            }
            else
            {
                code.Summary(Describe(field.Description, $"The <c>{field.Name}</c> field."));
                code.Line($"[JsonPropertyName({Code.Literal(field.Name)})]");
                code.Line($"public {Declaration(field.Type, field.Required, field.Nullable, name, "get; init;")}");
            }

            code.Line();
        }

        ExtensionData(code);
        code.Close();
    });

    private static string UnknownFile(string name, IrType union) =>
        Code.File(["System.Text.Json", "System.Text.Json.Serialization"], code =>
        {
            var property = TypeModel.Discriminator(union).Property!;
            code.Summary($"A <see cref=\"{union.Name}\"/> of a kind this version of the library doesn't know. Its other fields are kept in <see cref=\"ExtensionData\"/>.");
            code.Open($"public sealed record {name} : {union.Name}");
            code.Summary("The kind, as the API sent it.");
            code.Line($"[JsonPropertyName({Code.Literal(property)})]");
            code.Line($"public string {Property(property, name)} {{ get; init; }} = string.Empty;");
            code.Line();
            ExtensionData(code);
            code.Close();
        });

    private static void ExtensionData(Code code)
    {
        code.Summary(ExtensionDataDoc);
        code.Line("[JsonExtensionData]");
        // A setter, not init: the source generator binds init-only properties
        // like constructor parameters, which extension data can't be.
        code.Line("public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();");
    }

    private static string EnumFile(IrType type) => Code.File(["System.Text.Json", "System.Text.Json.Serialization"], code =>
    {
        var values = type.Values ?? throw new GeneratorException($"{type.Name} has no values");
        var names = values.Select(value => Naming.Pascal(value)).ToList();
        CheckUnique(type.Name, names);
        foreach (var name in names.Where(name => name == type.Name || ReservedEnumValues.Contains(name)))
        {
            throw new GeneratorException($"{type.Name} value {name} collides with a member C# reserves");
        }

        code.Summary(Describe(type.Description, $"The API's <c>{type.Name}</c> values."));
        code.Line("/// <remarks>");
        code.Line("/// New values may appear at any time, so this holds any string; compare");
        code.Line("/// against the known values.");
        code.Line("/// </remarks>");
        code.Line($"[JsonConverter(typeof({type.Name}JsonConverter))]");
        code.Open($"public readonly record struct {type.Name}");
        code.Line("private readonly string? _value;").Line();
        code.Summary("Wraps a value, known or not.");
        code.Line("/// <param name=\"value\">The value as the API sends it.</param>");
        code.Line($"public {type.Name}(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));");
        foreach (var (value, name) in values.Zip(names))
        {
            code.Line();
            code.Summary($"<c>{Code.Escape(value)}</c>.");
            code.Line($"public static {type.Name} {name} {{ get; }} = new({Code.Literal(value)});");
        }

        code.Line();
        code.Summary("The value as the API sends it.");
        code.Line("public string Value => _value ?? string.Empty;").Line();
        code.Line("/// <inheritdoc />");
        code.Line("public override string ToString() => Value;");
        code.Close();
        code.Line();
        code.Open($"internal sealed class {type.Name}JsonConverter : JsonConverter<{type.Name}>");
        code.Line($"public override {type.Name} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>");
        code.Indent().Line($"new(reader.GetString() ?? throw new JsonException(\"A {type.Name} must be a string.\"));").Outdent();
        code.Line();
        code.Line($"public override void Write(Utf8JsonWriter writer, {type.Name} value, JsonSerializerOptions options) =>");
        code.Indent().Line("writer.WriteStringValue(value.Value);").Outdent();
        code.Close();
    });

    private string UnionFile(IrType union) => Code.File(["System.Text.Json", "System.Text.Json.Serialization"], code =>
    {
        var discriminator = TypeModel.Discriminator(union);
        var mapping = discriminator.Mapping.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToList();
        var fallback = model.Fallback(union);
        var members = mapping.Select(pair => pair.Value).Distinct().ToList();
        if (fallback is not null)
        {
            members.Add(fallback);
        }

        code.Summary(Describe(union.Description, $"One of the API's kinds of <c>{union.Name}</c>."));
        code.Line("/// <remarks>");
        code.Line($"/// Known kinds: {string.Join(", ", members.Select(member => $"<see cref=\"{member}\"/>"))}.");
        code.Line(discriminator.Kind == IrDiscriminatorKind.Property
            ? $"/// Chosen by <c>{discriminator.Property}</c>; any other value reads as <see cref=\"{fallback}\"/>."
            : fallback is null
                ? "/// Chosen by which of their fields is present."
                : $"/// Chosen by which of their fields is present, else <see cref=\"{fallback}\"/>.");
        code.Line("/// </remarks>");
        code.Line($"[JsonConverter(typeof({union.Name}JsonConverter))]");
        code.Open($"public abstract record {union.Name} : TeachMeHipaaEntity");
        code.Open($"private protected {union.Name}()").Close();
        foreach (var field in model.SharedFields(union))
        {
            code.Line();
            code.Summary(Describe(field.Description, $"The <c>{field.Name}</c> field."));
            code.Line($"[JsonPropertyName({Code.Literal(field.Name)})]");
            code.Line($"public {Declaration(field.Type, field.Required, field.Nullable, Property(field.Name, union.Name), "get; init;")}");
        }

        code.Close();
        code.Line();

        // STJ's own polymorphism throws on an unknown discriminator, so the
        // union reads its members by hand.
        code.Open($"internal sealed class {union.Name}JsonConverter : JsonConverter<{union.Name}>");
        code.Open($"public override {union.Name}? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)");
        code.Line("using var document = JsonDocument.ParseValue(ref reader);");
        code.Line("var json = document.RootElement;");
        if (discriminator.Kind == IrDiscriminatorKind.Property)
        {
            var property = Code.Literal(discriminator.Property!);
            code.Line($"var tag = json.ValueKind == JsonValueKind.Object && json.TryGetProperty({property}, out var value) && value.ValueKind == JsonValueKind.String");
            code.Indent().Line("? value.GetString()").Line(": null;").Outdent();
            code.Line("return tag switch");
            code.Line("{");
            code.Indent();
            foreach (var (tag, variant) in mapping)
            {
                code.Line($"{Code.Literal(tag)} => json.Deserialize(TeachMeHipaaJsonContext.Default.{variant}),");
            }

            code.Line($"_ => json.Deserialize(TeachMeHipaaJsonContext.Default.{fallback}),");
            code.Outdent();
            code.Line("};");
        }
        else
        {
            foreach (var (marker, variant) in mapping)
            {
                code.Open($"if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty({Code.Literal(marker)}, out _))");
                code.Line($"return json.Deserialize(TeachMeHipaaJsonContext.Default.{variant});");
                code.Close();
                code.Line();
            }

            code.Line(fallback is null
                ? $"throw new JsonException(\"A {union.Name} has none of {string.Join(", ", mapping.Select(pair => pair.Key))}.\");"
                : $"return json.Deserialize(TeachMeHipaaJsonContext.Default.{fallback});");
        }

        code.Close();
        code.Line();
        code.Open($"public override void Write(Utf8JsonWriter writer, {union.Name} value, JsonSerializerOptions options)");
        code.Open("switch (value)");
        foreach (var member in members)
        {
            code.Line($"case {member} member:");
            code.Indent();
            code.Line($"JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.{member});");
            code.Line("break;");
            code.Outdent();
        }

        code.Close();
        code.Close();
        code.Close();
    });

    private static void CheckUnique(string owner, IEnumerable<string> names)
    {
        var duplicate = names.GroupBy(name => name, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new GeneratorException($"{owner} has two members named {duplicate.Key}");
        }
    }
}
