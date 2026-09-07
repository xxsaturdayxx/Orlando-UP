using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    public int ProductCount { get; private set; }

    /// <summary>
    /// Physical units, which is the fleet and not the catalog: the four stroller products carry
    /// none until they are bought, so this number and <see cref="ProductCount"/> differ on purpose.
    /// </summary>
    public int UnitCount { get; private set; }

    public int LocationCount { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        ProductCount = await _db.Products.CountAsync(cancellationToken);
        UnitCount = await _db.Units.CountAsync(cancellationToken);
        LocationCount = await _db.DeliveryLocations.CountAsync(cancellationToken);
    }
}
