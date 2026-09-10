using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin.Batteries;

/// <summary>
/// The batteries, which are what limits the day: with four scooters and six batteries per model,
/// the battery runs out before the scooter does (D36).
/// </summary>
/// <remarks>
/// Retired ones are listed and marked, like units: a battery that vanished from the screen is a
/// battery somebody buys twice. The kind is shown and never used to filter — availability treats
/// all batteries of a model as one pool (D2/04b), and control C01 states that as a prohibition.
/// </remarks>
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    public sealed record Row(
        int Id,
        string AssetTag,
        string ProductSlug,
        BatteryKind Kind,
        decimal? RangeMiles,
        UnitStatus Status,
        DateOnly? PurchasedOn);

    public IReadOnlyList<Row> Rows { get; private set; } = [];

    /// <summary>Available ones, which is the number that answers "can I promise a second one".</summary>
    public int AvailableCount { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Rows = await _db.Batteries
            .AsNoTracking()
            .OrderBy(battery => battery.Product!.SortOrder)
            .ThenBy(battery => battery.AssetTag)
            .Select(battery => new Row(
                battery.Id,
                battery.AssetTag,
                battery.Product!.Slug,
                battery.Kind,
                battery.RangeMiles,
                battery.Status,
                battery.PurchasedOn))
            .ToListAsync(cancellationToken);

        AvailableCount = Rows.Count(row => row.Status == UnitStatus.Available);
    }
}
