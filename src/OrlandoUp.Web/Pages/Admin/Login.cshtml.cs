using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace OrlandoUp.Pages.Admin;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signIn;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public LoginModel(SignInManager<IdentityUser> signIn, IStringLocalizer<SharedResource> localizer)
    {
        _signIn = signIn;
        _localizer = localizer;
    }

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? Problem { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            Problem = _localizer["Admin_InvalidLogin"];

            return Page();
        }

        // Lockout is on: a password that can be tried without limit is a password with no length.
        Microsoft.AspNetCore.Identity.SignInResult result = await _signIn.PasswordSignInAsync(
            Email, Password, isPersistent: false, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            Problem = _localizer["Admin_LockedOut"];

            return Page();
        }

        if (!result.Succeeded)
        {
            // One message for a wrong address and for a wrong password: telling them apart tells an
            // attacker which addresses exist.
            Problem = _localizer["Admin_InvalidLogin"];

            return Page();
        }

        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/admin");
    }
}
