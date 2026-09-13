namespace OrlandoUp.Domain;

/// <summary>
/// One extra sold with one line of a booking, priced as it was on the day.
/// </summary>
/// <remarks>
/// It hangs off the LINE and not off the booking (D10/03): a family renting a scooter and a
/// wheelchair on one booking can want a cup holder on the scooter and nothing on the chair, and an
/// extra attached to the booking could not say which. The quantity is a column of its own even
/// though this leva always fills it with the line's quantity — a later screen varies it without a
/// migration.
/// </remarks>
public class BookingAddOn
{
    public int Id { get; set; }

    public int BookingLineId { get; set; }

    public BookingLine? BookingLine { get; set; }

    public int AddOnId { get; set; }

    public AddOn? AddOn { get; set; }

    /// <summary>The extra's name in the booking's culture, on the day it was booked.</summary>
    public string AddOnName { get; set; } = string.Empty;

    /// <summary>How the amount was read, snapshotted: once for the rental, or once per day.</summary>
    public AddOnPricingMode PricingMode { get; set; }

    public decimal Amount { get; set; }

    public int Quantity { get; set; }

    /// <summary>
    /// The amount times the quantity, and times the days as well when the mode is per day.
    /// </summary>
    public decimal Total { get; set; }
}
