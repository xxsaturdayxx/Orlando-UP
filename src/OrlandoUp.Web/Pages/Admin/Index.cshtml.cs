using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OrlandoUp.Application;
using OrlandoUp.Infrastructure.Data;
using OrlandoUp.Infrastructure.Seeding;

namespace OrlandoUp.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    public int ProductCount { get; private set; }

    public int UnitCount { get; private set; }

    public int LocationCount { get; private set; }

    /// <summary>
    /// True while the catalog is still the one the seeding command wrote. It is decided by comparing
    /// the description of the first product with the text the seeder holds: no settings row, no flag
    /// to forget to clear, and it turns itself off the moment somebody edits that product for real.
    /// </summary>
    public bool ShowsPlaceholderData { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        ProductCount = await _db.Products.CountAsync(cancellationToken);
        UnitCount = await _db.Units.CountAsync(cancellationToken);
        LocationCount = await _db.DeliveryLocations.CountAsync(cancellationToken);

        string seededDescription = CatalogSeedData.Products
            .Single(product => product.Slug == "standard-scooter")
            .Texts.Single(text => text.Culture == SiteCultures.English)
            .Description;

        ShowsPlaceholderData = await _db.ProductTranslations
            .AnyAsync(
                translation => translation.Culture == SiteCultures.English
                    && translation.Description == seededDescription,
                cancellationToken);
    }
}
