using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

namespace TeachMeHipaa.Generator;

/// <summary>
/// Turns the vendored SDK IR into the library's generated layer: models,
/// services, their options and the JSON context, plus the Markdown resource
/// reference. Reads nothing but the IR and its schema, and produces the same
/// bytes for the same IR.
/// </summary>
public static class SurfaceGenerator
{
    public const string ModelsDirectory = "src/TeachMeHipaa/Models";
    public const string ServicesDirectory = "src/TeachMeHipaa/Services";

    /// <summary>The directories the generator owns: it deletes whatever it no longer writes there.</summary>
    public static IReadOnlyList<string> OwnedDirectories { get; } = [ModelsDirectory, ServicesDirectory];

    /// <summary>Single files the generator writes outside its directories.</summary>
    public static IReadOnlyList<string> OwnedFiles { get; } = [ReferenceWriter.Path];

    /// <summary>Repo-relative path → file contents, for every generated file.</summary>
    /// <exception cref="GeneratorException">The IR is invalid or has a shape with no rule here.</exception>
    public static SortedDictionary<string, string> Generate(string irJson, string schemaJson)
    {
        Validate(irJson, schemaJson);
        var ir = JsonSerializer.Deserialize(irJson, IrJsonContext.Default.IrDocument)
            ?? throw new GeneratorException("the IR is empty");
        if (ir.IrVersion != 1)
        {
            throw new GeneratorException($"irVersion {ir.IrVersion} is not 1");
        }

        var files = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var model = new TypeModel(ir);
        foreach (var (name, contents) in new ModelWriter(model).Files())
        {
            files.Add($"{ModelsDirectory}/{name}", contents);
        }

        var services = new ServiceWriter(ir, model);
        foreach (var (name, contents) in services.Files())
        {
            files.Add($"{ServicesDirectory}/{name}", contents);
        }

        files.Add($"{ModelsDirectory}/TeachMeHipaaJsonContext.cs", ContextFile(model, services));
        files.Add(ReferenceWriter.Path, new ReferenceWriter(ir, model, services).Write());
        return files;
    }

    private static void Validate(string irJson, string schemaJson)
    {
        var schema = JsonSchema.FromText(schemaJson);
        var results = schema.Evaluate(JsonNode.Parse(irJson), new EvaluationOptions { OutputFormat = OutputFormat.List });
        if (!results.IsValid)
        {
            var errors = results.Details
                .Where(detail => detail.Errors is not null)
                .SelectMany(detail => detail.Errors!.Select(error => $"{detail.InstanceLocation}: {error.Value}"))
                .Take(10);
            throw new GeneratorException("the IR does not match sdk-ir.schema.json:\n  " + string.Join("\n  ", errors));
        }
    }

    private static string ContextFile(TypeModel model, ServiceWriter services) =>
        Code.File(["System.Text.Json.Serialization"], code =>
        {
            code.Line("// Every type the generated services send or return; the core's wire");
            code.Line("// types have their own context (CoreJsonContext).");
            code.Line("[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]");
            var types = model.ModelNames.Concat(services.SerializedTypes).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);
            foreach (var type in types)
            {
                code.Line($"[JsonSerializable(typeof({type}))]");
            }

            code.Open("internal sealed partial class TeachMeHipaaJsonContext : JsonSerializerContext").Close();
        });
}
