using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrlandoUp.Application.Catalog;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Rentals;

public class IndexModel : PageModel
{
    private readonly CatalogQueries _catalog;

    public IndexModel(CatalogQueries catalog) => _catalog = catalog;

    /// <summary>The categories in the order the page shows them, each with its cards.</summary>
    public IReadOnlyList<IGrouping<ProductCategory, ProductCard>> Groups { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<ProductCard> cards =
            await _catalog.ActiveCardsAsync(CultureInfo.CurrentUICulture.Name, cancellationToken);

        Groups = cards.GroupBy(card => card.Category).ToList();
    }
}
