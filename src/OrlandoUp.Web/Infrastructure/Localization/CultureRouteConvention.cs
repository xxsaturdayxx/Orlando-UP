using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OrlandoUp.Infrastructure.Localization;

/// <summary>
/// Gives every public page a second address under the Portuguese prefix, so that /rentals and
/// /pt/rentals are the same page in two cultures (D21, D3/01).
/// </summary>
/// <remarks>
/// The prefix is constrained to the one extra culture the site serves, so an unknown prefix such
/// as /es matches no route at all and comes out as a 404 rather than as an English page under a
/// Spanish address. The administration is left out on purpose: it picks its culture from a cookie
/// (D4/01), because the delivery team switches language without changing the address they typed.
/// </remarks>
public sealed class CultureRouteConvention : IPageRouteModelConvention
{
    /// <summary>The route value the request culture provider reads.</summary>
    public const string RouteValueKey = "culture";

    /// <summary>The URL segment that stands for Portuguese.</summary>
    public const string PortugueseSegment = "pt";

    private const string Prefix = "{" + RouteValueKey + ":regex(^" + PortugueseSegment + "$)}";

    public void Apply(PageRouteModel model)
    {
        if (model.ViewEnginePath.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Copied first: the loop adds to the same collection it reads.
        List<SelectorModel> original = model.Selectors.ToList();

        foreach (SelectorModel selector in original)
        {
            string? template = selector.AttributeRouteModel?.Template;

            if (template is null)
            {
                continue;
            }

            string tail = template.TrimStart('/');
            bool isIndexless = tail.Length == 0;
            string prefixed = isIndexless ? Prefix : Prefix + "/" + tail;

            // The home page arrives here twice, because the framework gives it both "Index" and the
            // index-less "" — so it leaves with both {culture} and {culture}/Index. The two are
            // different addresses and both must keep matching, but link generation has to prefer
            // the short one, exactly as the framework prefers "/" over "/Index" in English. Equal
            // order left that to chance, and chance chose /pt/Index for the language switcher.
            int order = isIndexless ? -2 : -1;

            model.Selectors.Add(new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel
                {
                    Order = order,
                    Template = prefixed,
                },
            });
        }
    }
}
