namespace OrlandoUp.Application.Catalog;

/// <summary>
/// Which row of a translation table answers for the culture being served.
/// </summary>
/// <remarks>
/// The rule, decided on 2026-09-04: the culture asked for, and when that row does not exist, the
/// English one. A visitor never gets a 404 because a translator has not caught up; a missing
/// translation degrades to the language we always write first. Only when there is no English row
/// either is there nothing to show, and that is the caller's decision to make.
/// </remarks>
public static class TranslationPicker
{
    public static T? For<T>(IEnumerable<T> translations, string culture, Func<T, string> cultureOf)
        where T : class
    {
        List<T> all = translations.ToList();

        foreach (T candidate in all)
        {
            if (string.Equals(cultureOf(candidate), culture, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        foreach (T candidate in all)
        {
            if (string.Equals(cultureOf(candidate), SiteCultures.English, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return null;
    }
}
