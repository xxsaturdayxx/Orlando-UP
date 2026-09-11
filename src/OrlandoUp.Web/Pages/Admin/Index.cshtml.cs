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
    /// <remarks>
    /// <b>Nullable, and that is the whole point.</b> This is the one number on the dashboard that
    /// comes from a ROW rather than from a <c>Count</c>: an empty table honestly holds zero
    /// batteries, but a missing settings row does not mean the operation owns no chargers — it
    /// means nobody knows. Projected onto a non-nullable <c>int</c>, <c>FirstOrDefault</c> hands
    /// back <c>0</c> and the screen states a fact about the fleet that nothing measured (D15).
    /// The type is the barrier; the habit is not.
    /// </remarks>
    public int? ChargerCount { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        ProductCount = await _db.Products.CountAsync(cancellationToken);
        UnitCount = await _db.Units.CountAsync(cancellationToken);
        LocationCount = await _db.DeliveryLocations.CountAsync(cancellationToken);
        BatteryCount = await _db.Batteries.CountAsync(cancellationToken);

        // The row the migration created. Projected onto int? so that a database without it answers
        // absence and not zero; the screen then shows the absence, in the same vocabulary the
        // settings screen already uses for it.
        ChargerCount = await _db.OperationalSettings
            .Where(row => row.Id == OperationalSettings.SingletonId)
            .Select(row => (int?)row.ChargerCount)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
