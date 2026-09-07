using System.Net;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrlandoUp.Application;
using OrlandoUp.Application.Catalog;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;
using OrlandoUp.Infrastructure.Localization;

namespace OrlandoUp.Tests;

/// <summary>
/// The two things on this site written for a machine rather than for a reader: the sitemap and the
/// structured data. Both are the kind of artefact nobody opens again, so what is asserted here is
/// what keeps them honest.
/// </summary>
public class SeoTests : IAsyncLifetime
{
    private const string Sitemap = "/sitemap.xml";
    private static readonly XNamespace Urlset = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private static readonly XNamespace Xhtml = "http://www.w3.org/1999/xhtml";

    private readonly SiteFactory _factory = new();

    public async Task InitializeAsync() => await _factory.SeedAsync();

    public Task DisposeAsync()
    {
        _factory.Dispose();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task The_sitemap_is_well_formed_and_declares_the_encoding_it_is_sent_in()
    {
        HttpResponseMessage response = await _factory.CreateClient().GetAsync(Sitemap);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync();

        // A document that declares one encoding while its bytes are another is a document a parser
        // may refuse, and it is exactly what writing XML into a StringBuilder produces by default.
        Assert.Contains("encoding=\"utf-8\"", body, StringComparison.OrdinalIgnoreCase);

        XDocument document = XDocument.Parse(body);

        Assert.Equal(Urlset + "urlset", document.Root!.Name);
    }

    [Fact]
    public async Task The_sitemap_carries_every_public_page_in_both_cultures()
    {
        // The set of pages comes from the framework, through dependency injection - the same source
        // the endpoint reads. A list typed into this test would agree with a list typed into the
        // endpoint and both could be wrong together.
        using IServiceScope scope = _factory.Services.CreateScope();
        PublicPages pages = scope.ServiceProvider.GetRequiredService<PublicPages>();

        IReadOnlyList<string> names = pages.Names();

        // Reach: an empty page set would make every assertion below pass by having nothing to check.
        Assert.True(names.Count >= 8, $"only {names.Count} public pages were discovered");

        // Compared as paths. The scheme of a loc follows the scheme of the request, and the test
        // host serves over http while a browser will not - asserting an absolute address here would
        // be asserting something about the host rather than about the sitemap.
        HashSet<string> paths = (await LocationsAsync())
            .Select(url => new Uri(url).AbsolutePath)
            .ToHashSet(StringComparer.Ordinal);

        foreach (string name in names)
        {
            foreach ((string segment, _) in PublicPages.Cultures)
            {
                string? path = pages.PathFor(name, segment);

                Assert.NotNull(path);
                Assert.Contains(path, paths);
            }
        }
    }

    [Fact]
    public void The_two_lists_of_public_pages_agree_with_each_other()
    {
        // SiteBehaviourTests carries a hand-typed list of addresses, and it is useful precisely
        // because it is independent. Independent is only worth something while the two agree, so
        // the disagreement is what is asserted here: a page added to the site and forgotten in the
        // typed list, or the other way round, shows up as a name rather than as a mystery later.
        using IServiceScope scope = _factory.Services.CreateScope();
        PublicPages pages = scope.ServiceProvider.GetRequiredService<PublicPages>();

        HashSet<string> derived = [];

        foreach (string name in pages.Names())
        {
            foreach ((string segment, _) in PublicPages.Cultures)
            {
                string? path = pages.PathFor(name, segment);

                if (path is not null)
                {
                    derived.Add(path);
                }
            }
        }

        // The typed list also covers product pages, which the derived one cannot: a product address
        // needs a slug the framework does not know. Those are compared in the sitemap test instead.
        HashSet<string> typed = SiteBehaviourTests.PublicPathList
            .Where(path => !path.Contains("/rentals/", StringComparison.Ordinal))
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(derived.SetEquals(typed),
            $"only derived: {string.Join(", ", derived.Except(typed).Order())}; "
            + $"only typed: {string.Join(", ", typed.Except(derived).Order())}");
    }

    [Fact]
    public async Task The_sitemap_lists_the_products_that_are_on_the_catalog_and_no_other()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        CatalogQueries catalog = scope.ServiceProvider.GetRequiredService<CatalogQueries>();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        IReadOnlyList<ProductCard> cards = await catalog.ActiveCardsAsync(SiteCultures.English, CancellationToken.None);

        Assert.True(cards.Count >= 3, $"only {cards.Count} products were read");

        HashSet<string> before = await LocationsAsync();

        foreach (ProductCard card in cards)
        {
            Assert.Contains(before, url => url.EndsWith("/rentals/" + card.Slug, StringComparison.Ordinal));
        }

        // And the other side: a product an administrator hides leaves the sitemap with it. Without
        // this, the whole assertion is "everything visible is listed", which a sitemap that lists
        // absolutely everything would also satisfy.
        Product hidden = await db.Products.FirstAsync(product => product.Slug == "infant-stroller");
        hidden.IsActive = false;
        await db.SaveChangesAsync();

        HashSet<string> after = await LocationsAsync();

        Assert.DoesNotContain(after, url => url.EndsWith("/rentals/infant-stroller", StringComparison.Ordinal));
        Assert.Contains(after, url => url.EndsWith("/rentals/drive-scout-4", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Every_address_in_the_sitemap_declares_its_alternates_and_a_default()
    {
        // The alternates are the whole reason a bilingual site has a sitemap at all, and they are
        // also the part nobody ever opens again: a regression that dropped every xhtml:link would
        // have left the locs intact and passed every other assertion in this file.
        XDocument document = XDocument.Parse(await _factory.CreateClient().GetStringAsync(Sitemap));

        List<XElement> addresses = document.Descendants(Urlset + "url").ToList();

        Assert.True(addresses.Count >= 20, $"only {addresses.Count} addresses were found");

        int alternates = 0;

        foreach (XElement address in addresses)
        {
            string loc = address.Element(Urlset + "loc")!.Value;

            Dictionary<string, string> byLanguage = address
                .Elements(Xhtml + "link")
                .Where(link => (string?)link.Attribute("rel") == "alternate")
                .ToDictionary(
                    link => (string)link.Attribute("hreflang")!,
                    link => (string)link.Attribute("href")!,
                    StringComparer.Ordinal);

            alternates += byLanguage.Count;

            foreach ((_, string culture) in PublicPages.Cultures)
            {
                Assert.True(byLanguage.ContainsKey(culture), $"{loc} declares no {culture} alternate");
            }

            Assert.True(byLanguage.ContainsKey("x-default"), $"{loc} declares no x-default");

            // The default is the English address of THIS page, not of some other one and not the
            // page itself: on a /pt address, an x-default pointing at the /pt address would tell a
            // search engine that Portuguese is what to serve someone with no language preference.
            Assert.Equal(byLanguage[PublicPages.Cultures[0].Culture], byLanguage["x-default"]);
        }

        // Reach: every assertion above is inside a loop, and a document whose urls carried no link
        // at all would have made each of them vacuous had they been written as absences.
        Assert.Equal(addresses.Count * (PublicPages.Cultures.Count + 1), alternates);
    }

    [Fact]
    public async Task The_sitemap_says_nothing_about_the_administration_or_the_error_pages()
    {
        HashSet<string> locations = await LocationsAsync();

        Assert.NotEmpty(locations);
        Assert.DoesNotContain(locations, url => url.Contains("/admin", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(locations, url => url.Contains("/error", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(locations, url => url.Contains("/healthz", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/pt")]
    [InlineData("/rentals/drive-scout-4")]
    public async Task The_structured_data_is_one_block_of_valid_json(string path)
    {
        string json = await JsonBlockAsync(path);

        using JsonDocument document = JsonDocument.Parse(json);

        Assert.Equal("https://schema.org", document.RootElement.GetProperty("@context").GetString());
        Assert.True(document.RootElement.GetProperty("@graph").GetArrayLength() >= 1);
    }

    [Fact]
    public async Task The_structured_data_omits_every_company_field_that_is_still_a_placeholder()
    {
        string json = await JsonBlockAsync("/");

        // Reach first: a document with nothing in it also contains no marker.
        Assert.Contains("Ronatrip", json, StringComparison.Ordinal);
        Assert.DoesNotContain("TODO-", json, StringComparison.Ordinal);

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement business = document.RootElement.GetProperty("@graph")[0];

        Assert.False(business.TryGetProperty("telephone", out _));
        Assert.False(business.TryGetProperty("email", out _));
    }

    [Fact]
    public async Task The_structured_data_promises_no_offer_the_site_cannot_keep()
    {
        // There is no checkout behind this page yet. An offer carries a price and an availability,
        // and both are things a search engine will show and a visitor will try to act on.
        string json = await JsonBlockAsync("/rentals/drive-scout-4");

        Assert.Contains("Drive Scout 4", json, StringComparison.Ordinal);
        Assert.DoesNotContain("offers", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("aggregateRating", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"review\"", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Catalog_text_cannot_end_the_structured_data_block_early()
    {
        // The JSON sits inside a script element, whose content is raw text: the browser decodes no
        // character reference there and only looks for the closing tag. A product name is a column
        // an administrator edits, so this is the path from an edit box to running code, and the
        // only thing that closes it is the encoder.
        ProductDetail product = new(
            "attack",
            ProductCategory.MobilityScooter,
            null,
            "</script><script>alert(1)</script>",
            "tagline <b>with</b> markup & an ampersand",
            string.Empty,
            [],
            null, null, null, null, null, null,
            true,
            null,
            [],
            []);

        string json = StructuredData.ForPage(new CompanyOptions { TradeName = "Orlando Up" }, "https://x", "en-US", product);

        Assert.DoesNotContain("</script>", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<", json, StringComparison.Ordinal);
        Assert.DoesNotContain(">", json, StringComparison.Ordinal);

        // And the accents still travel as themselves, which is why the encoder is built by hand.
        string accented = StructuredData.ForPage(
            new CompanyOptions { TradeName = "Órlando Up — ação" },
            "https://x",
            "pt-BR",
            null);

        Assert.Contains("ação", accented, StringComparison.Ordinal);
    }

    private async Task<HashSet<string>> LocationsAsync()
    {
        string body = await _factory.CreateClient().GetStringAsync(Sitemap);

        return XDocument.Parse(body)
            .Descendants(Urlset + "loc")
            .Select(element => element.Value)
            .ToHashSet(StringComparer.Ordinal);
    }

    private async Task<string> JsonBlockAsync(string path)
    {
        string body = await _factory.CreateClient().GetStringAsync(path);

        const string Opening = "<script type=\"application/ld+json\">";
        int start = body.IndexOf(Opening, StringComparison.Ordinal);

        Assert.True(start >= 0, $"{path} carries no structured data block");
        Assert.Equal(-1, body.IndexOf(Opening, start + 1, StringComparison.Ordinal));

        int from = start + Opening.Length;
        int to = body.IndexOf("</script>", from, StringComparison.Ordinal);

        return body[from..to];
    }
}
