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

        Assert.Contains("Fits Disney buses", await client.GetStringAsync("/rentals/standard-scooter"));
        Assert.DoesNotContain("Fits Disney buses", await client.GetStringAsync("/rentals/triple-stroller"));
    }

    [Fact]
    public async Task A_product_page_shows_a_price_and_never_a_zero_standing_in_for_a_missing_one()
    {
        string body = await _factory.CreateClient().GetStringAsync("/rentals/standard-scooter");

        Assert.Contains("US$ 27.00", body);
        Assert.DoesNotContain("US$ 0.00", body);
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
