using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrlandoUp.Pages.Admin;

/// <summary>
/// Signing out happens on a POST only. A sign-out reachable by a link is a sign-out any page on the
/// internet can trigger by embedding an image.
/// </summary>
public class LogoutModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signIn;

    public LogoutModel(SignInManager<IdentityUser> signIn) => _signIn = signIn;

    public IActionResult OnGet() => RedirectToPage("/Admin/Index");

    public async Task<IActionResult> OnPostAsync()
    {
        await _signIn.SignOutAsync();

        return RedirectToPage("/Admin/Login");
    }
}
