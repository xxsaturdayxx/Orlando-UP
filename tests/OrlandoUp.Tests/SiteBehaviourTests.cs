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

    /// <summary>Every public address of the site, in both cultures.</summary>
    public static TheoryData<string> PublicPaths() =>
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
    ];

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
        string body = await _factory.CreateClient().GetStringAsync("/rentals");

        Assert.Contains("from US$", body, StringComparison.Ordinal);
        Assert.Contains("Coming soon", body, StringComparison.Ordinal);
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
