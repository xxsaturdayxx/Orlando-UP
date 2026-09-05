using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrlandoUp.Pages.Error;

/// <summary>
/// The one error page, re-executed by the pipeline with the status code in the address. It answers
/// in the visitor's language because it goes through the same localization the rest of the site
/// does; only the two codes we have words for get their own wording.
/// </summary>
public class StatusModel : PageModel
{
    public string? TitleKey { get; private set; }

    public string? TextKey { get; private set; }

    public void OnGet(int code)
    {
        if (code is 404 or 500)
        {
            TitleKey = $"Error_{code}_Title";
            TextKey = $"Error_{code}_Text";
        }

        Response.StatusCode = code is >= 400 and <= 599 ? code : StatusCodes.Status500InternalServerError;
    }
}
