namespace OrlandoUp.Domain;

/// <summary>What the fleet has, for one product, on one stretch of days.</summary>
/// <param name="UnitsAvailable">Machines of this product whose status is Available.</param>
/// <param name="BatteriesAvailable">Batteries of this product whose status is Available.</param>
/// <param name="ChargerCount">Chargers the operation owns. Any charger fits any battery.</param>
/// <param name="TurnaroundDays">The day off this product needs between two rentals.</param>
public readonly record struct FleetOnHand(
    int UnitsAvailable,
    int BatteriesAvailable,
    int ChargerCount,
    int TurnaroundDays);

/// <summary>
/// One line of an existing booking that is holding equipment, flattened to the numbers the rule
/// needs. The loader builds these; the rule never sees a booking.
/// </summary>
public sealed record HoldingLine(
    int ProductId,
    bool IsScooter,
    DateOnly Start,
    DateOnly End,
    int Quantity,
    int ExtraBatteryCount,
    int TurnaroundDays);

/// <summary>What the rule answers.</summary>
/// <param name="IsAvailable">Whether the request can be served exactly as asked.</param>
/// <param name="MaxQuantity">The most of this product that could be had on these dates.</param>
/// <param name="MaxExtraBatteries">How many second batteries are left after the requested machines.</param>
public sealed record AvailabilityResult(
    bool IsAvailable,
    int MaxQuantity,
    int MaxExtraBatteries,
    int UnitsFree,
    int BatteriesFree,
    int ChargersFree);

/// <summary>
/// Whether the fleet can serve a request, counting machines AND batteries AND chargers.
/// </summary>
/// <remarks>
/// <b>This is the decision the whole leva carries</b>, and it is a pure function over plain lists
/// on purpose: it takes no context, touches no database, and can therefore be pinned by tests that
/// state a fleet and a diary in four lines. The loader fetches; this computes.
///
/// The three bounds exist because counting machines alone would oversell. Four scooters and six
/// batteries per model means that four customers each wanting a second battery need eight
/// batteries — <b>the battery runs out before the scooter does</b> (D36), and a rule that ignored
/// batteries would promise a second one that does not exist. The chargers bind across BOTH scooter
/// models at once, because a charger fits either.
///
/// The turnaround is applied by padding the EXISTING lines and comparing against the RAW request.
/// That is symmetric — a request ending the day before an existing booking starts is caught by
/// that booking's left padding — and it keeps the reading of the number in one place.
/// </remarks>
public static class Availability
{
    /// <summary>
    /// Answers for one product over <paramref name="start"/> to <paramref name="end"/> inclusive.
    /// </summary>
    /// <param name="holdingLines">
    /// Every line of every booking whose status holds inventory, for ANY product: the rule picks
    /// out the ones it needs, and the charger bound needs the scooter lines of the other model.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="end"/> is before <paramref name="start"/>.</exception>
    public static AvailabilityResult For(
        int productId,
        bool isScooter,
        DateOnly start,
        DateOnly end,
        int quantity,
        int extraBatteries,
        FleetOnHand fleet,
        IEnumerable<HoldingLine> holdingLines)
    {
        if (end < start)
        {
            throw new ArgumentOutOfRangeException(
                nameof(end), end, "A rental cannot end before it starts.");
        }

        List<HoldingLine> lines = holdingLines.ToList();

        int busiestUnits = 0;
        int busiestBatteries = 0;
        int busiestChargers = 0;

        for (DateOnly day = start; day <= end; day = day.AddDays(1))
        {
            int unitsBusy = 0;
            int batteriesBusy = 0;
            int chargersBusy = 0;

            foreach (HoldingLine line in lines)
            {
                if (!Occupies(line, day))
                {
                    continue;
                }

                // A charger is a charger: every scooter out on this day is holding one for its own
                // battery and one more for each second battery, whichever model it is.
                if (line.IsScooter)
                {
                    chargersBusy += line.Quantity + line.ExtraBatteryCount;
                }

                if (line.ProductId != productId)
                {
                    continue;
                }

                unitsBusy += line.Quantity;
                batteriesBusy += line.Quantity + line.ExtraBatteryCount;
            }

            busiestUnits = Math.Max(busiestUnits, unitsBusy);
            busiestBatteries = Math.Max(busiestBatteries, batteriesBusy);
            busiestChargers = Math.Max(busiestChargers, chargersBusy);
        }

        // The worst day of the stretch is what decides: a machine free on four days of five does
        // not make a five-day rental possible.
        int unitsFree = fleet.UnitsAvailable - busiestUnits;
        int batteriesFree = fleet.BatteriesAvailable - busiestBatteries;
        int chargersFree = fleet.ChargerCount - busiestChargers;

        bool available = quantity <= unitsFree;

        if (isScooter)
        {
            // Deliberately NOT skipped when the pool is empty. A scooter model with no working
            // battery cannot go out at any quantity, and the rule says so rather than falling back
            // to counting machines only (D36).
            available = available
                && quantity + extraBatteries <= batteriesFree
                && quantity + extraBatteries <= chargersFree;
        }

        int maxQuantity = isScooter
            ? Math.Min(unitsFree, Math.Min(batteriesFree, chargersFree))
            : unitsFree;

        int maxExtraBatteries = isScooter
            ? Math.Min(batteriesFree, chargersFree) - quantity
            : 0;

        // Staff may enter a booking above the fleet (D4/03), which makes the raw numbers negative.
        // A floor of zero is what lets a page say "only 2 left" without ever saying "only -1 left".
        return new AvailabilityResult(
            available,
            Math.Max(0, maxQuantity),
            Math.Max(0, maxExtraBatteries),
            unitsFree,
            batteriesFree,
            chargersFree);
    }

    /// <summary>
    /// Whether an existing line still ties up equipment on <paramref name="day"/>, counting the
    /// turnaround on both sides of it.
    /// </summary>
    private static bool Occupies(HoldingLine line, DateOnly day) =>
        day >= line.Start.AddDays(-line.TurnaroundDays)
        && day <= line.End.AddDays(line.TurnaroundDays);
}
