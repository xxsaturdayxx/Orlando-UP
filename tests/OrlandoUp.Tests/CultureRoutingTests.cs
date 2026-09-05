using System.Net;
using System.Text.RegularExpressions;

namespace OrlandoUp.Tests;

/// <summary>
/// The observable behaviour of the culture prefix (D21, D3/01). These assertions are about
/// addresses and markup, not about the mechanism, so the mechanism stays free to change.
/// </summary>
public class CultureRoutingTests : IAsyncLifetime
{
    private readonly SiteFactory _factory = new();

    public async Task InitializeAsync() => await _factory.SeedAsync();

    public Task DisposeAsync()
    {
        _factory.Dispose();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task The_root_is_served_in_English()
    {
        HttpResponseMessage response = await _factory.CreateClient().GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<html lang=\"en\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task The_prefix_is_served_in_Portuguese()
    {
        HttpResponseMessage response = await _factory.CreateClient().GetAsync("/pt");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<html lang=\"pt-BR\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task The_catalog_under_the_prefix_is_served_in_Portuguese()
    {
        HttpResponseMessage response = await _factory.CreateClient().GetAsync("/pt/rentals");
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<html lang=\"pt-BR\"", body);

        // A word that exists only in the Portuguese resource file, so the assertion cannot pass on
        // an English page that merely carried the right attribute.
        Assert.Contains("Equipamentos", body);
    }

    [Fact]
    public async Task A_culture_the_site_does_not_serve_is_not_a_page()
    {
        HttpResponseMessage response = await _factory.CreateClient().GetAsync("/es");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Every_internal_link_of_a_Portuguese_page_stays_under_the_prefix()
    {
        string body = await _factory.CreateClient().GetStringAsync("/pt/rentals");

        List<string> escaping = Regex
            .Matches(body, "<a\\b[^>]*?href=\"(?<href>[^\"]+)\"", RegexOptions.IgnoreCase)
            .Select(match => match.Groups["href"].Value)
            .Where(href => href.StartsWith('/'))
            .Where(href => !IsAsset(href))
            .Where(href => !href.Equals("/pt", StringComparison.Ordinal))
            .Where(href => !href.StartsWith("/pt/", StringComparison.Ordinal))
            .ToList();

        // The switcher is the one link that is supposed to leave, and it leaves to the English
        // root of the same page. Everything else staying put is what this asserts.
        Assert.Equal(["/rentals"], escaping);
    }

    [Fact]
    public async Task The_English_page_links_to_its_Portuguese_twin_and_back()
    {
        string english = await _factory.CreateClient().GetStringAsync("/rentals");

        Assert.Contains("href=\"/pt/rentals\"", english);
    }

    [Fact]
    public async Task The_home_page_switches_to_the_short_Portuguese_address()
    {
        // The home page is the one page the framework gives two templates, "" and "Index", so the
        // convention gives it both /pt and /pt/Index. Both must answer — but the switcher has to
        // offer the short one, the same way English offers "/" and not "/Index". It offered
        // /pt/Index until the convention said which of the two it prefers.
        string home = await _factory.CreateClient().GetStringAsync("/");

        Assert.Contains("href=\"/pt\"", home);
        Assert.DoesNotContain("href=\"/pt/Index\"", home);
    }

    [Fact]
    public async Task Both_addresses_of_the_Portuguese_home_page_keep_answering()
    {
        // The sibling of the assertion above: preferring the short address must not have taken the
        // long one off the routing table, or an address already in somebody's history would 404.
        HttpResponseMessage shortAddress = await _factory.CreateClient().GetAsync("/pt");
        HttpResponseMessage longAddress = await _factory.CreateClient().GetAsync("/pt/Index");

        Assert.Equal(HttpStatusCode.OK, shortAddress.StatusCode);
        Assert.Equal(HttpStatusCode.OK, longAddress.StatusCode);
        Assert.Contains("<html lang=\"pt-BR\"", await longAddress.Content.ReadAsStringAsync());
    }

    private static bool IsAsset(string href) =>
        href.StartsWith("/css/", StringComparison.Ordinal)
        || href.StartsWith("/img/", StringComparison.Ordinal)
        || href.StartsWith("/fonts/", StringComparison.Ordinal)
        || href.StartsWith("/favicon", StringComparison.Ordinal);
}
