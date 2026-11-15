namespace TeachMeHipaa.Generator;

public static class RepoRoot
{
    /// <summary>The nearest directory above the working directory (or this tool) holding the solution.</summary>
    public static string Find()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "TeachMeHipaa.sln")))
                {
                    return directory.FullName;
                }
            }
        }

        throw new DirectoryNotFoundException("TeachMeHipaa.sln not found above the working directory.");
    }

    /// <summary>Every file now in the generator's directories, repo-relative with forward slashes.</summary>
    public static SortedDictionary<string, string> GeneratedFiles(string root)
    {
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var directory in SurfaceGenerator.OwnedDirectories)
        {
            var path = Path.Combine(root, directory);
            if (!Directory.Exists(path))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                files[Path.GetRelativePath(root, file).Replace('\\', '/')] = File.ReadAllText(file);
            }
        }

        return files;
    }
}
