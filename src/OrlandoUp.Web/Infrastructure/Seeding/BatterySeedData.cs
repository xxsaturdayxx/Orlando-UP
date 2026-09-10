using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Seeding;

/// <summary>
/// The twelve batteries the operation owns on 2026-09-10, as D37 and D38 describe them.
/// </summary>
/// <remarks>
/// <b>No catalog identifier is typed in this file, and that is a control and not a preference.</b>
/// C16 of <c>public-site.tsv</c> sweeps all of <c>src/</c> for the eleven catalog identifiers and
/// excludes exactly one file, by the name <c>CatalogSeedData.cs</c> — this file is not that one. So
/// a battery names its model by <b>position among the scooters</b>, and the seeder resolves the
/// position against the database, ordered the way the catalog itself orders products. The seeder
/// refuses outright when it does not find exactly two scooters, which is what keeps that coupling
/// visible instead of silent.
///
/// The tag and the grade are separate columns because they are separate facts (D38): the tag
/// identifies the object, the <see cref="BatteryKind"/> describes it. Nothing anywhere reads a
/// grade out of tag text.
/// </remarks>
internal static class BatterySeedData
{
    /// <summary>How many scooter models the seed expects to find, by position.</summary>
    public const int ExpectedScooterModels = 2;

    /// <summary>The first scooter by display order — the one whose batteries carry BSC.</summary>
    public const int FirstScooter = 0;

    /// <summary>The second scooter by display order — the one whose batteries carry BSP.</summary>
    public const int SecondScooter = 1;

    internal sealed record SeedBattery(int ScooterIndex, string AssetTag, BatteryKind Kind, decimal? RangeMiles);

    public static readonly SeedBattery[] Batteries =
    [
        // Five Normal and one Extended Range on the first scooter: Rod owned two XL and a customer
        // broke one (D37). BSC-06 is the survivor, and only the Kind column says so.
        new(FirstScooter, "BSC-01", BatteryKind.Normal, 9m),
        new(FirstScooter, "BSC-02", BatteryKind.Normal, 9m),
        new(FirstScooter, "BSC-03", BatteryKind.Normal, 9m),
        new(FirstScooter, "BSC-04", BatteryKind.Normal, 9m),
        new(FirstScooter, "BSC-05", BatteryKind.Normal, 9m),
        new(FirstScooter, "BSC-06", BatteryKind.ExtendedRange, 14m),

        // All six Normal on the second scooter: D37 confirms only the first model's XL is known to
        // exist, and Q15 asks whether one of the other model ever did. A row is added through the
        // screen the day one turns up.
        new(SecondScooter, "BSP-01", BatteryKind.Normal, 9m),
        new(SecondScooter, "BSP-02", BatteryKind.Normal, 9m),
        new(SecondScooter, "BSP-03", BatteryKind.Normal, 9m),
        new(SecondScooter, "BSP-04", BatteryKind.Normal, 9m),
        new(SecondScooter, "BSP-05", BatteryKind.Normal, 9m),
        new(SecondScooter, "BSP-06", BatteryKind.Normal, 9m),
    ];
}
