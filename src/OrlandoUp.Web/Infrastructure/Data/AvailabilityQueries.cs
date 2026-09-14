using Microsoft.EntityFrameworkCore;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data;

/// <summary>
/// Fetches what <see cref="Availability"/> needs and calls it. It decides nothing.
/// </summary>
/// <remarks>
/// Everything is pulled into memory first, and the sums and maxima happen in C#. That is not
/// laziness: the test host is SQLite, which cannot translate every aggregate the rule would want,
/// and a rule that computed differently under test than in production would be measured by a suite
/// that does not exercise it. The quantities summed here are integers in any case, and <b>no money
/// is ever aggregated in SQL anywhere in this leva</b>.
///
/// The settings row is read with <c>SingleAsync</c> and its absence is allowed to throw (D12/03).
/// Projecting it through <c>FirstOrDefault</c> onto a value type would turn a missing row into a
/// zero, and a zero charger count is a claim about the fleet rather than a report of a broken
/// deployment — the lesson of the previous leva's dashboard.
/// </remarks>
public sealed class AvailabilityQueries
{
    private readonly AppDbContext _db;

    public AvailabilityQueries(AppDbContext db) => _db = db;

    /// <summary>
    /// Whether the fleet can serve <paramref name="quantity"/> of <paramref name="productId"/>,
    /// with <paramref name="extraBatteries"/> second batteries, from <paramref name="start"/> to
    /// <paramref name="end"/> inclusive.
    /// </summary>
    /// <param name="alsoHolding">
    /// Lines that are not in the database yet but will be if this request is accepted — the OTHER
    /// lines of the same booking. Without them each line is measured against the diary alone, and
    /// two lines that each fit can be written together into a fleet that cannot serve both: the
    /// chargers are one pool shared by both scooter models, so a Scout line and a Spitfire line
    /// add up even though neither touches the other's machines.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// The <c>OperationalSettings</c> row is absent. A host without it is a deployment defect and
    /// is reported as one, never quietly treated as an operation that owns no chargers.
    /// </exception>
    public async Task<AvailabilityResult> ForProductAsync(
        int productId,
        DateOnly start,
        DateOnly end,
        int quantity,
        int extraBatteries,
        CancellationToken cancellation,
        IReadOnlyList<HoldingLine>? alsoHolding = null)
    {
        IReadOnlyDictionary<int, ProductFacts> products = await ProductFactsAsync(cancellation);

        if (!products.TryGetValue(productId, out ProductFacts? product))
        {
            throw new InvalidOperationException(
                $"No product with key {productId} to check availability for.");
        }

        int chargerCount = await _db.OperationalSettings
            .AsNoTracking()
            .Select(row => row.ChargerCount)
            .SingleAsync(cancellation);

        int unitsAvailable = await _db.Units
            .AsNoTracking()
            .CountAsync(unit => unit.ProductId == productId && unit.Status == UnitStatus.Available, cancellation);

        int batteriesAvailable = await _db.Batteries
            .AsNoTracking()
            .CountAsync(battery => battery.ProductId == productId && battery.Status == UnitStatus.Available, cancellation);

        IReadOnlyList<HoldingLine> holding = await HoldingLinesAsync(start, end, products, cancellation);

        if (alsoHolding is { Count: > 0 })
        {
            holding = [.. holding, .. alsoHolding];
        }

        return Availability.For(
            productId,
            product.IsScooter,
            start,
            end,
            quantity,
            extraBatteries,
            new FleetOnHand(unitsAvailable, batteriesAvailable, chargerCount, product.TurnaroundDays),
            holding);
    }

    /// <summary>
    /// The category and turnaround of every product, keyed by id. Read whole because the charger
    /// bound spans both scooter models and the padding is per product.
    /// </summary>
    public async Task<IReadOnlyDictionary<int, ProductFacts>> ProductFactsAsync(CancellationToken cancellation)
    {
        List<ProductFacts> rows = await _db.Products
            .AsNoTracking()
            .Select(product => new ProductFacts(
                product.Id,
                product.Category == ProductCategory.MobilityScooter,
                product.TurnaroundDays))
            .ToListAsync(cancellation);

        return rows.ToDictionary(row => row.ProductId);
    }

    /// <summary>
    /// Every line of every booking that is holding equipment anywhere near this stretch of days.
    /// </summary>
    private async Task<IReadOnlyList<HoldingLine>> HoldingLinesAsync(
        DateOnly start,
        DateOnly end,
        IReadOnlyDictionary<int, ProductFacts> products,
        CancellationToken cancellation)
    {
        // The window is widened by the largest turnaround any product declares, and it is READ
        // rather than assumed: a constant here would silently stop matching the day somebody sets
        // a product's turnaround to three on the catalog screen.
        int widest = products.Count == 0 ? 0 : products.Values.Max(product => product.TurnaroundDays);

        DateOnly from = start.AddDays(-widest);
        DateOnly to = end.AddDays(widest);

        var rows = await _db.BookingLines
            .AsNoTracking()
            .Where(line => line.Booking!.StartDate <= to && line.Booking.EndDate >= from)
            .Select(line => new
            {
                line.ProductId,
                line.Quantity,
                line.ExtraBatteryCount,
                line.Booking!.Status,
                line.Booking.StartDate,
                line.Booking.EndDate,
            })
            .ToListAsync(cancellation);

        List<HoldingLine> holding = [];

        foreach (var row in rows)
        {
            // Filtered here and not in SQL: HoldsInventory is the one place that answers this, and
            // translating it into a query would be a second copy of the set it defines.
            if (!BookingStatusRules.HoldsInventory(row.Status))
            {
                continue;
            }

            if (!products.TryGetValue(row.ProductId, out ProductFacts? product))
            {
                continue;
            }

            holding.Add(new HoldingLine(
                row.ProductId,
                product.IsScooter,
                row.StartDate,
                row.EndDate,
                row.Quantity,
                row.ExtraBatteryCount,
                product.TurnaroundDays));
        }

        return holding;
    }
}

/// <summary>The two things availability needs to know about a product.</summary>
public sealed record ProductFacts(int ProductId, bool IsScooter, int TurnaroundDays);
