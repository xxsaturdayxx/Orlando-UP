using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OrlandoUp.Application;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin.Products;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    public sealed record Row(
        string Slug,
        ProductCategory Category,
        string? NameEnglish,
        string? NamePortuguese,
        bool IsActive,
        int UnitCount);

    public IReadOnlyList<Row> Rows { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        // The administration sees hidden products too: the flag is a soft hide for the public site,
        // not a way to lose a product from the back office.
        Rows = await _db.Products
            .AsNoTracking()
            .OrderBy(product => product.SortOrder)
            .Select(product => new Row(
                product.Slug,
                product.Category,
                product.Translations
                    .Where(text => text.Culture == SiteCultures.English)
                    .Select(text => text.Name)
                    .FirstOrDefault(),
                product.Translations
                    .Where(text => text.Culture == SiteCultures.Portuguese)
                    .Select(text => text.Name)
                    .FirstOrDefault(),
                product.IsActive,
                product.Units.Count))
            .ToListAsync(cancellationToken);
    }
}
