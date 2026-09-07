using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Seeding;

namespace OrlandoUp.Tests;

public class ProductTests
{
    [Theory]
    [InlineData(30, 48, true)]
    [InlineData(30.1, 48, false)]
    [InlineData(30, 48.1, false)]
    [InlineData(21, 41, true)]
    [InlineData(31, 52, false)]
    public void The_transport_badge_is_a_reading_of_the_dimensions(double width, double length, bool expected)
    {
        Product product = new() { WidthIn = (decimal)width, LengthIn = (decimal)length };

        Assert.Equal(expected, product.FitsDisneyTransport);
    }

    // The suffixes are load-bearing: a bare 21 boxes as Int32 and reflection refuses to hand an
    // Int32 to a double? parameter, so the case fails before it asserts anything.
    [Theory]
    [InlineData(null, null)]
    [InlineData(21d, null)]
    [InlineData(null, 41d)]
    public void An_unmeasured_unit_answers_neither_yes_nor_no_about_the_buses(double? width, double? length)
    {
        // Half a measurement is not a measurement: 21 inches wide tells nobody whether the thing is
        // shorter than 48 inches. The badge is shown on true and on nothing else, so the absence
        // has to survive as an absence all the way to the page.
        Product product = new()
        {
            WidthIn = (decimal?)width,
            LengthIn = (decimal?)length,
        };

        Assert.Null(product.FitsDisneyTransport);
    }

    [Fact]
    public void A_product_nobody_decided_about_is_not_on_sale()
    {
        // Fail closed (D32). The store default fills the rows that existed when the column was
        // added; this is the rule for every row born after it, and it is the opposite of IsActive,
        // whose default is true because a product is written to be shown.
        Product product = new();

        Assert.False(product.IsBookable);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void The_advertised_daily_price_is_the_lowest_daily_band()
    {
        Product product = new()
        {
            PricingTiers =
            [
                new PricingTier { MinDays = 1, MaxDays = 2, Mode = TierMode.FlatPerRental, Amount = 75m },
                new PricingTier { MinDays = 3, MaxDays = 6, Mode = TierMode.PerDay, Amount = 32m },
                new PricingTier { MinDays = 7, MaxDays = null, Mode = TierMode.PerDay, Amount = 27m },
            ],
        };

        Assert.Equal(27m, product.FromPricePerDay());
    }

    [Fact]
    public void A_flat_band_is_read_as_a_daily_price_by_its_own_length()
    {
        Product product = new()
        {
            PricingTiers = [new PricingTier { MinDays = 1, MaxDays = 2, Mode = TierMode.FlatPerRental, Amount = 75m }],
        };

        Assert.Equal(37.50m, product.FromPricePerDay());
    }

    [Fact]
    public void A_product_with_no_readable_band_advertises_nothing_rather_than_zero()
    {
        // The point of D15: absence stays absence. A zero here would read as a free rental, and no
        // assertion about a NUMBER can tell those two apart, which is why control C17 is a control
        // of FORM.
        Product product = new()
        {
            PricingTiers = [new PricingTier { MinDays = 1, MaxDays = null, Mode = TierMode.FlatPerRental, Amount = 75m }],
        };

        Assert.Null(product.FromPricePerDay());
    }

    [Fact]
    public void A_product_with_no_bands_at_all_advertises_nothing()
    {
        Assert.Null(new Product().FromPricePerDay());
    }
}

public class PricingTierRulesTests
{
    [Fact]
    public void A_price_list_that_starts_at_one_day_and_reaches_the_open_end_is_valid()
    {
        PricingTier[] tiers =
        [
            new() { MinDays = 1, MaxDays = 2, Mode = TierMode.FlatPerRental, Amount = 75m },
            new() { MinDays = 3, MaxDays = 6, Mode = TierMode.PerDay, Amount = 32m },
            new() { MinDays = 7, MaxDays = null, Mode = TierMode.PerDay, Amount = 27m },
        ];

        Assert.Equal(PricingTierSetProblem.None, PricingTierRules.Validate(tiers));
    }

    [Fact]
    public void Two_bands_that_cover_the_same_length_are_refused()
    {
        PricingTier[] tiers =
        [
            new() { MinDays = 1, MaxDays = 4, Mode = TierMode.FlatPerRental, Amount = 75m },
            new() { MinDays = 3, MaxDays = null, Mode = TierMode.PerDay, Amount = 27m },
        ];

        Assert.Equal(PricingTierSetProblem.Overlap, PricingTierRules.Validate(tiers));
    }

    [Fact]
    public void A_length_covered_by_no_band_is_refused()
    {
        PricingTier[] tiers =
        [
            new() { MinDays = 1, MaxDays = 2, Mode = TierMode.FlatPerRental, Amount = 75m },
            new() { MinDays = 5, MaxDays = null, Mode = TierMode.PerDay, Amount = 27m },
        ];

        Assert.Equal(PricingTierSetProblem.Gap, PricingTierRules.Validate(tiers));
    }

    [Fact]
    public void A_price_list_that_does_not_start_at_one_day_is_refused()
    {
        PricingTier[] tiers = [new() { MinDays = 2, MaxDays = null, Mode = TierMode.PerDay, Amount = 27m }];

        Assert.Equal(PricingTierSetProblem.DoesNotStartAtOneDay, PricingTierRules.Validate(tiers));
    }

    [Fact]
    public void A_price_list_that_stops_covering_is_refused()
    {
        PricingTier[] tiers = [new() { MinDays = 1, MaxDays = 6, Mode = TierMode.PerDay, Amount = 27m }];

        Assert.Equal(PricingTierSetProblem.NoOpenEndedBand, PricingTierRules.Validate(tiers));
    }

    [Fact]
    public void An_empty_price_list_is_refused()
    {
        Assert.Equal(PricingTierSetProblem.Empty, PricingTierRules.Validate([]));
    }

    [Fact]
    public void Every_seeded_product_carries_a_valid_price_list()
    {
        // Two sides, because the seed now has two kinds of product. One on sale must cover every
        // rental length with no gap and no overlap; one not on sale must carry no price at all,
        // since a price nobody can pay is the number a visitor remembers and quotes back (D32).
        int onSale = 0;
        int comingSoon = 0;

        foreach (SeedProduct seed in CatalogSeedData.Products)
        {
            PricingTier[] tiers = seed.Tiers
                .Select(tier => new PricingTier
                {
                    MinDays = tier.MinDays,
                    MaxDays = tier.MaxDays,
                    Mode = tier.Mode,
                    Amount = tier.Amount,
                })
                .ToArray();

            if (seed.IsBookable)
            {
                onSale++;
                Assert.Equal(PricingTierSetProblem.None, PricingTierRules.Validate(tiers));
            }
            else
            {
                comingSoon++;
                Assert.Empty(tiers);
                Assert.Empty(seed.AddOnCodes);
            }
        }

        // Reach: neither branch may be the empty one, or half of this test proves nothing.
        Assert.True(onSale > 0 && comingSoon > 0, $"on sale: {onSale}, coming soon: {comingSoon}");
    }

    [Fact]
    public void Only_a_product_on_sale_carries_units()
    {
        foreach (SeedProduct seed in CatalogSeedData.Products)
        {
            if (seed.IsBookable)
            {
                Assert.True(seed.UnitCount > 0, $"{seed.Slug} is on sale with no unit behind it");
            }
            else
            {
                Assert.Equal(0, seed.UnitCount);
            }
        }
    }

    [Fact]
    public void No_seeded_product_carries_a_dimension_that_was_never_measured()
    {
        // Width and length are either both known or both absent. Half a measurement cannot answer
        // the question the badge asks, and the page has no way to say "we measured one side".
        foreach (SeedProduct seed in CatalogSeedData.Products)
        {
            Assert.Equal(seed.WidthIn is null, seed.LengthIn is null);
        }
    }
}
