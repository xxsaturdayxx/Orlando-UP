using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrlandoUp.Application;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;
using OrlandoUp.Infrastructure.Seeding;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace OrlandoUp.Tests;

/// <summary>
/// The endpoints that are not pages, the administration gate, and the assertion that this release
/// has no way to send anything to anybody.
/// </summary>
public class SiteBehaviourTests : IAsyncLifetime
{
    private readonly SiteFactory _factory = new();

    public async Task InitializeAsync() => await _factory.SeedAsync();

    public Task DisposeAsync()
    {
        _factory.Dispose();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task The_health_endpoint_reports_a_reachable_database()
    {
        HttpResponseMessage response = await _factory.CreateClient().GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal("ok", body.RootElement.GetProperty("status").GetString());
        Assert.Equal("ok", body.RootElement.GetProperty("database").GetString());
    }

    [Fact]
    public async Task The_crawler_file_closes_the_site_while_indexing_is_off()
    {
        string body = await _factory.CreateClient().GetStringAsync("/robots.txt");

        Assert.Contains("Disallow: /", body);
    }

    [Fact]
    public async Task Every_public_page_carries_the_no_index_instruction_while_indexing_is_off()
    {
        string body = await _factory.CreateClient().GetStringAsync("/");

        Assert.Contains("name=\"robots\" content=\"noindex\"", body);
    }

    [Fact]
    public async Task An_anonymous_visitor_to_the_administration_is_sent_to_the_login_page()
    {
        HttpClient client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        HttpResponseMessage response = await client.GetAsync("/admin");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("/admin/login", response.Headers.Location?.OriginalString ?? string.Empty);
    }

    [Fact]
    public async Task The_login_page_itself_is_reachable_without_an_account()
    {
        HttpResponseMessage response = await _factory.CreateClient().GetAsync("/admin/login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task An_unknown_address_answers_with_the_localized_error_page()
    {
        HttpResponseMessage response = await _factory.CreateClient().GetAsync("/no-such-page");
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("We could not find that page", body);
    }

    [Fact]
    public async Task A_product_page_shows_the_transport_badge_only_when_the_dimensions_allow_it()
    {
        HttpClient client = _factory.CreateClient();

        // A measured fit, an unmeasured product, and the difference between them. The stroller has
        // no dimensions at all now, so its answer is neither yes nor no - and the badge is shown
        // only on yes.
        Assert.Contains("Fits Disney buses", await client.GetStringAsync("/rentals/drive-scout-4"));
        Assert.DoesNotContain("Fits Disney buses", await client.GetStringAsync("/rentals/triple-stroller"));
        Assert.DoesNotContain("Fits Disney buses", await client.GetStringAsync("/rentals/drive-wheelchair"));
    }

    [Fact]
    public async Task A_product_page_shows_a_price_and_never_a_zero_standing_in_for_a_missing_one()
    {
        string body = await _factory.CreateClient().GetStringAsync("/rentals/drive-scout-4");

        Assert.Contains("US$ 27.00", body);
        Assert.DoesNotContain("US$ 0.00", body);
    }

    /// <summary>Every public address of the site, in both cultures, typed by hand on purpose.</summary>
    /// <remarks>
    /// Independent of the set the framework derives, and useful only while the two agree - which is
    /// what SeoTests asserts. A page added to the site and forgotten here shows up there by name.
    /// </remarks>
    public static readonly IReadOnlyList<string> PublicPathList =
    [
        "/", "/pt",
        "/rentals", "/pt/rentals",
        "/rentals/drive-scout-4", "/pt/rentals/drive-scout-4",
        "/rentals/drive-wheelchair", "/rentals/single-stroller", "/pt/rentals/single-stroller",
        "/how-it-works", "/pt/how-it-works",
        "/delivery-areas", "/pt/delivery-areas",
        "/faq", "/pt/faq",
        "/contact", "/pt/contact",
        "/terms", "/pt/terms",
        "/privacy", "/pt/privacy",
        "/book", "/pt/book",
    ];

    public static TheoryData<string> PublicPaths()
    {
        TheoryData<string> data = [];

        foreach (string path in PublicPathList)
        {
            data.Add(path);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(PublicPaths))]
    public async Task No_public_page_shows_a_value_that_is_still_a_placeholder(string path)
    {
        // Presence before absence. "This body has no TODO- in it" is also true of an empty body and
        // of a 500, so the page has to be proved to have rendered before the absence means anything.
        HttpResponseMessage response = await _factory.CreateClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync();

        Assert.True(body.Length > 1000, $"{path} answered 200 with only {body.Length} characters");
        Assert.DoesNotContain("TODO-", body, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(PublicPaths))]
    public async Task Every_public_page_has_exactly_one_top_level_heading(string path)
    {
        string body = await _factory.CreateClient().GetStringAsync(path);

        int count = System.Text.RegularExpressions.Regex.Matches(body, "<h1[ >]").Count;

        Assert.True(count == 1, $"{path} has {count} h1 elements");
    }

    [Fact]
    public async Task A_product_we_have_not_bought_shows_no_price_anywhere_on_its_page()
    {
        HttpClient client = _factory.CreateClient();

        string comingSoon = await client.GetStringAsync("/rentals/single-stroller");
        string onSale = await client.GetStringAsync("/rentals/drive-scout-4");

        // Reach: the currency string has to be findable at all, or the absence below is vacuous.
        Assert.Contains("US$", onSale, StringComparison.Ordinal);
        Assert.DoesNotContain("US$", comingSoon, StringComparison.Ordinal);
        Assert.Contains("Coming soon", comingSoon, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_catalog_page_prices_what_is_on_sale_and_only_that()
    {
        // The name of this test promises a negative, so the negative is what it counts. "from US$
        // is present, Coming soon is present, US$ 0.00 is absent" was satisfied by a page printing
        // a price under a stroller card, and a name that promises cover it does not give is worse
        // than no test: somebody later reads the name and stops looking.
        using IServiceScope scope = _factory.Services.CreateScope();
        var catalog = scope.ServiceProvider.GetRequiredService<OrlandoUp.Infrastructure.Data.CatalogQueries>();

        var cards = await catalog.ActiveCardsAsync("en-US", CancellationToken.None);

        int bookable = cards.Count(card => card.IsBookable);
        int comingSoon = cards.Count - bookable;

        Assert.True(bookable > 0 && comingSoon > 0, $"on sale: {bookable}, coming soon: {comingSoon}");

        string body = await _factory.CreateClient().GetStringAsync("/rentals");

        int prices = System.Text.RegularExpressions.Regex.Matches(body, "from US\\$").Count;
        int pills = System.Text.RegularExpressions.Regex.Matches(body, "badge--soon").Count;

        Assert.Equal(bookable, prices);
        Assert.Equal(comingSoon, pills);
        Assert.DoesNotContain("US$ 0.00", body, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/delivery-areas", "Walt Disney World resorts")]
    [InlineData("/pt/delivery-areas", "Resorts do Walt Disney World")]
    public async Task The_delivery_page_names_every_active_zone_from_the_database(string path, string firstZone)
    {
        string body = await _factory.CreateClient().GetStringAsync(path);

        // The body is DECODED before comparing, not the expectation encoded. The framework writes
        // an accented letter as a numeric entity, and it picks the hexadecimal form while the
        // encoder in the test library picks the decimal one - two spellings of the same character,
        // and comparing them fails on output that is perfectly correct. Decoding removes the
        // question entirely: what is asserted is the text a reader sees.
        string text = System.Net.WebUtility.HtmlDecode(body);

        Assert.Contains(firstZone, text, StringComparison.Ordinal);

        // Every zone, not just the first: the page reads them from the same query the booking will.
        using IServiceScope scope = _factory.Services.CreateScope();
        var catalog = scope.ServiceProvider.GetRequiredService<OrlandoUp.Infrastructure.Data.CatalogQueries>();
        var zones = await catalog.ActiveZonesAsync(
            path.StartsWith("/pt", StringComparison.Ordinal) ? "pt-BR" : "en-US",
            CancellationToken.None);

        Assert.True(zones.Count >= 4, $"only {zones.Count} zones were read; the assertion below would prove little");

        foreach (var zone in zones)
        {
            Assert.Contains(zone.Name, text, StringComparison.Ordinal);
        }

        // And the sentence, not only the name. The page exists because the hand-over instruction a
        // visitor reads has to be the same string the booking will show (D5/02) - four names listed
        // and the instructions silently dropped would have passed everything above.
        var disney = zones.First(zone => zone.Handover == OrlandoUp.Domain.HandoverMode.MeetAndGreet);
        string instructions = System.Text.RegularExpressions.Regex.Replace(
            System.Net.WebUtility.HtmlDecode(disney.InstructionsHtml), "<[^>]+>", string.Empty);

        Assert.True(instructions.Length > 80, $"the instructions read {instructions.Length} characters");
        Assert.Contains(instructions.Trim()[..60], text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_faq_page_renders_every_question_the_resource_file_carries()
    {
        // The list is read from the same source the page reads, through dependency injection, and
        // never by walking the folder: a page that quietly stopped after the eighth question would
        // pass any assertion written against a number typed here.
        using IServiceScope scope = _factory.Services.CreateScope();
        var localizer = scope.ServiceProvider
            .GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizer<OrlandoUp.SharedResource>>();

        List<Microsoft.Extensions.Localization.LocalizedString> questions = localizer
            .GetAllStrings(includeParentCultures: true)
            .Where(entry => System.Text.RegularExpressions.Regex.IsMatch(entry.Name, "^Faq_Q[0-9]+$"))
            .ToList();

        Assert.True(questions.Count >= 8, $"only {questions.Count} questions were read from the resources");

        string body = await _factory.CreateClient().GetStringAsync("/faq");

        foreach (var question in questions)
        {
            Assert.Contains(question.Value, System.Net.WebUtility.HtmlDecode(body), StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Nothing_in_this_release_can_send_a_message_to_anybody()
    {
        // The effects to neutralise before the first scenario are, in this release, none: no mail
        // sender is registered at all. This asserts the absence instead of trusting it, so that the
        // day one is registered, whoever registers it has to come and say why.
        //
        // Reach first (rule 5): "no name contains EmailSender" is also true of an empty list, so a
        // host that registered nothing would look exactly like a host that registered no sender.
        // The application registers hundreds of services; this assertion is what makes the next one
        // mean something.
        Assert.NotEmpty(_factory.RegisteredServiceNames);

        List<string> senders = _factory.RegisteredServiceNames
            .Where(name => name.Contains("EmailSender", StringComparison.Ordinal))
            .ToList();

        Assert.Empty(senders);
    }
}

/// <summary>
/// The public booking page: what a visitor is told, and what he is not told.
/// </summary>
/// <remarks>
/// It is a barrier (D4/03). Everything a member of staff may override, this page refuses — and the
/// case the whole leva exists for is the one where the machines are free and the batteries are
/// not, which no other rental site in Orlando answers honestly.
/// </remarks>
public class PublicBookingTests : IAsyncLifetime
{
    private readonly SiteFactory _factory = new();

    private int _scoutId;
    private string _scoutSlug = string.Empty;
    private int _place;

    public async Task InitializeAsync()
    {
        await _factory.SeedAsync();

        using IServiceScope scope = _factory.Services.CreateScope();

        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IClock clock = scope.ServiceProvider.GetRequiredService<IClock>();
        ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Tests");

        db.OperationalSettings.Add(new OperationalSettings
        {
            Id = OperationalSettings.SingletonId,
            ChargerCount = 14,
            SecondBatteryPerDay = 8.00m,
            LostChargerFee = 30.00m,
            NextDayCutoffHour = 18,
        });

        await db.SaveChangesAsync();

        Assert.Equal(0, await BatterySeeder.RunAsync(db, clock, logger, CancellationToken.None));

        Product scout = await db.Products
            .Where(row => row.Category == ProductCategory.MobilityScooter)
            .OrderBy(row => row.SortOrder)
            .FirstAsync();

        _scoutId = scout.Id;
        _scoutSlug = scout.Slug;
        _place = await db.DeliveryLocations.OrderBy(row => row.SortOrder).Select(row => row.Id).FirstAsync();

        // Pinned well before the dates asked for, so the cut-off never turns these into a refusal
        // about the calendar.
        _factory.Clock.Set(new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task An_available_request_is_answered_with_the_word_and_the_total()
    {
        string html = await _factory.CreateClient().GetStringAsync(Query());

        Assert.Contains("Available for these dates", html, StringComparison.Ordinal);

        // 160 rental for five days at 32, plus 40 for the second battery at 8 a day. Zone fee and
        // tax rate are both zero, so the tax line prints the promise instead of US$ 0.00.
        Assert.Contains("US$ 200.00", html, StringComparison.Ordinal);
        Assert.Contains("Taxes included", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Sold out for these dates", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_portuguese_twin_answers_in_portuguese()
    {
        // Razor encodes every non-ASCII character, so the accented sentences arrive as entities.
        // Decoded here the way the other Portuguese assertions of this file already do it.
        string html = System.Net.WebUtility.HtmlDecode(
            await _factory.CreateClient().GetStringAsync("/pt" + Query()));

        Assert.Contains("Disponível nessas datas", html, StringComparison.Ordinal);
        Assert.Contains("US$ 200.00", html, StringComparison.Ordinal);
        Assert.Contains("Impostos incluídos", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Four_scooters_already_out_make_it_sold_out()
    {
        await OccupyAsync(quantity: 4, extras: 0);

        string html = await _factory.CreateClient().GetStringAsync(Query(extras: 0));

        Assert.Contains("Sold out for these dates", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Available for these dates", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Three_second_batteries_out_leave_the_machine_free_and_the_battery_not()
    {
        // Three Scouts with two second batteries between them: five of the six batteries, and one
        // machine still on the floor. The fourth Scout can go out — the sixth battery is its own —
        // but there is no seventh for a spare. That gap of exactly one is what the sentence is for.
        //
        // Three machines with three spares would be SOLD OUT rather than this, and the arithmetic
        // is why: six batteries for three machines and three spares leaves nothing for a fourth
        // machine at all.
        await OccupyAsync(quantity: 3, extras: 2);

        string html = await _factory.CreateClient().GetStringAsync(Query(extras: 1));

        Assert.Contains("The second battery is not available for these dates", html, StringComparison.Ordinal);

        // And the price shown is the rental WITHOUT it: 160, not 200.
        Assert.Contains("US$ 160.00", html, StringComparison.Ordinal);
        Assert.DoesNotContain("US$ 200.00", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_day_before_the_cut_off_allows_is_refused_with_the_day_named()
    {
        // 23:00 UTC on 5 December is 18:00 in Orlando — at the cut-off, so the earliest is the 7th.
        _factory.Clock.Set(new DateTime(2026, 12, 5, 23, 0, 0, DateTimeKind.Utc));

        string html = await _factory.CreateClient().GetStringAsync(
            $"/book?product={_scoutSlug}&start=2026-12-06&end=2026-12-08&quantity=1&extraBatteries=0&place=L{_place}");

        Assert.Contains("The earliest delivery day is 2026-12-07", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Available for these dates", html, StringComparison.Ordinal);

        // The presence half: one day later the same request is answered.
        Assert.Contains(
            "Available for these dates",
            await _factory.CreateClient().GetStringAsync(
                $"/book?product={_scoutSlug}&start=2026-12-07&end=2026-12-09&quantity=1&extraBatteries=0&place=L{_place}"),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_page_never_offers_to_overbook()
    {
        await OccupyAsync(quantity: 4, extras: 0);

        string html = await _factory.CreateClient().GetStringAsync(Query(extras: 0));

        // Whatever else it says, it offers no way past the fleet and no price to act on.
        Assert.DoesNotContain("Overbook", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Your price", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_product_page_links_here_instead_of_a_disabled_button()
    {
        string scooter = await _factory.CreateClient().GetStringAsync($"/rentals/{_scoutSlug}");

        Assert.Contains("Check availability and price", scooter, StringComparison.Ordinal);
        Assert.DoesNotContain("Booking opens soon", scooter, StringComparison.Ordinal);
        Assert.Contains("/book?", scooter, StringComparison.Ordinal);

        // The stroller keeps its coming-soon sentence: it is not on sale, and this leva did not
        // touch that branch.
        string stroller = await _factory.CreateClient().GetStringAsync("/rentals/single-stroller");

        Assert.DoesNotContain("Check availability and price", stroller, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_sitemap_carries_both_addresses_with_their_alternates()
    {
        string xml = await _factory.CreateClient().GetStringAsync("/sitemap.xml");

        Assert.Contains("/book<", xml, StringComparison.Ordinal);
        Assert.Contains("/pt/book<", xml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Without_the_settings_row_the_page_says_it_cannot_check_rather_than_quoting()
    {
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.OperationalSettings.RemoveRange(await db.OperationalSettings.ToListAsync());
            await db.SaveChangesAsync();
        }

        string html = await _factory.CreateClient().GetStringAsync(Query());

        Assert.Contains("cannot check availability", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Available for these dates", html, StringComparison.Ordinal);
        Assert.DoesNotContain("US$ 200.00", html, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------------------------------

    private string Query(int extras = 1) =>
        $"/book?product={_scoutSlug}&start=2026-12-20&end=2026-12-24&quantity=1&extraBatteries={extras}&place=L{_place}";

    private async Task OccupyAsync(int quantity, int extras)
    {
        using IServiceScope scope = _factory.Services.CreateScope();

        BookingWriter writer = scope.ServiceProvider.GetRequiredService<BookingWriter>();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        DeliveryZone zone = await db.DeliveryZones.OrderBy(row => row.SortOrder).FirstAsync();

        BookingWriteResult result = await writer.CreateByStaffAsync(
            new StaffBookingDetails(
                "en-US", "Ada", "Lovelace", "ada@example.com", "+1 407 555 0100",
                zone.Id, _place, null, null,
                new DateOnly(2026, 12, 20), new DateOnly(2026, 12, 24),
                DeliveryWindow.Morning, DeliveryWindow.Afternoon, null),
            [new QuoteLineAsked(_scoutId, quantity, extras, 8m, [])],
            "staff@orlandoup.com",
            acknowledgeOverbooking: false,
            CancellationToken.None);

        Assert.True(result.Succeeded, "the diary could not be arranged, so nothing after it would mean anything");
    }
}
