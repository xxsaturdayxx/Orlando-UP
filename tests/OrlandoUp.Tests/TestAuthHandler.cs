using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrlandoUp.Application;

namespace OrlandoUp.Tests;

/// <summary>
/// Signs a request in as a member of staff, so a test can reach a page behind the administration
/// gate. It lives in the test project and nowhere else (D1/04).
/// </summary>
/// <remarks>
/// Two properties matter more than the code, and both are asserted in <c>AdminCrudTests</c>.
///
/// First, it authenticates <b>only</b> a request carrying <see cref="HeaderName"/>. Without the
/// header it answers <see cref="AuthenticateResult.NoResult"/>, which leaves the caller anonymous —
/// that is what keeps the default client anonymous and keeps
/// <c>SiteBehaviourTests.An_anonymous_visitor_to_the_administration_is_sent_to_the_login_page</c>
/// proving the gate rather than proving the harness.
///
/// Second, only the default <b>authenticate</b> scheme is replaced. The <b>challenge</b> scheme
/// stays the Identity cookie, so a request that fails to authenticate is still redirected to
/// <c>/admin/login</c> and not answered with a bare 401. The application is not told any of this:
/// <c>Program.cs</c> gains no environment branch, because a bypass that start-up can switch on is
/// one wrong environment variable away from an open administration.
/// </remarks>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>The name of the scheme this handler is registered under.</summary>
    public const string SchemeName = "TestStaff";

    /// <summary>Present on a request means "sign this one in"; absent means anonymous.</summary>
    public const string HeaderName = "X-Test-Staff";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    /// <summary>The name the principal carries, so an audit assertion has something to read.</summary>
    public const string ActorEmail = "harness@orlandoup.test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(HeaderName))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        Claim[] claims =
        [
            new Claim(ClaimTypes.Name, ActorEmail),
            new Claim(ClaimTypes.NameIdentifier, ActorEmail),
            new Claim(ClaimTypes.Role, Roles.Admin),
        ];

        ClaimsPrincipal principal = new(new ClaimsIdentity(claims, SchemeName));

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
