using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace OrlandoUp.Pages;

public partial class FaqModel : PageModel
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public FaqModel(IStringLocalizer<SharedResource> localizer) => _localizer = localizer;

    /// <summary>
    /// The numbers of the questions the resource file actually carries, in order.
    /// </summary>
    /// <remarks>
    /// Read from the resources instead of counted in the page, so that adding a question is one
    /// edit in two .resx files and nothing else. A hard-coded bound is the shape that silently
    /// stops rendering the eleventh question, and no test of the page alone would notice.
    /// </remarks>
    public IReadOnlyList<int> Numbers { get; private set; } = [];

    public void OnGet()
    {
        Numbers = _localizer.GetAllStrings(includeParentCultures: true)
            .Select(entry => QuestionKey().Match(entry.Name))
            .Where(match => match.Success)
            .Select(match => int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture))
            .Distinct()
            .OrderBy(number => number)
            .ToList();
    }

    [GeneratedRegex(@"^Faq_Q([0-9]+)$")]
    private static partial Regex QuestionKey();
}
