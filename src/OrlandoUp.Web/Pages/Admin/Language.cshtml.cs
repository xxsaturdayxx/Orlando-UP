using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrlandoUp.Application;

namespace OrlandoUp.Pages.Admin;

/// <summary>Writes the culture cookie the administration reads, then goes back where it came from.</summary>
public class LanguageModel : PageModel
{
    public IActionResult OnGet(string culture, string? returnUrl)
    {
        // Only the cultures the site actually serves, so the cookie cannot be used to ask for one
        // the application has no resources for.
        string chosen = culture == SiteCultures.Portuguese ? SiteCultures.Portuguese : SiteCultures.English;

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(SiteCultures.English, chosen)),
            new CookieOptions
            {
                Expires = DateTimeOffset.MaxValue,
                IsEssential = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
            });

        // Only an address inside this site: an open redirect here would be a phishing hop wearing
        // our domain.
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/admin");
    }
}
