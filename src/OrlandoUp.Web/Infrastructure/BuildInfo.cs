using System.Reflection;

namespace OrlandoUp.Infrastructure;

/// <summary>
/// What the build stamped into the assembly, read once. The layout puts it in a generator meta tag
/// so that anyone looking at a page can tell which build produced it.
/// </summary>
public static class BuildInfo
{
    /// <summary>
    /// The short commit of the build, with a marker appended when the tree had pending changes.
    /// Empty when the build machine had no git, which is a declared loss, not a silent one.
    /// </summary>
    public static string Revision { get; } = ReadRevision();

    private static string ReadRevision()
    {
        string? informational = (Assembly.GetEntryAssembly() ?? typeof(BuildInfo).Assembly)
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(informational))
        {
            return string.Empty;
        }

        int plus = informational.IndexOf('+');

        return plus >= 0 && plus < informational.Length - 1
            ? informational[(plus + 1)..]
            : string.Empty;
    }
}
