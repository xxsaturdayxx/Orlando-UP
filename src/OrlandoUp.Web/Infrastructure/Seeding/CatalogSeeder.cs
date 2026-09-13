using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrlandoUp.Application;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Infrastructure.Seeding;

/// <summary>
/// Writes the placeholder catalog, once, and only into an empty table.
/// </summary>
/// <remarks>
/// The guard is the whole point: these rows become editable content the moment an administrator
/// touches them, and a seeder that ran twice, or that reconciled what it wrote with what it now
/// says, would overwrite that edit without asking. So it inserts into emptiness or does nothing.
/// </remarks>
internal static class CatalogSeeder
{
    private static readonly JsonSerializerOptions HighlightsJson = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static async Task<int> RunAsync(
        AppDbContext db,
        IClock clock,
        ILogger logger,
        CancellationToken cancellation)
    {
        if (await db.Products.AnyAsync(cancellation))
        {
            logger.LogInformation(
                "seed-catalog: the Products table is not empty, so nothing was inserted.");

            return 0;
        }

        DateTime now = clock.UtcNow;

        Dictionary<string, AddOn> addOnsByCode = [];

        foreach (SeedAddOn seed in CatalogSeedData.AddOns)
        {
            AddOn addOn = new()
            {
                Code = seed.Code,
                PricingMode = seed.PricingMode,
                Amount = seed.Amount,
                IsActive = true,
                SortOrder = seed.SortOrder,
            };

            foreach (SeedAddOnText text in seed.Texts)
            {
                addOn.Translations.Add(new AddOnTranslation
                {
                    Culture = text.Culture,
                    Name = text.Name,
                    Description = text.Description,
                });
            }

            addOnsByCode[seed.Code] = addOn;
            db.AddOns.Add(addOn);
        }

        foreach (SeedProduct seed in CatalogSeedData.Products)
        {
            Product product = new()
            {
                Slug = seed.Slug,
                Category = seed.Category,
                Configuration = seed.Configuration,
                MaxRiderWeightLb = seed.MaxRiderWeightLb,
                WidthIn = seed.WidthIn,
                LengthIn = seed.LengthIn,
                SeatWidthIn = seed.SeatWidthIn,
                RangeMiles = seed.RangeMiles,

                // A day off between two rentals of the same machine, for cleaning, charge and
                // check (D2/03). It is seed data now rather than a literal, so the number is a
                // fact about each product and the availability rule reads it per product.
                TurnaroundDays = seed.TurnaroundDays,
                IsActive = true,
                SortOrder = seed.SortOrder,
                IsBookable = seed.IsBookable,
                ImagePath = null,
                CreatedAtUtc = now,
            };

            foreach (SeedText text in seed.Texts)
            {
                product.Translations.Add(new ProductTranslation
                {
                    Culture = text.Culture,
                    Name = text.Name,
                    Tagline = text.Tagline,
                    Description = text.Description,
                    Highlights = JsonSerializer.Serialize(text.Highlights, HighlightsJson),
                });
            }

            foreach (SeedTier tier in seed.Tiers)
            {
                product.PricingTiers.Add(new PricingTier
                {
                    MinDays = tier.MinDays,
                    MaxDays = tier.MaxDays,
                    Mode = tier.Mode,
                    Amount = tier.Amount,
                });
            }

            // The guard has two sides, and both matter. A product on sale is refused unless its
            // price list covers every rental length without a gap and without an overlap: a gap
            // would advertise a length nobody can be charged for. A product NOT on sale is refused
            // if it carries a price list at all, because a price nobody can pay is worse than no
            // price - it is the number a visitor remembers and quotes back on the phone (D32).
            if (seed.IsBookable)
            {
                PricingTierSetProblem problem = PricingTierRules.Validate(product.PricingTiers);

                if (problem != PricingTierSetProblem.None)
                {
                    logger.LogError(
                        "seed-catalog: the price list of {Slug} is invalid ({Problem}); nothing was written.",
                        seed.Slug,
                        problem);

                    return 1;
                }
            }
            else if (product.PricingTiers.Count > 0 || seed.AddOnCodes.Length > 0)
            {
                logger.LogError(
                    "seed-catalog: {Slug} is not on sale and must carry no price and no add-on; nothing was written.",
                    seed.Slug);

                return 1;
            }

            // One row per physical unit, tagged the way the labels are read in the warehouse. A
            // product with no unit gets none, which is what makes the admin count the fleet rather
            // than the catalog.
            for (int number = 1; number <= seed.UnitCount; number++)
            {
                product.Units.Add(new Unit
                {
                    AssetTag = $"{seed.Slug.ToUpperInvariant()}-{number:000}",
                    Status = UnitStatus.Available,
                    CreatedAtUtc = now,
                });
            }

            foreach (string code in seed.AddOnCodes)
            {
                if (!addOnsByCode.TryGetValue(code, out AddOn? addOn))
                {
                    logger.LogError("seed-catalog: {Slug} asks for the unknown add-on {Code}.", seed.Slug, code);

                    return 1;
                }

                product.AddOns.Add(new ProductAddOn { AddOn = addOn });
            }

            db.Products.Add(product);
        }

        foreach (SeedZone seed in CatalogSeedData.Zones)
        {
            DeliveryZone zone = new()
            {
                Code = seed.Code,
                Kind = seed.Kind,
                DeliveryFee = seed.DeliveryFee,
                HandoverMode = seed.HandoverMode,
                SalesTaxRate = 0m,
                IsActive = true,
                SortOrder = seed.SortOrder,
            };

            foreach (SeedZoneText text in seed.Texts)
            {
                zone.Translations.Add(new DeliveryZoneTranslation
                {
                    Culture = text.Culture,
                    Name = text.Name,
                    Instructions = text.Instructions,
                });
            }

            int order = 1;

            foreach (string name in seed.LocationNames)
            {
                zone.Locations.Add(new DeliveryLocation
                {
                    Name = name,
                    IsActive = true,
                    SortOrder = order,
                });

                order++;
            }

            db.DeliveryZones.Add(zone);
        }

        int rows = await db.SaveChangesAsync(cancellation);

        logger.LogInformation(
            "seed-catalog: wrote {Products} products, {AddOns} add-ons and {Zones} zones ({Rows} rows).",
            CatalogSeedData.Products.Length,
            CatalogSeedData.AddOns.Length,
            CatalogSeedData.Zones.Length,
            rows);

        return 0;
    }
}
