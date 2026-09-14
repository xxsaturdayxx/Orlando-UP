using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrlandoUp.Application;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data;

/// <summary>What the fleet is short of, for one product, on the dates asked for.</summary>
public sealed record Shortfall(int ProductId, string ProductName, int Asked, int MaxQuantity, int AskedExtras, int MaxExtras);

/// <summary>Either the booking that was written, or the reasons it was not.</summary>
public sealed record BookingWriteResult
{
    private BookingWriteResult(Booking? booking, QuoteResult? quote, IReadOnlyList<Shortfall> shortfalls)
    {
        Booking = booking;
        Quote = quote;
        Shortfalls = shortfalls;
    }

    public Booking? Booking { get; }

    /// <summary>Set when the quote refused: it carries the named problem, never a zero price.</summary>
    public QuoteResult? Quote { get; }

    /// <summary>Non-empty when the fleet cannot serve the request and nobody acknowledged it.</summary>
    public IReadOnlyList<Shortfall> Shortfalls { get; }

    public bool Succeeded => Booking is not null;

    public static BookingWriteResult Written(Booking booking) => new(booking, null, []);

    public static BookingWriteResult RefusedByFleet(IReadOnlyList<Shortfall> shortfalls) =>
        new(null, null, shortfalls);

    public static BookingWriteResult RefusedByQuote(QuoteResult quote) => new(null, quote, []);
}

/// <summary>
/// The one way a booking is created or cancelled.
/// </summary>
/// <remarks>
/// It checks the same availability the visitor sees, and refuses by default what the fleet cannot
/// serve. When the operator ticks the box saying he will solve it by hand, it writes the booking
/// anyway, marks it, and says so in the first line of its history — deliberate over-selling is a
/// business decision, and this is where it is written down (D4/03).
///
/// It never assigns a status: the two members that do live in <see cref="Booking"/>, next to the
/// transition table they consult.
///
/// <b>It writes the first line of history inside the same transaction as the booking</b>, and that
/// placement is the invariant: a booking is never born without its event. The catalog screens leave
/// their audit line to the page that called them, and that is fine for a trail nobody reads back —
/// here the history IS the record of the reservation, and the payment front will create bookings
/// through this class with no administration handler anywhere near it. Discipline in a page does
/// not reach that caller; a write inside this transaction does.
/// </remarks>
public sealed class BookingWriter
{
    private readonly AppDbContext _db;
    private readonly AvailabilityQueries _availability;
    private readonly QuoteBuilder _quotes;
    private readonly BookingTimeline _timeline;
    private readonly IClock _clock;

    public BookingWriter(
        AppDbContext db,
        AvailabilityQueries availability,
        QuoteBuilder quotes,
        BookingTimeline timeline,
        IClock clock)
    {
        _db = db;
        _availability = availability;
        _quotes = quotes;
        _timeline = timeline;
        _clock = clock;
    }

    /// <summary>Enters a booking that arrived by WhatsApp, phone or e-mail.</summary>
    public async Task<BookingWriteResult> CreateByStaffAsync(
        StaffBookingDetails details,
        IReadOnlyList<QuoteLineAsked> asked,
        string? actorEmail,
        bool acknowledgeOverbooking,
        CancellationToken cancellation)
    {
        QuoteResult quote = await _quotes.BuildAsync(
            asked, details.DeliveryZoneId, details.StartDate, details.EndDate, details.Culture, cancellation);

        if (quote.Breakdown is not QuoteBreakdown breakdown)
        {
            return BookingWriteResult.RefusedByQuote(quote);
        }

        List<Shortfall> shortfalls = [];

        // Every line of this request is measured against the diary AND against its own siblings.
        // Checking each line alone would let two lines that each fit be written together into a
        // fleet that serves neither: the chargers are a single pool across both scooter models, so
        // one Scout and one Spitfire, each with a second battery, draw four chargers between them
        // while each line on its own draws two. A booking born above the fleet without the mark
        // (D4/03) is the silent version of exactly what that flag exists to make visible.
        IReadOnlyDictionary<int, ProductFacts> facts = await _availability.ProductFactsAsync(cancellation);

        foreach (QuoteLineAsked line in asked)
        {
            List<HoldingLine> siblings = [];

            foreach (QuoteLineAsked other in asked)
            {
                if (ReferenceEquals(other, line) || !facts.TryGetValue(other.ProductId, out ProductFacts? fact))
                {
                    continue;
                }

                siblings.Add(new HoldingLine(
                    other.ProductId,
                    fact.IsScooter,
                    details.StartDate,
                    details.EndDate,
                    other.Quantity,
                    other.ExtraBatteryCount,
                    fact.TurnaroundDays));
            }

            AvailabilityResult answer = await _availability.ForProductAsync(
                line.ProductId, details.StartDate, details.EndDate, line.Quantity, line.ExtraBatteryCount,
                cancellation, siblings);

            if (answer.IsAvailable)
            {
                continue;
            }

            QuotedLine priced = breakdown.Lines.Single(row => row.ProductId == line.ProductId);

            shortfalls.Add(new Shortfall(
                line.ProductId,
                priced.ProductName,
                line.Quantity,
                answer.MaxQuantity,
                line.ExtraBatteryCount,
                answer.MaxExtraBatteries));
        }

        bool isOverbooked = shortfalls.Count > 0;

        if (isOverbooked && !acknowledgeOverbooking)
        {
            return BookingWriteResult.RefusedByFleet(shortfalls);
        }

        DateTime now = _clock.UtcNow;

        Booking booking = Booking.CreateByStaff(details, breakdown, actorEmail, now, isOverbooked);

        _db.Bookings.Add(booking);

        // Saved once to be given a key, then again with the number derived from it. Both inside one
        // transaction, so a booking never exists without its number (D6/03).
        await using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellation);

        await _db.SaveChangesAsync(cancellation);

        booking.Number = BookingRules.FormatNumber(booking.Id);

        _timeline.Record(
            actorEmail,
            booking.Id,
            BookingEventType.Created,
            isOverbooked
                ? $"Booking {booking.Number} entered by staff, overbooked above the fleet."
                : $"Booking {booking.Number} entered by staff.");

        // The number and the first line of history land in the same commit as the booking.
        await _db.SaveChangesAsync(cancellation);
        await transaction.CommitAsync(cancellation);

        return BookingWriteResult.Written(booking);
    }

    /// <summary>Cancels a booking and gives its equipment back to the pool.</summary>
    /// <exception cref="InvalidOperationException">
    /// The booking cannot be cancelled from its current status. Thrown by the domain, not decided
    /// here.
    /// </exception>
    public async Task<Booking> CancelAsync(
        int bookingId,
        string? actorEmail,
        string reason,
        CancellationToken cancellation)
    {
        Booking booking = await _db.Bookings.SingleAsync(row => row.Id == bookingId, cancellation);

        booking.Cancel(_clock.UtcNow, reason);

        _timeline.Record(
            actorEmail,
            booking.Id,
            BookingEventType.Cancelled,
            $"Booking {booking.Number} cancelled: {reason}");

        await _db.SaveChangesAsync(cancellation);

        return booking;
    }
}
