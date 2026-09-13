namespace OrlandoUp.Domain;

/// <summary>
/// One reservation: who, what, where, when, and every amount it was priced at on the day.
/// </summary>
/// <remarks>
/// <b>Every amount here is a snapshot (D7/03).</b> The quote computes them once, this row stores
/// them, and no screen recomputes one from the catalog afterwards — a price list edited in March
/// must not rewrite what a customer paid in January. That is why the tier that priced each line is
/// copied onto the line, and why the zone's tax rate is copied here rather than read back through
/// <c>DeliveryZoneId</c>.
///
/// <b><see cref="Status"/> has a private setter, and that is a barrier and not a preference.</b>
/// EF Core materialises through it, so persistence is unaffected; a page that tried to assign a
/// status would not compile. The two places that may change it both live in this file — the factory
/// that creates the booking and <see cref="Cancel"/> — which is what makes the status machine of
/// <c>BookingStatusRules</c> enforceable instead of merely documented (EMENDA-03-01, correction 2).
/// </remarks>
public class Booking
{
    public int Id { get; set; }

    /// <summary>
    /// What a customer reads aloud on the phone: <c>OU-</c> and the row id padded to six digits
    /// (D6/03). Stored rather than derived, because it is also what a search box looks for, and
    /// written in the same transaction as the insert.
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Where the booking stands. Assigned only inside this class — see the remarks on the type.
    /// </summary>
    public BookingStatus Status { get; private set; }

    public BookingSource Source { get; set; }

    /// <summary>
    /// The language the customer deals with us in, <c>en-US</c> or <c>pt-BR</c>. Stored so that a
    /// later e-mail is written in it, and so that the names snapshotted on the lines can be read
    /// back in the language they were quoted in.
    /// </summary>
    public string Culture { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>One field. The number people reach us on is the number they message us on.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// The zone, always. The delivery fee and the tax rate came from it, and it is set even when a
    /// curated location was picked (D11/03).
    /// </summary>
    public int DeliveryZoneId { get; set; }

    public DeliveryZone? DeliveryZone { get; set; }

    /// <summary>The curated place, when the zone has a list. Null when the zone has none.</summary>
    public int? DeliveryLocationId { get; set; }

    public DeliveryLocation? DeliveryLocation { get; set; }

    /// <summary>Required by validation when no curated location was chosen.</summary>
    public string? Address { get; set; }

    /// <summary>Room, gate, "meet at the bus loop".</summary>
    public string? DeliveryNotes { get; set; }

    /// <summary>The delivery day, on the Orlando calendar.</summary>
    public DateOnly StartDate { get; set; }

    /// <summary>The pickup day, on the Orlando calendar.</summary>
    public DateOnly EndDate { get; set; }

    public DeliveryWindow DeliveryWindow { get; set; }

    public DeliveryWindow PickupWindow { get; set; }

    /// <summary>
    /// <c>EndDate − StartDate + 1</c>. Stored because every amount below was computed from it, and
    /// a stored total whose divisor has to be recomputed to be understood is a total nobody trusts.
    /// </summary>
    public int Days { get; set; }

    /// <summary>The rental of the equipment itself: the sum of the lines.</summary>
    public decimal Subtotal { get; set; }

    public decimal ExtraBatteriesTotal { get; set; }

    public decimal AddOnsTotal { get; set; }

    public decimal DeliveryFee { get; set; }

    /// <summary>
    /// The zone's rate on the day of the booking, four decimal places. Copied so that a rate
    /// decided later never rewrites this booking's tax.
    /// </summary>
    public decimal TaxRate { get; set; }

    public decimal Tax { get; set; }

    public decimal Total { get; set; }

    /// <summary>
    /// True when staff entered this above what the fleet has on these dates, having decided to
    /// solve it by hand (D4/03). The public site can never set it.
    /// </summary>
    public bool IsOverbooked { get; set; }

    /// <summary>How it was paid, and anything else the office needs to remember.</summary>
    public string? StaffNotes { get; set; }

    /// <summary>An instant, from <c>IClock</c> (D16).</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>The member of staff who entered it; null when a customer created it.</summary>
    public string? CreatedByEmail { get; set; }

    public DateTime? CancelledAtUtc { get; set; }

    public string? CancelReason { get; set; }

    public List<BookingLine> Lines { get; set; } = [];

    public List<BookingEvent> Events { get; set; } = [];

    /// <summary>
    /// Builds the booking a member of staff enters, already priced, already <see cref="BookingStatus.Confirmed"/>.
    /// </summary>
    /// <remarks>
    /// This is one of the two members in the whole application that assign a status, and it is here
    /// rather than in the writing service for a reason that outlives this leva: the invariant is
    /// that a booking is never half-built — a status without a total, or a total that no quote
    /// produced. Handing the quote in whole and getting the booking back whole is what makes that
    /// unrepresentable instead of merely discouraged.
    ///
    /// The number is NOT set here: it is derived from the identity key, which the database has not
    /// handed out yet. The writer assigns it in the same transaction as the insert (D6/03).
    /// </remarks>
    public static Booking CreateByStaff(
        StaffBookingDetails details,
        QuoteBreakdown quote,
        string? actorEmail,
        DateTime nowUtc,
        bool isOverbooked)
    {
        Booking booking = new()
        {
            Status = BookingStatus.Confirmed,
            Source = BookingSource.Staff,
            Culture = details.Culture,
            FirstName = details.FirstName,
            LastName = details.LastName,
            Email = details.Email,
            Phone = details.Phone,
            DeliveryZoneId = details.DeliveryZoneId,
            DeliveryLocationId = details.DeliveryLocationId,
            Address = details.Address,
            DeliveryNotes = details.DeliveryNotes,
            StartDate = details.StartDate,
            EndDate = details.EndDate,
            DeliveryWindow = details.DeliveryWindow,
            PickupWindow = details.PickupWindow,
            Days = quote.Days,
            Subtotal = quote.Subtotal,
            ExtraBatteriesTotal = quote.ExtraBatteriesTotal,
            AddOnsTotal = quote.AddOnsTotal,
            DeliveryFee = quote.DeliveryFee,
            TaxRate = quote.TaxRate,
            Tax = quote.Tax,
            Total = quote.Total,
            IsOverbooked = isOverbooked,
            StaffNotes = details.StaffNotes,
            CreatedAtUtc = nowUtc,
            CreatedByEmail = actorEmail,
        };

        foreach (QuotedLine line in quote.Lines)
        {
            BookingLine bookingLine = new()
            {
                ProductId = line.ProductId,
                ProductName = line.ProductName,
                Quantity = line.Quantity,
                ExtraBatteryCount = line.ExtraBatteryCount,
                TierMinDays = line.TierMinDays,
                TierMaxDays = line.TierMaxDays,
                TierMode = line.TierMode,
                TierAmount = line.TierAmount,
                UnitPrice = line.UnitPrice,
                LineTotal = line.LineTotal,
                ExtraBatteryPerDay = line.ExtraBatteryPerDay,
                ExtraBatteriesTotal = line.ExtraBatteriesTotal,
            };

            foreach (QuotedAddOn addOn in line.AddOns)
            {
                bookingLine.AddOns.Add(new BookingAddOn
                {
                    AddOnId = addOn.AddOnId,
                    AddOnName = addOn.Name,
                    PricingMode = addOn.Mode,
                    Amount = addOn.Amount,
                    Quantity = addOn.Quantity,
                    Total = addOn.Total,
                });
            }

            booking.Lines.Add(bookingLine);
        }

        return booking;
    }

    /// <summary>
    /// Gives the equipment back to the pool: a cancelled booking does not hold inventory, so the
    /// days it occupied become available to everyone again.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The booking cannot be cancelled from the status it is in. Refused here and not in a page,
    /// so that the refusal holds for every caller there will ever be.
    /// </exception>
    public void Cancel(DateTime nowUtc, string reason)
    {
        if (!BookingStatusRules.CanTransition(Status, BookingStatus.Cancelled))
        {
            throw new InvalidOperationException($"A booking in {Status} cannot be cancelled.");
        }

        Status = BookingStatus.Cancelled;
        CancelledAtUtc = nowUtc;
        CancelReason = reason;
    }
}

/// <summary>
/// Everything about a staff booking that the quote does not already carry.
/// </summary>
/// <remarks>
/// A parameter object rather than fourteen arguments, so that two of the same type cannot be
/// swapped at a call site without the compiler noticing the name.
/// </remarks>
public sealed record StaffBookingDetails(
    string Culture,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    int DeliveryZoneId,
    int? DeliveryLocationId,
    string? Address,
    string? DeliveryNotes,
    DateOnly StartDate,
    DateOnly EndDate,
    DeliveryWindow DeliveryWindow,
    DeliveryWindow PickupWindow,
    string? StaffNotes);
