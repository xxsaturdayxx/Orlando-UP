using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

namespace OrlandoUp.Infrastructure.Localization;

/// <summary>
/// Every public page of the site, and the two addresses each of them answers on.
/// </summary>
/// <remarks>
/// The set is derived from the Razor page collection the application actually registered, never
/// from a list typed somewhere. A typed list is a second source of truth that drifts the first
/// time somebody adds a page and forgets it, and the sitemap is exactly the artefact nobody looks
/// at again — so the page that was forgotten is invisible until a search engine says so, which is
/// months later. Asking the framework what pages exist means a new page is in the sitemap the
/// moment it exists.
/// </remarks>
public sealed class PublicPages
{
    private readonly IActionDescriptorCollectionProvider _actions;
    private readonly LinkGenerator _links;

    public PublicPages(IActionDescriptorCollectionProvider actions, LinkGenerator links)
    {
        _actions = actions;
        _links = links;
    }

    /// <summary>The Razor page paths a visitor may open without an account and without a parameter.</summary>
    public IReadOnlyList<string> Names()
    {
        List<PageActionDescriptor> pages = _actions.ActionDescriptors.Items
            .OfType<PageActionDescriptor>()
            .ToList();

        return pages
            .Select(page => page.ViewEnginePath)
            .Distinct(StringComparer.Ordinal)
            .Where(IsPublic)
            .Where(name => !TakesAParameter(name, pages))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>The address of a page in a culture, or <c>null</c> when it cannot be generated.</summary>
    public string? PathFor(string name, string cultureSegment, string? slug = null)
    {
        RouteValueDictionary values = new()
        {
            [CultureRouteConvention.RouteValueKey] = cultureSegment,
        };

        if (slug is not null)
        {
            values["slug"] = slug;
        }

        return _links.GetPathByPage(name, handler: null, values: values);
    }

    /// <summary>The culture segments the site serves, in the order the sitemap lists them.</summary>
    public static IReadOnlyList<(string Segment, string Culture)> Cultures { get; } =
    [
        (string.Empty, "en-US"),
        (CultureRouteConvention.PortugueseSegment, "pt-BR"),
    ];

    private static bool IsPublic(string viewEnginePath) =>
        !viewEnginePath.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase)
        && !viewEnginePath.StartsWith("/Error", StringComparison.OrdinalIgnoreCase)
        && !viewEnginePath.StartsWith("/Shared", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// A page whose address needs something the sitemap cannot invent - the product detail page and
    /// its slug. Those addresses come from the catalog instead, one per row that exists.
    /// </summary>
    private static bool TakesAParameter(string viewEnginePath, IEnumerable<PageActionDescriptor> pages) =>
        pages
            .Where(page => string.Equals(page.ViewEnginePath, viewEnginePath, StringComparison.Ordinal))
            .Select(page => page.AttributeRouteInfo?.Template ?? string.Empty)
            .Any(template => template
                .Replace("{" + CultureRouteConvention.RouteValueKey, string.Empty, StringComparison.Ordinal)
                .Contains('{', StringComparison.Ordinal));
}
