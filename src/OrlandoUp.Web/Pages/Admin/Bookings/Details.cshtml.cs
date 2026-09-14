using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin.Bookings;

/// <summary>
/// One booking as it was written: every stored column, its lines, its extras and its history.
/// </summary>
/// <remarks>
/// <b>Nothing here is recomputed from the catalog</b> (D7/03). The page prints columns — the tier
/// that priced each line, the amounts, the tax rate — because a price list edited in March must
/// not change what a January booking says it charged. The control of this leva states that as a
/// prohibition over this file by name.
/// </remarks>
public class DetailsModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly BookingWriter _writer;
    private readonly BookingTimeline _timeline;

    public DetailsModel(AppDbContext db, BookingWriter writer, BookingTimeline timeline)
    {
        _db = db;
        _writer = writer;
        _timeline = timeline;
    }

    public Booking Booking { get; private set; } = null!;

    public IReadOnlyList<BookingEvent> Events { get; private set; } = [];

    /// <summary>Whether the cancel form is offered at all, read from the domain's own table.</summary>
    public bool CanCancel { get; private set; }

    [BindProperty]
    public string? CancelReason { get; set; }

    public string? Flash { get; private set; }

    public string? Error { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        if (!await LoadAsync(id, cancellationToken))
        {
            return NotFound();
        }

        Flash = TempData["Flash"] as string;

        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id, CancellationToken cancellationToken)
    {
        if (!await LoadAsync(id, cancellationToken))
        {
            return NotFound();
        }

        // The status is checked here so the page can answer in words, and again inside the domain,
        // which throws. Two barriers on purpose: this one is the message, that one is the rule,
        // and a caller that forgets this one still cannot write the wrong thing.
        if (!CanCancel)
        {
            Error = "Admin_ErrorCannotCancel";

            return Page();
        }

        if (string.IsNullOrWhiteSpace(CancelReason))
        {
            Error = "Admin_FieldCancelReason";

            return Page();
        }

        Booking cancelled = await _writer.CancelAsync(id, CancelReason.Trim(), cancellationToken);

        // The trace is written HERE, by the handler that did the write, which is how every other
        // administration screen works and what keeps the relation between handlers and records
        // countable over this folder. A booking's history is its audit, so it goes to the table
        // whose own screen shows it and never to the administration's trail.
        _timeline.Record(
            User.Identity?.Name,
            cancelled.Id,
            BookingEventType.Cancelled,
            $"Booking {cancelled.Number} cancelled: {CancelReason.Trim()}");

        await _db.SaveChangesAsync(cancellationToken);

        TempData["Flash"] = "Admin_BookingCancelled";

        return RedirectToPage("/Admin/Bookings/Details", new { id });
    }

    private async Task<bool> LoadAsync(int id, CancellationToken cancellationToken)
    {
        Booking? booking = await _db.Bookings
            .AsNoTracking()
            .Include(row => row.DeliveryZone)
            .Include(row => row.DeliveryLocation)
            .Include(row => row.Lines)
                .ThenInclude(line => line.AddOns)
            .SingleOrDefaultAsync(row => row.Id == id, cancellationToken);

        if (booking is null)
        {
            return false;
        }

        Booking = booking;
        Events = await _timeline.ForBookingAsync(id, cancellationToken);
        CanCancel = BookingStatusRules.CanTransition(booking.Status, BookingStatus.Cancelled);

        return true;
    }
}
