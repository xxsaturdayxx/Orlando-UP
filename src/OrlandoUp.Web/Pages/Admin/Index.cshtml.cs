using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OrlandoUp.Domain;
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

    /// <summary>
    /// Batteries, which are what limits the day: with four scooters and six batteries per model the
    /// battery runs out before the scooter does (D36).
    /// </summary>
    public int BatteryCount { get; private set; }

    /// <summary>
    /// Chargers, which are a stored number and not a table: any charger fits any battery of either
    /// model, so a piece with no identity of its own is counted rather than listed (D3/04b). It is
    /// independent of the battery count and is not derived from it.
    /// </summary>
    public int ChargerCount { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        ProductCount = await _db.Products.CountAsync(cancellationToken);
        UnitCount = await _db.Units.CountAsync(cancellationToken);
        LocationCount = await _db.DeliveryLocations.CountAsync(cancellationToken);
        BatteryCount = await _db.Batteries.CountAsync(cancellationToken);

        // The row the migration created. A database without it shows zero rather than throwing on
        // the dashboard, and the settings screen is where that absence is reported.
        ChargerCount = await _db.OperationalSettings
            .Where(row => row.Id == OperationalSettings.SingletonId)
            .Select(row => row.ChargerCount)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
