using TeachMeHipaa.Generator;

// dotnet run --project tools/Generator            rewrites the generated files
// dotnet run --project tools/Generator -- --check  fails if they are stale
var check = args.Contains("--check");
if (args.Any(argument => argument != "--check"))
{
    Console.Error.WriteLine("usage: dotnet run --project tools/Generator [-- --check]");
    return 2;
}

var root = RepoRoot.Find();
SortedDictionary<string, string> files;
try
{
    files = SurfaceGenerator.Generate(
        File.ReadAllText(Path.Combine(root, "openapi", "sdk-ir.json")),
        File.ReadAllText(Path.Combine(root, "openapi", "sdk-ir.schema.json")));
}
catch (GeneratorException error)
{
    Console.Error.WriteLine($"generator: {error.Message}");
    return 1;
}

var current = RepoRoot.GeneratedFiles(root);
var stale = files.Keys.Union(current.Keys)
    .Where(path => !files.TryGetValue(path, out var fresh) || !current.TryGetValue(path, out var committed) || fresh != committed)
    .Order(StringComparer.Ordinal)
    .ToList();

if (check)
{
    foreach (var path in stale)
    {
        Console.Error.WriteLine($"stale: {path}");
    }

    if (stale.Count > 0)
    {
        Console.Error.WriteLine("Generated files are out of date; run `dotnet run --project tools/Generator`.");
    }

    return stale.Count == 0 ? 0 : 1;
}

foreach (var path in stale)
{
    var target = Path.Combine(root, path);
    if (files.TryGetValue(path, out var contents))
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.WriteAllText(target, contents);
    }
    else
    {
        File.Delete(target);
    }
}

Console.WriteLine($"generator: {files.Count} files, {stale.Count} changed.");
return 0;
