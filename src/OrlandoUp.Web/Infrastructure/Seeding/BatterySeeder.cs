using Microsoft.EntityFrameworkCore;
using OrlandoUp.Application;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Infrastructure.Seeding;

/// <summary>
/// Writes the twelve batteries, once, and only into an empty table.
/// </summary>
/// <remarks>
/// The guard is the same one <see cref="CatalogSeeder"/> carries and it is there for the same
/// reason: the moment an administrator edits a battery, this file stops being truth and becomes
/// history. A seeder that ran twice, or that reconciled what it wrote with what it now says, would
/// undo that edit without asking.
/// </remarks>
internal static class BatterySeeder
{
    public static async Task<int> RunAsync(
        AppDbContext db,
        IClock clock,
        ILogger logger,
        CancellationToken cancellation)
    {
        if (await db.Batteries.AnyAsync(cancellation))
        {
            logger.LogInformation(
                "seed-batteries: the Batteries table is not empty, so nothing was inserted.");

            return 0;
        }

        // The models are resolved by category and display order, never by a typed slug: this file
        // is swept by C16 of public-site.tsv, which excludes one file by name and not this one.
        List<Product> scooters = await db.Products
            .Where(product => product.Category == ProductCategory.MobilityScooter)
            .OrderBy(product => product.SortOrder)
            .ToListAsync(cancellation);

        if (scooters.Count != BatterySeedData.ExpectedScooterModels)
        {
            // Refusing loudly is the whole point of resolving by position: if the catalog ever
            // holds a different number of scooter models, the seed's idea of "first" and "second"
            // has quietly stopped meaning what it meant, and stopping here is how that is noticed.
            logger.LogError(
                "seed-batteries: expected {Expected} scooter models and found {Found}; nothing was written.",
                BatterySeedData.ExpectedScooterModels,
                scooters.Count);

            return 1;
        }

        DateTime now = clock.UtcNow;

        foreach (BatterySeedData.SeedBattery seed in BatterySeedData.Batteries)
        {
            db.Batteries.Add(new Battery
            {
                ProductId = scooters[seed.ScooterIndex].Id,
                AssetTag = seed.AssetTag,
                Kind = seed.Kind,
                RangeMiles = seed.RangeMiles,
                Status = UnitStatus.Available,
                CreatedAtUtc = now,
            });
        }

        await db.SaveChangesAsync(cancellation);

        logger.LogInformation(
            "seed-batteries: wrote {Count} batteries across {Models} scooter models.",
            BatterySeedData.Batteries.Length,
            scooters.Count);

        return 0;
    }
}
