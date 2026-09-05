using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrlandoUp.Application.Catalog;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages;

public class IndexModel : PageModel
{
    private readonly CatalogQueries _catalog;

    public IndexModel(CatalogQueries catalog) => _catalog = catalog;

    public IReadOnlyList<ProductCard> Cards { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Cards = await _catalog.ActiveCardsAsync(CultureInfo.CurrentUICulture.Name, cancellationToken);
    }
}
