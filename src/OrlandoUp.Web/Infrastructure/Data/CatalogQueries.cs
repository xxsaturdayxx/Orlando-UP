using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrlandoUp.Application;
using OrlandoUp.Application.Catalog;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data;

/// <summary>
/// The reads the public pages do. Every one of them filters on the active flag: a product hidden
/// by an administrator disappears from the site while its history stays in the database, which is
/// the whole reason the flag exists instead of a delete.
/// </summary>
public sealed class CatalogQueries
{
    private readonly AppDbContext _db;
    private readonly RichText _richText;

    public CatalogQueries(AppDbContext db, RichText richText)
    {
        _db = db;
        _richText = richText;
    }

    public async Task<IReadOnlyList<ProductCard>> ActiveCardsAsync(string culture, CancellationToken cancellation)
    {
        List<Product> products = await _db.Products
            .AsNoTracking()
            .Where(product => product.IsActive)
            .Include(product => product.Translations)
            .Include(product => product.PricingTiers)
            .OrderBy(product => product.SortOrder)
            .ToListAsync(cancellation);

        List<ProductCard> cards = [];

        foreach (Product product in products)
        {
            ProductTranslation? text = TranslationPicker.For(product.Translations, culture, t => t.Culture);

            if (text is null)
            {
                // No row in the requested culture and none in English either: there is no name to
                // print, so the card is left out rather than shown blank.
                continue;
            }

            cards.Add(new ProductCard(
                product.Slug,
                product.Category,
                text.Name,
                text.Tagline,
                product.FitsDisneyTransport,
                product.FromPricePerDay(),
                product.ImagePath));
        }

        return cards;
    }

    public async Task<ProductDetail?> ActiveDetailAsync(string slug, string culture, CancellationToken cancellation)
    {
        Product? product = await _db.Products
            .AsNoTracking()
            .Where(candidate => candidate.IsActive && candidate.Slug == slug)
            .Include(candidate => candidate.Translations)
            .Include(candidate => candidate.PricingTiers)
            .Include(candidate => candidate.AddOns)
                .ThenInclude(link => link.AddOn!)
                    .ThenInclude(addOn => addOn.Translations)
            .FirstOrDefaultAsync(cancellation);

        if (product is null)
        {
            return null;
        }

        ProductTranslation? text = TranslationPicker.For(product.Translations, culture, t => t.Culture);

        if (text is null)
        {
            return null;
        }

        List<PricingRow> rows = product.PricingTiers
            .OrderBy(tier => tier.MinDays)
            .Select(tier => new PricingRow(tier.MinDays, tier.MaxDays, tier.Mode, tier.Amount))
            .ToList();

        List<AddOnRow> addOns = [];

        foreach (ProductAddOn link in product.AddOns.OrderBy(link => link.AddOn!.SortOrder))
        {
            AddOn addOn = link.AddOn!;

            if (!addOn.IsActive)
            {
                continue;
            }

            AddOnTranslation? addOnText = TranslationPicker.For(addOn.Translations, culture, t => t.Culture);

            if (addOnText is null)
            {
                continue;
            }

            addOns.Add(new AddOnRow(addOn.Code, addOnText.Name, addOnText.Description, addOn.PricingMode, addOn.Amount));
        }

        return new ProductDetail(
            product.Slug,
            product.Category,
            product.Configuration,
            text.Name,
            text.Tagline,
            _richText.ToHtml(text.Description),
            ReadHighlights(text.Highlights),
            product.MaxRiderWeightLb,
            product.WidthIn,
            product.LengthIn,
            product.SeatWidthIn,
            product.RangeMiles,
            product.FitsDisneyTransport,
            product.ImagePath,
            rows,
            addOns);
    }

    public async Task<IReadOnlyList<ZoneInstructions>> ActiveZonesAsync(string culture, CancellationToken cancellation)
    {
        List<DeliveryZone> zones = await _db.DeliveryZones
            .AsNoTracking()
            .Where(zone => zone.IsActive)
            .Include(zone => zone.Translations)
            .OrderBy(zone => zone.SortOrder)
            .ToListAsync(cancellation);

        List<ZoneInstructions> result = [];

        foreach (DeliveryZone zone in zones)
        {
            DeliveryZoneTranslation? text = TranslationPicker.For(zone.Translations, culture, t => t.Culture);

            if (text is null)
            {
                continue;
            }

            result.Add(new ZoneInstructions(zone.Code, text.Name, _richText.ToHtml(text.Instructions)));
        }

        return result;
    }

    private static IReadOnlyList<string> ReadHighlights(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            // A malformed highlights column is a content defect, not a reason to fail the page.
            return [];
        }
    }
}
