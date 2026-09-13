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
        // Fail closed (D32), and both assertions are about the C# INITIALISERS on Product, not
        // about the database: since leva 04 no boolean column carries a store default at all
        // (D34), so there is nothing in the schema left to confuse these two answers with.
        // IsBookable is left at the type's own default because a product nobody decided about must
        // not be on sale; IsActive is initialised true because a product is written to be shown.
        // The admin's create handler overrides that true and writes false, so a half-filled form
        // cannot publish anything — which is a decision of the handler, not of this class.
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

/// <summary>
/// The rule that decides what the site may promise, and the price it may promise it at. Pure
/// functions, no host, no database — which is what lets a fleet and a diary be stated in four
/// lines and the answer be read off.
/// </summary>
public class BookingRuleTests
{
    // The seed fleet, as D36 describes it: four machines and six batteries per scooter model,
    // fourteen chargers between them, one day of turnaround.
    private const int ScoutId = 1;
    private const int SpitfireId = 2;
    private const int WheelchairId = 3;

    private static FleetOnHand ScooterFleet(int units = 4, int batteries = 6, int chargers = 14, int turnaround = 1) =>
        new(units, batteries, chargers, turnaround);

    private static HoldingLine Line(
        int productId, string start, string end, int quantity, int extras, bool isScooter = true, int turnaround = 1) =>
        new(productId, isScooter, DateOnly.Parse(start), DateOnly.Parse(end), quantity, extras, turnaround);

    private static AvailabilityResult Ask(
        int productId, string start, string end, int quantity, int extras, FleetOnHand fleet, params HoldingLine[] holding) =>
        Availability.For(productId, true, DateOnly.Parse(start), DateOnly.Parse(end), quantity, extras, fleet, holding);

    // ---------------------------------------------------------------------------------------
    // The status machine
    // ---------------------------------------------------------------------------------------

    [Fact]
    public void Exactly_five_statuses_hold_equipment()
    {
        BookingStatus[] holding =
        [
            BookingStatus.PendingPayment, BookingStatus.Confirmed, BookingStatus.Scheduled,
            BookingStatus.OutForDelivery, BookingStatus.Active,
        ];

        // Both directions, because a set assertion that only checks one side passes when the
        // implementation returns true for everything.
        foreach (BookingStatus status in holding)
        {
            Assert.True(BookingStatusRules.HoldsInventory(status), $"{status} should hold");
        }

        foreach (BookingStatus status in Enum.GetValues<BookingStatus>().Except(holding))
        {
            Assert.False(BookingStatusRules.HoldsInventory(status), $"{status} should not hold");
        }

        Assert.Equal(5, Enum.GetValues<BookingStatus>().Count(BookingStatusRules.HoldsInventory));
    }

    [Theory]
    [InlineData(BookingStatus.Draft, BookingStatus.PendingPayment, true)]
    [InlineData(BookingStatus.PendingPayment, BookingStatus.Confirmed, true)]
    [InlineData(BookingStatus.Confirmed, BookingStatus.Scheduled, true)]
    [InlineData(BookingStatus.Scheduled, BookingStatus.OutForDelivery, true)]
    [InlineData(BookingStatus.OutForDelivery, BookingStatus.Active, true)]
    [InlineData(BookingStatus.Active, BookingStatus.PickedUp, true)]
    [InlineData(BookingStatus.PickedUp, BookingStatus.Completed, true)]
    [InlineData(BookingStatus.Confirmed, BookingStatus.Cancelled, true)]
    [InlineData(BookingStatus.Cancelled, BookingStatus.Refunded, true)]
    // The illegal ones the spec names, and they are what give the legal ones their meaning.
    [InlineData(BookingStatus.Cancelled, BookingStatus.Confirmed, false)]
    [InlineData(BookingStatus.Completed, BookingStatus.Active, false)]
    [InlineData(BookingStatus.Confirmed, BookingStatus.Confirmed, false)]
    [InlineData(BookingStatus.Draft, BookingStatus.Active, false)]
    [InlineData(BookingStatus.Refunded, BookingStatus.Confirmed, false)]
    public void The_transition_table_says_what_the_architecture_says(BookingStatus from, BookingStatus to, bool legal)
    {
        Assert.Equal(legal, BookingStatusRules.CanTransition(from, to));
    }

    [Fact]
    public void Cancelling_records_the_instant_and_the_reason_and_refuses_the_second_time()
    {
        DateTime when = new(2026, 12, 20, 15, 4, 0, DateTimeKind.Utc);
        Booking booking = StaffBooking();

        Assert.Equal(BookingStatus.Confirmed, booking.Status);

        booking.Cancel(when, "Customer changed dates");

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal(when, booking.CancelledAtUtc);
        Assert.Equal("Customer changed dates", booking.CancelReason);

        Assert.Throws<InvalidOperationException>(() => booking.Cancel(when, "again"));
    }

    // ---------------------------------------------------------------------------------------
    // Availability — the four scenes of the spec, with their numbers
    // ---------------------------------------------------------------------------------------

    [Fact]
    public void Scene_one_the_battery_runs_out_before_the_scooter_does()
    {
        // Three Scouts already out, each with a second battery: six batteries, the whole pool.
        HoldingLine[] booked =
        [
            Line(ScoutId, "2026-12-20", "2026-12-24", 1, 1),
            Line(ScoutId, "2026-12-20", "2026-12-24", 1, 1),
            Line(ScoutId, "2026-12-20", "2026-12-24", 1, 1),
        ];

        AvailabilityResult fourth = Ask(ScoutId, "2026-12-21", "2026-12-23", 1, 0, ScooterFleet(), booked);

        // The machine is there and the battery is not, which is the whole of D36 in two numbers.
        Assert.Equal(1, fourth.UnitsFree);
        Assert.Equal(0, fourth.BatteriesFree);
        Assert.False(fourth.IsAvailable);

        // And the presence half: the same request against an empty diary is available, so the
        // refusal above is the bookings talking and not the rule refusing everything.
        Assert.True(Ask(ScoutId, "2026-12-21", "2026-12-23", 1, 0, ScooterFleet()).IsAvailable);
    }

    [Fact]
    public void Scene_two_the_turnaround_keeps_the_day_after_a_rental_busy()
    {
        HoldingLine[] booked = [Line(ScoutId, "2026-12-10", "2026-12-12", 1, 0)];

        // Padded, that line occupies 9 to 13 December: on the 13th one machine is still out.
        AvailabilityResult onThe13th = Ask(ScoutId, "2026-12-13", "2026-12-13", 4, 0, ScooterFleet(), booked);

        Assert.Equal(3, onThe13th.UnitsFree);
        Assert.False(onThe13th.IsAvailable);
        Assert.True(Ask(ScoutId, "2026-12-13", "2026-12-13", 3, 0, ScooterFleet(), booked).IsAvailable);

        // One day later the padding has run out and the whole fleet is free again.
        Assert.True(Ask(ScoutId, "2026-12-14", "2026-12-14", 4, 0, ScooterFleet(), booked).IsAvailable);
    }

    [Fact]
    public void Scene_three_a_cancelled_booking_gives_the_equipment_back()
    {
        // The loader only ever passes lines whose status holds inventory, so a cancelled booking
        // reaches the rule as an absence — which is exactly an empty diary.
        Assert.True(Ask(ScoutId, "2026-12-13", "2026-12-13", 4, 0, ScooterFleet()).IsAvailable);

        Assert.False(BookingStatusRules.HoldsInventory(BookingStatus.Cancelled));
    }

    [Theory]
    // 22:59 UTC on 5 December is 17:59 in Orlando, one minute before a cut-off of eighteen.
    [InlineData("2026-12-05T22:59:00Z", "2026-12-06")]
    [InlineData("2026-12-05T23:00:00Z", "2026-12-07")]
    // The same pair in July, when Orlando is on daylight time and the offset is an hour smaller.
    [InlineData("2026-07-05T21:59:00Z", "2026-07-06")]
    [InlineData("2026-07-05T22:00:00Z", "2026-07-07")]
    public void Scene_four_the_cut_off_reads_the_orlando_wall_clock_on_both_sides_of_the_switch(
        string instant, string earliest)
    {
        FakeClock clock = new(DateTime.Parse(
            instant, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal));

        Assert.Equal(
            DateOnly.Parse(earliest, System.Globalization.CultureInfo.InvariantCulture),
            BookingRules.EarliestPublicStart(clock.NowInOrlando(), 18));
    }

    [Fact]
    public void Staff_may_enter_today_and_the_cut_off_does_not_bind_them()
    {
        FakeClock clock = new(new DateTime(2026, 12, 5, 23, 30, 0, DateTimeKind.Utc));

        // The public page is already on the 7th at this hour; staff are still on the 5th.
        Assert.Equal(new DateOnly(2026, 12, 7), BookingRules.EarliestPublicStart(clock.NowInOrlando(), 18));
        Assert.Equal(clock.TodayInOrlando(), BookingRules.EarliestStaffStart(clock.TodayInOrlando()));
        Assert.Equal(new DateOnly(2026, 12, 5), BookingRules.EarliestStaffStart(clock.TodayInOrlando()));
    }

    // ---------------------------------------------------------------------------------------
    // Availability — the bounds each pool imposes
    // ---------------------------------------------------------------------------------------

    [Fact]
    public void A_machine_in_maintenance_and_a_retired_battery_are_simply_not_counted()
    {
        // The loader counts only Available rows, so a fleet with one machine under maintenance
        // arrives here as three units and one broken battery as five.
        FleetOnHand reduced = ScooterFleet(units: 3, batteries: 5);

        Assert.True(Ask(ScoutId, "2026-12-20", "2026-12-22", 3, 2, reduced).IsAvailable);
        Assert.False(Ask(ScoutId, "2026-12-20", "2026-12-22", 4, 0, reduced).IsAvailable);
        Assert.False(Ask(ScoutId, "2026-12-20", "2026-12-22", 3, 3, reduced).IsAvailable);
    }

    [Fact]
    public void A_scooter_model_with_no_working_battery_cannot_go_out_at_all()
    {
        FleetOnHand noBatteries = ScooterFleet(batteries: 0);

        Assert.False(Ask(ScoutId, "2026-12-20", "2026-12-20", 1, 0, noBatteries).IsAvailable);

        // The presence half: the machines really are there, so the refusal is the battery pool.
        Assert.Equal(4, Ask(ScoutId, "2026-12-20", "2026-12-20", 1, 0, noBatteries).UnitsFree);
    }

    [Fact]
    public void A_wheelchair_ignores_batteries_and_chargers()
    {
        // Two chairs, no battery pool and no chargers left anywhere: it still goes out, because
        // there is nothing to charge.
        FleetOnHand chairs = new(UnitsAvailable: 2, BatteriesAvailable: 0, ChargerCount: 0, TurnaroundDays: 1);

        AvailabilityResult answer = Availability.For(
            WheelchairId, false, new DateOnly(2026, 12, 20), new DateOnly(2026, 12, 24), 2, 0, chairs, []);

        Assert.True(answer.IsAvailable);
        Assert.Equal(2, answer.MaxQuantity);
        Assert.Equal(0, answer.MaxExtraBatteries);
    }

    [Fact]
    public void The_charger_count_binds_across_both_scooter_models_at_once()
    {
        // Three of each model, each with a second battery: six batteries per pool — both exactly
        // full — and twelve chargers of the fourteen.
        HoldingLine[] booked =
        [
            Line(ScoutId, "2026-12-20", "2026-12-24", 3, 3),
            Line(SpitfireId, "2026-12-20", "2026-12-24", 3, 3),
        ];

        FleetOnHand fleet = ScooterFleet();

        AvailabilityResult oneMoreScout = Ask(ScoutId, "2026-12-21", "2026-12-22", 1, 0, fleet, booked);

        // Two chargers are still free, so the fourteen are not what refuses; the model's own
        // battery pool, at zero, is. The two numbers say which is which.
        Assert.Equal(2, oneMoreScout.ChargersFree);
        Assert.Equal(0, oneMoreScout.BatteriesFree);
        Assert.False(oneMoreScout.IsAvailable);

        // Against an empty diary the same fleet serves it, so the refusal is the diary talking.
        Assert.True(Ask(ScoutId, "2026-12-21", "2026-12-22", 1, 0, fleet).IsAvailable);
    }

    [Fact]
    public void The_page_is_told_how_many_are_left_rather_than_only_that_it_is_full()
    {
        HoldingLine[] booked = [Line(ScoutId, "2026-12-20", "2026-12-24", 2, 0)];

        AvailabilityResult answer = Ask(ScoutId, "2026-12-21", "2026-12-22", 4, 0, ScooterFleet(), booked);

        Assert.False(answer.IsAvailable);
        Assert.Equal(2, answer.MaxQuantity);
    }

    [Fact]
    public void An_overbooked_diary_never_produces_a_negative_offer()
    {
        // Six machines out of a fleet of four: staff entered above the fleet, which is allowed.
        HoldingLine[] booked = [Line(ScoutId, "2026-12-20", "2026-12-24", 6, 6)];

        AvailabilityResult answer = Ask(ScoutId, "2026-12-21", "2026-12-22", 1, 0, ScooterFleet(), booked);

        Assert.False(answer.IsAvailable);
        Assert.Equal(0, answer.MaxQuantity);
        Assert.Equal(0, answer.MaxExtraBatteries);

        // The raw numbers are negative, and that is what the floors above are hiding from the page.
        Assert.Equal(-2, answer.UnitsFree);
    }

    [Fact]
    public void A_rental_that_ends_before_it_starts_is_refused_and_not_computed()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Availability.For(ScoutId, true, new DateOnly(2026, 12, 20), new DateOnly(2026, 12, 19),
                1, 0, ScooterFleet(), []));
    }

    // ---------------------------------------------------------------------------------------
    // The quote
    // ---------------------------------------------------------------------------------------

    [Theory]
    [InlineData("2026-12-20", "2026-12-21", 75)]
    [InlineData("2026-12-20", "2026-12-24", 160)]
    [InlineData("2026-12-20", "2026-12-26", 189)]
    public void A_rental_is_priced_by_the_band_that_covers_its_length(string start, string end, int expected)
    {
        QuoteResult result = Quote.For(Request(start, end, ScoutLine()));

        Assert.Equal(QuoteProblem.None, result.Problem);
        Assert.Equal(expected, result.Breakdown!.Subtotal);
    }

    [Fact]
    public void The_second_battery_and_the_extras_are_priced_by_their_own_modes()
    {
        // Five days: a second battery at 8.00 a day, a sunshade at 3.00 a day, a cup holder at
        // 5.00 once.
        QuoteLineRequest line = ScoutLine(extraBatteries: 1, perDay: 8m, addOns:
        [
            new QuoteAddOnRequest(10, "Sunshade", AddOnPricingMode.PerDay, 3m),
            new QuoteAddOnRequest(11, "Cup holder", AddOnPricingMode.PerRental, 5m),
        ]);

        QuoteBreakdown price = Quote.For(Request("2026-12-20", "2026-12-24", line)).Breakdown!;

        Assert.Equal(160m, price.Subtotal);
        Assert.Equal(40m, price.ExtraBatteriesTotal);
        Assert.Equal(20m, price.AddOnsTotal);
        Assert.Equal(220m, price.Total);
    }

    [Fact]
    public void The_tax_is_rounded_once_away_from_zero_on_the_taxable_total()
    {
        // 160 rental + 40 battery + 20 extras + 25 delivery = 245, at 6.5 per cent = 15.925.
        QuoteLineRequest line = ScoutLine(extraBatteries: 1, perDay: 8m, addOns:
        [
            new QuoteAddOnRequest(10, "Sunshade", AddOnPricingMode.PerDay, 3m),
            new QuoteAddOnRequest(11, "Cup holder", AddOnPricingMode.PerRental, 5m),
        ]);

        QuoteBreakdown price = Quote.For(
            Request("2026-12-20", "2026-12-24", line, deliveryFee: 25m, taxRate: 0.0650m)).Breakdown!;

        Assert.Equal(245m, price.Subtotal + price.ExtraBatteriesTotal + price.AddOnsTotal + price.DeliveryFee);
        Assert.Equal(15.93m, price.Tax);
        Assert.Equal(260.93m, price.Total);
    }

    [Fact]
    public void A_price_list_with_a_gap_fails_closed_and_names_the_problem()
    {
        // One to two days and then four onwards: nothing prices a three-day rental.
        QuoteLineRequest broken = ScoutLine(tiers:
        [
            new PricingTier { MinDays = 1, MaxDays = 2, Mode = TierMode.FlatPerRental, Amount = 75m },
            new PricingTier { MinDays = 4, MaxDays = null, Mode = TierMode.PerDay, Amount = 27m },
        ]);

        QuoteResult result = Quote.For(Request("2026-12-20", "2026-12-24", broken));

        Assert.Equal(QuoteProblem.PriceListInvalid, result.Problem);
        Assert.Equal(PricingTierSetProblem.Gap, result.TierProblem);
        Assert.Equal(ScoutId, result.ProductId);

        // The thing this test exists for: there is no amount at all, not an amount of zero.
        Assert.Null(result.Breakdown);
    }

    [Theory]
    [InlineData("2026-12-20", "2026-12-19", QuoteProblem.EndBeforeStart)]
    [InlineData("2026-09-20", "2027-09-20", QuoteProblem.TooLong)]
    public void A_length_outside_the_bounds_is_refused_with_the_reason(string start, string end, QuoteProblem expected)
    {
        QuoteResult result = Quote.For(Request(start, end, ScoutLine()));

        Assert.Equal(expected, result.Problem);
        Assert.Null(result.Breakdown);
    }

    [Fact]
    public void Sixty_days_is_priced_and_sixty_one_is_not()
    {
        DateOnly start = new(2026, 12, 1);

        Assert.Equal(QuoteProblem.None,
            Quote.For(Request(start, start.AddDays(59), ScoutLine())).Problem);

        Assert.Equal(QuoteProblem.TooLong,
            Quote.For(Request(start, start.AddDays(60), ScoutLine())).Problem);
    }

    [Fact]
    public void A_negative_amount_is_refused_rather_than_subtracted_from_the_total()
    {
        QuoteResult result = Quote.For(
            Request("2026-12-20", "2026-12-24", ScoutLine(extraBatteries: 1, perDay: -8m)));

        Assert.Equal(QuoteProblem.NegativeAmount, result.Problem);
        Assert.Null(result.Breakdown);
    }

    // ---------------------------------------------------------------------------------------
    // The booking number and the windows
    // ---------------------------------------------------------------------------------------

    [Theory]
    [InlineData(7, "OU-000007")]
    [InlineData(123456, "OU-123456")]
    [InlineData(1234567, "OU-1234567")]
    public void The_booking_number_is_the_key_padded_to_six_digits(int id, string expected)
    {
        Assert.Equal(expected, BookingRules.FormatNumber(id));
    }

    [Theory]
    [InlineData(DeliveryWindow.Morning, 8, 10)]
    [InlineData(DeliveryWindow.LateMorning, 10, 12)]
    [InlineData(DeliveryWindow.Afternoon, 14, 16)]
    [InlineData(DeliveryWindow.Evening, 18, 20)]
    public void Each_window_stands_for_the_hours_the_operation_drives(DeliveryWindow window, int from, int to)
    {
        (TimeOnly start, TimeOnly end) = DeliveryWindows.Hours(window);

        Assert.Equal(new TimeOnly(from, 0), start);
        Assert.Equal(new TimeOnly(to, 0), end);
    }

    [Fact]
    public void A_window_nobody_recognises_throws_rather_than_becoming_the_morning_run()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DeliveryWindows.Hours((DeliveryWindow)99));
    }

    // ---------------------------------------------------------------------------------------
    // The factory
    // ---------------------------------------------------------------------------------------

    [Fact]
    public void The_factory_freezes_the_quote_onto_the_booking_and_its_lines()
    {
        Booking booking = StaffBooking();

        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.Equal(BookingSource.Staff, booking.Source);
        Assert.Equal(5, booking.Days);
        Assert.Equal(160m, booking.Subtotal);
        Assert.Equal(160m, booking.Total);

        BookingLine line = Assert.Single(booking.Lines);

        // The band that priced it is copied onto the line, so a price list edited tomorrow cannot
        // change what this booking says it was charged under.
        Assert.Equal("Drive Scout 4", line.ProductName);
        Assert.Equal(3, line.TierMinDays);
        Assert.Equal(6, line.TierMaxDays);
        Assert.Equal(TierMode.PerDay, line.TierMode);
        Assert.Equal(32m, line.TierAmount);
        Assert.Equal(160m, line.LineTotal);
    }

    // ---------------------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------------------

    private static PricingTier[] ScoutTiers() =>
    [
        new() { MinDays = 1, MaxDays = 2, Mode = TierMode.FlatPerRental, Amount = 75m },
        new() { MinDays = 3, MaxDays = 6, Mode = TierMode.PerDay, Amount = 32m },
        new() { MinDays = 7, MaxDays = null, Mode = TierMode.PerDay, Amount = 27m },
    ];

    private static QuoteLineRequest ScoutLine(
        int quantity = 1,
        int extraBatteries = 0,
        decimal perDay = 0m,
        IReadOnlyList<PricingTier>? tiers = null,
        IReadOnlyList<QuoteAddOnRequest>? addOns = null) =>
        new(ScoutId, "Drive Scout 4", quantity, extraBatteries, perDay, tiers ?? ScoutTiers(), addOns ?? []);

    private static QuoteRequest Request(
        string start, string end, QuoteLineRequest line, decimal deliveryFee = 0m, decimal taxRate = 0m) =>
        Request(
            DateOnly.Parse(start, System.Globalization.CultureInfo.InvariantCulture),
            DateOnly.Parse(end, System.Globalization.CultureInfo.InvariantCulture),
            line, deliveryFee, taxRate);

    private static QuoteRequest Request(
        DateOnly start, DateOnly end, QuoteLineRequest line, decimal deliveryFee = 0m, decimal taxRate = 0m) =>
        new(start, end, deliveryFee, taxRate, [line]);

    private static Booking StaffBooking()
    {
        QuoteBreakdown price = Quote.For(Request("2026-12-20", "2026-12-24", ScoutLine())).Breakdown!;

        return Booking.CreateByStaff(
            new StaffBookingDetails(
                "en-US", "Ada", "Lovelace", "ada@example.com", "+1 407 555 0100",
                DeliveryZoneId: 1, DeliveryLocationId: 2, Address: null, DeliveryNotes: null,
                new DateOnly(2026, 12, 20), new DateOnly(2026, 12, 24),
                DeliveryWindow.Morning, DeliveryWindow.Afternoon, StaffNotes: null),
            price,
            "staff@orlandoup.com",
            new DateTime(2026, 12, 1, 12, 0, 0, DateTimeKind.Utc),
            isOverbooked: false);
    }
}
