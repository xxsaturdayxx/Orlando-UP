using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin.Units;

/// <summary>
/// The fleet, which is the physical machines and not the catalog. Retired units are listed too and
/// marked: a unit that vanished from the screen is a unit somebody buys twice.
/// </summary>
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    public sealed record Row(
        int Id,
        string AssetTag,
        string ProductSlug,
        UnitStatus Status,
        string? SerialNumber,
        DateOnly? PurchasedOn);

    public IReadOnlyList<Row> Rows { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Rows = await _db.Units
            .AsNoTracking()
            .OrderBy(unit => unit.Product!.SortOrder)
            .ThenBy(unit => unit.AssetTag)
            .Select(unit => new Row(
                unit.Id,
                unit.AssetTag,
                unit.Product!.Slug,
                unit.Status,
                unit.SerialNumber,
                unit.PurchasedOn))
            .ToListAsync(cancellationToken);
    }
}
