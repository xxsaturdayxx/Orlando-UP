using System.Globalization;
using OrlandoUp.Application;
using OrlandoUp.Infrastructure.Localization;

namespace OrlandoUp.Pages;

/// <summary>
/// The value every internal link puts on its culture route value so that it stays in the language
/// being read.
/// </summary>
/// <remarks>
/// The plan for this leva expected link generation to carry the culture segment by itself, as an
/// ambient route value, and the routing test was written to find out. It does not: an ambient value
/// travels to the SAME page, not to a different one, so every link from a Portuguese page was
/// coming out at the English address. Rather than leave that to a mechanism, every internal link
/// now says which culture it wants, and the empty string is a real answer meaning "no prefix".
/// </remarks>
public static class CultureLink
{
    /// <summary>The segment for the culture being read: the prefix, or nothing for the default.</summary>
    public static string Current =>
        string.Equals(CultureInfo.CurrentUICulture.Name, SiteCultures.Portuguese, StringComparison.OrdinalIgnoreCase)
            ? CultureRouteConvention.PortugueseSegment
            : string.Empty;
}
