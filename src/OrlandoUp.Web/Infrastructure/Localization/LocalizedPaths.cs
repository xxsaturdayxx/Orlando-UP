namespace OrlandoUp.Infrastructure.Localization;

/// <summary>
/// The same page in the two cultures, as paths, for the canonical and alternate links of the head.
/// </summary>
/// <remarks>
/// The switcher in the header does NOT use this: it goes through link generation, so that the
/// route values of the current page travel with it. This is only for the head, where an absolute
/// address is needed and there is no page to generate from.
/// </remarks>
public static class LocalizedPaths
{
    private const string Prefix = "/" + CultureRouteConvention.PortugueseSegment;

    /// <summary>The address of the current page without the culture prefix.</summary>
    public static string English(PathString path)
    {
        string value = path.HasValue ? path.Value! : "/";

        if (string.Equals(value, Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return "/";
        }

        if (value.StartsWith(Prefix + "/", StringComparison.OrdinalIgnoreCase))
        {
            return value[Prefix.Length..];
        }

        return value;
    }

    /// <summary>The address of the current page under the culture prefix.</summary>
    public static string Portuguese(PathString path)
    {
        string english = English(path);

        return english == "/" ? Prefix : Prefix + english;
    }
}
