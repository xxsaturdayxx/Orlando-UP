using Microsoft.EntityFrameworkCore;
using OrlandoUp.Application.Catalog;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data;

/// <summary>What one product on a quote was asked for, before any price is known.</summary>
public sealed record QuoteLineAsked(
    int ProductId,
    int Quantity,
    int ExtraBatteryCount,
    decimal ExtraBatteryPerDay,
    IReadOnlyList<int> AddOnIds);

/// <summary>
/// Loads the catalog a quote needs, in the booking's language, and hands it to <see cref="Quote"/>.
/// </summary>
/// <remarks>
/// <b>This is the only place in the application allowed to read a price out of the catalog.</b>
/// Once a booking exists its amounts are snapshots and the screens print columns (D7/03); the
/// control of this leva states that as a prohibition over the booking detail page and points at
/// this file as the presence half.
///
/// Names are resolved through the same fallback the public pages use, so a product with no
/// Portuguese row is quoted under its English name rather than under an empty string.
/// </remarks>
public sealed class QuoteBuilder
{
    private readonly AppDbContext _db;

    public QuoteBuilder(AppDbContext db) => _db = db;

    /// <summary>
    /// Prices the request, or returns the named reason it cannot be priced. Persists nothing.
    /// </summary>
    public async Task<QuoteResult> BuildAsync(
        IReadOnlyList<QuoteLineAsked> asked,
        int deliveryZoneId,
        DateOnly start,
        DateOnly end,
        string culture,
        CancellationToken cancellation)
    {
        if (asked.Count == 0)
        {
            return QuoteResult.Failed(QuoteProblem.NoLines);
        }

        DeliveryZone? zone = await _db.DeliveryZones
            .AsNoTracking()
            .SingleOrDefaultAsync(row => row.Id == deliveryZoneId, cancellation);

        if (zone is null)
        {
            throw new InvalidOperationException(
                $"No delivery zone with key {deliveryZoneId} to quote against.");
        }

        List<int> productIds = asked.Select(line => line.ProductId).ToList();

        List<Product> products = await _db.Products
            .AsNoTracking()
            .Include(product => product.PricingTiers)
            .Include(product => product.Translations)
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync(cancellation);

        List<int> addOnIds = asked.SelectMany(line => line.AddOnIds).Distinct().ToList();

        List<AddOn> addOns = await _db.AddOns
            .AsNoTracking()
            .Include(addOn => addOn.Translations)
            .Where(addOn => addOnIds.Contains(addOn.Id))
            .ToListAsync(cancellation);

        List<QuoteLineRequest> lines = [];

        foreach (QuoteLineAsked line in asked)
        {
            Product? product = products.SingleOrDefault(row => row.Id == line.ProductId);

            if (product is null)
            {
                throw new InvalidOperationException(
                    $"No product with key {line.ProductId} to quote.");
            }

            List<QuoteAddOnRequest> lineAddOns = [];

            foreach (int addOnId in line.AddOnIds)
            {
                AddOn? addOn = addOns.SingleOrDefault(row => row.Id == addOnId);

                if (addOn is null)
                {
                    throw new InvalidOperationException($"No add-on with key {addOnId} to quote.");
                }

                lineAddOns.Add(new QuoteAddOnRequest(
                    addOn.Id,
                    NameOf(addOn, culture),
                    addOn.PricingMode,
                    addOn.Amount));
            }

            lines.Add(new QuoteLineRequest(
                product.Id,
                NameOf(product, culture),
                line.Quantity,
                line.ExtraBatteryCount,
                line.ExtraBatteryPerDay,
                product.PricingTiers.ToList(),
                lineAddOns));
        }

        return Quote.For(new QuoteRequest(start, end, zone.DeliveryFee, zone.SalesTaxRate, lines));
    }

    private static string NameOf(Product product, string culture)
    {
        ProductTranslation? text = TranslationPicker.For(
            product.Translations, culture, row => row.Culture);

        // An untranslated product is a content defect, not a pricing one — the slug is at least
        // something a member of staff recognises, and refusing the quote would hide the booking
        // rather than the gap.
        return text is null ? product.Slug : text.Name;
    }

    private static string NameOf(AddOn addOn, string culture)
    {
        AddOnTranslation? text = TranslationPicker.For(
            addOn.Translations, culture, row => row.Culture);

        return text is null ? addOn.Code : text.Name;
    }
}
