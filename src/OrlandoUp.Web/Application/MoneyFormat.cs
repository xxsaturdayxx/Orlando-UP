using System.Globalization;

namespace OrlandoUp.Application;

/// <summary>
/// Money is written the same way in both languages, because the formatting culture never moves
/// (D20): the site charges in United States dollars and a comma where a point belongs is how a
/// bound decimal becomes a wrong number.
/// </summary>
public static class MoneyFormat
{
    private static readonly CultureInfo Formatting = CultureInfo.GetCultureInfo(SiteCultures.English);

    /// <summary>An amount as the pages show it, for example US$ 27.00.</summary>
    public static string Usd(decimal amount) => "US$ " + amount.ToString("0.00", Formatting);

    /// <summary>A number as the pages show it, for example 12.5.</summary>
    public static string Number(decimal value) => value.ToString("0.#", Formatting);
}
