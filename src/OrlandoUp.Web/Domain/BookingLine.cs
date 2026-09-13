namespace OrlandoUp.Domain;

/// <summary>
/// One piece of equipment on a booking, with the price band it was charged under, frozen.
/// </summary>
/// <remarks>
/// The tier is copied onto the line — its bounds, its mode and its amount — rather than linked to
/// the row it came from. A price list is editable content (leva 04 built the screen for it), so a
/// link would let an edit in March change what a January line says it was charged under. Four
/// columns of snapshot are cheaper than a booking nobody can explain.
/// </remarks>
public class BookingLine
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public Booking? Booking { get; set; }

    /// <summary>
    /// The catalog row, kept as a link because a product is never deleted — it is hidden — so the
    /// link survives. The name is snapshotted all the same, because a product can be renamed.
    /// </summary>
    public int ProductId { get; set; }

    public Product? Product { get; set; }

    /// <summary>The product's name in the booking's culture, on the day it was booked.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>How many of this product. One or more.</summary>
    public int Quantity { get; set; }

    /// <summary>
    /// How many second batteries, at most one per machine, and always zero for anything that is
    /// not a scooter (D8/03). The line draws <c>Quantity + ExtraBatteryCount</c> batteries and the
    /// same number of chargers from the pools.
    /// </summary>
    public int ExtraBatteryCount { get; set; }

    /// <summary>The lower bound of the band that priced this line.</summary>
    public int TierMinDays { get; set; }

    /// <summary>
    /// The upper bound, or null for the open-ended band. The one legitimate null of this row.
    /// </summary>
    public int? TierMaxDays { get; set; }

    public TierMode TierMode { get; set; }

    public decimal TierAmount { get; set; }

    /// <summary>
    /// What ONE of these cost for the whole rental: the tier's amount when the band is flat, and
    /// the amount times the days when it is per day.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary><see cref="UnitPrice"/> times <see cref="Quantity"/>.</summary>
    public decimal LineTotal { get; set; }

    /// <summary>
    /// What a second battery was sold for per day on this line. Defaulted from the settings row and
    /// typed over when the office decides otherwise; zero is a courtesy and still consumes a
    /// battery and a charger (D37).
    /// </summary>
    public decimal ExtraBatteryPerDay { get; set; }

    /// <summary><see cref="ExtraBatteryCount"/> × <see cref="ExtraBatteryPerDay"/> × the days.</summary>
    public decimal ExtraBatteriesTotal { get; set; }

    public List<BookingAddOn> AddOns { get; set; } = [];
}
