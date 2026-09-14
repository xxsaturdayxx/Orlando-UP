using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin.Bookings;

/// <summary>
/// Every reservation, newest first — what WhatsApp has been giving Rod on paper.
/// </summary>
/// <remarks>
/// The list reads the columns the booking stored and never the catalog (D7/03): the place is the
/// name as it is today because that is a label rather than a price, but every amount shown is the
/// snapshot the quote produced on the day.
/// </remarks>
public class IndexModel : PageModel
{
    /// <summary>One screen of scrolling, the same ceiling the audit list uses.</summary>
    public const int RecentCount = 100;

    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    public sealed record Row(
        int Id,
        string Number,
        string Customer,
        DateOnly StartDate,
        DateOnly EndDate,
        string Place,
        BookingStatus Status,
        decimal Total,
        bool IsOverbooked);

    public IReadOnlyList<Row> Rows { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Rows = await _db.Bookings
            .AsNoTracking()
            .OrderByDescending(booking => booking.CreatedAtUtc)
            .ThenByDescending(booking => booking.Id)
            .Take(RecentCount)
            .Select(booking => new Row(
                booking.Id,
                booking.Number,
                booking.FirstName + " " + booking.LastName,
                booking.StartDate,
                booking.EndDate,

                // The curated place when there is one, otherwise the zone's own code: the zone is
                // always set, so this never has to fall back to an empty cell.
                booking.DeliveryLocation != null ? booking.DeliveryLocation.Name : booking.DeliveryZone!.Code,
                booking.Status,
                booking.Total,
                booking.IsOverbooked))
            .ToListAsync(cancellationToken);
    }
}
