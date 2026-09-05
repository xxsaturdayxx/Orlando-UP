using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrlandoUp.Application.Catalog;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Rentals;

public class DetailsModel : PageModel
{
    private readonly CatalogQueries _catalog;

    public DetailsModel(CatalogQueries catalog) => _catalog = catalog;

    public ProductDetail Product { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken cancellationToken)
    {
        ProductDetail? found =
            await _catalog.ActiveDetailAsync(slug, CultureInfo.CurrentUICulture.Name, cancellationToken);

        // Not found, or hidden by an administrator: the same answer either way, because a page that
        // said which of the two it was would leak the catalog of things not on sale.
        if (found is null)
        {
            return NotFound();
        }

        Product = found;

        return Page();
    }
}
