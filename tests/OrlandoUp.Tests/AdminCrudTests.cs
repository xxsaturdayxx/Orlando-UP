using System.Net;

namespace OrlandoUp.Tests;

/// <summary>
/// The administration's write path. At stop P2 this file holds only the three proofs that the
/// harness works; the assertions about what the screens do arrive with the screens.
/// </summary>
/// <remarks>
/// The three proofs are one argument, not three tests that happen to be near each other. The first
/// says an authenticated request gets past the gate. The second says a post carrying the token is
/// accepted. The third says the same post WITHOUT the token is refused — and it is the third that
/// gives the second its meaning, because a post accepted by a host with antiforgery switched off
/// would look exactly like a post accepted because its token was right.
///
/// The form used is the sign-out of <c>_AdminLayout.cshtml</c>, which is the only authenticated
/// POST that exists before the CRUD screens do.
/// </remarks>
public class AdminCrudTests : IAsyncLifetime
{
    private readonly SiteFactory _factory = new();

    public async Task InitializeAsync() => await _factory.SeedAsync();

    public Task DisposeAsync()
    {
        _factory.Dispose();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task An_authenticated_request_reaches_a_page_behind_the_administration_gate()
    {
        HttpResponseMessage response = await _factory.CreateStaffClient().GetAsync("/admin/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // The presence half: a 200 from a redirect chain that landed on the login page would also
        // be a 200. This asserts the page that answered is the one asked for.
        Assert.Equal("/admin/products", response.RequestMessage?.RequestUri?.AbsolutePath);
        Assert.Contains(FormPoster.TokenFieldName, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task The_same_page_without_the_seam_still_sends_the_visitor_to_the_login_page()
    {
        HttpClient anonymous = _factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });

        HttpResponseMessage response = await anonymous.GetAsync("/admin/products");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("/admin/login", response.Headers.Location?.OriginalString ?? string.Empty);
    }

    [Fact]
    public async Task A_post_carrying_the_antiforgery_token_is_accepted()
    {
        HttpClient staff = _factory.CreateStaffClient(allowAutoRedirect: false);

        HttpResponseMessage response = await FormPoster.PostWithTokenAsync(
            staff, tokenPagePath: "/admin/products", formPath: "/admin/logout");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("/admin/login", response.Headers.Location?.OriginalString ?? string.Empty);
    }

    [Fact]
    public async Task The_same_post_without_the_antiforgery_token_is_refused()
    {
        HttpClient staff = _factory.CreateStaffClient(allowAutoRedirect: false);

        // The token page is read first anyway, so the antiforgery COOKIE is present on the client
        // and the only thing missing from the post is the form field. Without this the test would
        // pass for the wrong reason: no cookie is also no token.
        await FormPoster.ReadTokenAsync(staff, "/admin/products");

        HttpResponseMessage response = await FormPoster.PostWithoutTokenAsync(staff, "/admin/logout");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
