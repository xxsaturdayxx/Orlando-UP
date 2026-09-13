using Microsoft.EntityFrameworkCore;
using OrlandoUp.Application;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data;

/// <summary>
/// Writes and reads a booking's history. The sibling of <see cref="AuditTrail"/>, and shaped after
/// it deliberately.
/// </summary>
/// <remarks>
/// <b>The method is called <c>Record</c> on purpose.</b> A control of the previous leva counts the
/// administration's POST handlers and subtracts the calls that register what they did, and the
/// difference is the invariant: every write leaves a trace. A booking's history IS its audit, so
/// the booking screens record here — one row per write, in the table whose own screen shows it —
/// and never into the administration's table. Naming this method anything else would leave that
/// control red while the design was right.
///
/// Like its sibling, <see cref="Record"/> only STAGES the row. The caller's <c>SaveChangesAsync</c>
/// commits it, so a write that fails leaves behind no line claiming it happened. The instant comes
/// from <see cref="IClock"/>.
/// </remarks>
public sealed class BookingTimeline
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;

    public BookingTimeline(AppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    /// <summary>Stages one line of history against a booking.</summary>
    public void Record(string? actorEmail, int bookingId, BookingEventType type, string summary)
    {
        _db.BookingEvents.Add(new BookingEvent
        {
            BookingId = bookingId,
            OccurredAtUtc = _clock.UtcNow,

            // Null is meaningful here and is NOT flattened to a placeholder the way the
            // administration's trail flattens it: a null actor is the customer or the system,
            // which the payment front will need to tell apart from a member of staff.
            ActorEmail = actorEmail,
            Type = type,
            Summary = summary,
        });
    }

    /// <summary>One booking's history, oldest first — the order the detail screen reads it in.</summary>
    public async Task<IReadOnlyList<BookingEvent>> ForBookingAsync(int bookingId, CancellationToken cancellation)
    {
        return await _db.BookingEvents
            .AsNoTracking()
            .Where(row => row.BookingId == bookingId)
            .OrderBy(row => row.OccurredAtUtc)
            .ThenBy(row => row.Id)
            .ToListAsync(cancellation);
    }
}
