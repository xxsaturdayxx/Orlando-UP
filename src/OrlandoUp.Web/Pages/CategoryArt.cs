using OrlandoUp.Domain;

namespace OrlandoUp.Pages;

/// <summary>
/// Which drawing stands in for a product that has no photograph yet (open question Q11). The
/// illustrations are simple line drawings made for this site; no third-party artwork is used.
/// </summary>
public static class CategoryArt
{
    public static string PathFor(ProductCategory category) => category switch
    {
        ProductCategory.MobilityScooter => "/img/categories/mobility-scooter.svg",
        ProductCategory.Wheelchair => "/img/categories/wheelchair.svg",
        _ => "/img/categories/stroller.svg",
    };

    /// <summary>The resource key that names the category in the visitor's language.</summary>
    public static string NameKey(ProductCategory category) => "Category_" + category;
}
