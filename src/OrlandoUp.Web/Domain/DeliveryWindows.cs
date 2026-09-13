namespace OrlandoUp.Domain;

/// <summary>
/// The hours each <see cref="DeliveryWindow"/> stands for, in Orlando wall time (D3/03).
/// </summary>
/// <remarks>
/// One table, in one file, so that a driver's route changing in December is one line here and not
/// a hunt through the pages that print a window. The times are local to Orlando and carry no date:
/// a window is a pair of times of day, and the day it applies to is the booking's own
/// <c>StartDate</c> or <c>EndDate</c> (D16 — a calendar date is not an instant).
/// </remarks>
public static class DeliveryWindows
{
    /// <summary>The start and end of the window, as times of day in Orlando.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The value is not one of the four members. Thrown rather than defaulted, because a window
    /// nobody recognises must not silently become the morning run.
    /// </exception>
    public static (TimeOnly Start, TimeOnly End) Hours(DeliveryWindow window) => window switch
    {
        DeliveryWindow.Morning => (new TimeOnly(8, 0), new TimeOnly(10, 0)),
        DeliveryWindow.LateMorning => (new TimeOnly(10, 0), new TimeOnly(12, 0)),
        DeliveryWindow.Afternoon => (new TimeOnly(14, 0), new TimeOnly(16, 0)),
        DeliveryWindow.Evening => (new TimeOnly(18, 0), new TimeOnly(20, 0)),
        _ => throw new ArgumentOutOfRangeException(nameof(window), window, "Not a delivery window."),
    };
}
