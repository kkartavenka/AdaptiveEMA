namespace FilterComparison;

/// <summary>
/// Resolves paths relative to the repository root so the harness reads the data files
/// in place. Avoids a copy of every dataset landing in the build output.
/// </summary>
internal static class RepoPaths
{
    private const string RootMarker = "AdaptiveEMA.sln";

    internal static string Root { get; } = FindRoot();

    internal static string Resolve(string relativePath)
        => Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, RootMarker)))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException($"Could not locate {RootMarker} above {AppContext.BaseDirectory}");
    }
}
