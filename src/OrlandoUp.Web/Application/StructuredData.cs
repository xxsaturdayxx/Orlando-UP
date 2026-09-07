using System.Text.Json;
using System.Text.Json.Serialization;
using OrlandoUp.Application.Catalog;

namespace OrlandoUp.Application;

/// <summary>
/// The machine-readable description of the business, and of a product when there is one on the
/// page. One entry point, so there is one place that decides what the site claims about itself.
/// </summary>
/// <remarks>
/// Two rules govern everything below, and both are about not claiming what is not true.
///
/// First: a company field that is still a marked placeholder is OMITTED, never emitted. A search
/// engine reading a telephone number of "TODO-phone" would publish it, and a marker is worse than
/// a gap because it looks like data.
///
/// Second: there is no Offer. An offer carries a price and an availability, and a search engine is
/// entitled to show both and a visitor to try to act on them - which is a promise this site cannot
/// keep until a checkout exists behind it (D7/02). The offer arrives with the booking flow, not
/// before.
///
/// There is also no brand, and that is a deliberate absence rather than an oversight: the schema
/// has no column for a manufacturer, so writing one here would mean typing fleet data into code -
/// the same defect that made the administration page reference a slug that stopped existing. It
/// comes back when a product row can carry it.
/// </remarks>
public static class StructuredData
{
    /// <summary>
    /// The encoder matters more than it looks, and the relaxed one would be a way in.
    /// </summary>
    /// <remarks>
    /// This JSON is written inside a &lt;script&gt; element, and the content of a script element is
    /// RAW TEXT: the browser does not decode character references there, it only looks for the
    /// closing tag. So a product name or tagline - columns an administrator edits - containing the
    /// six characters that close a script tag would end the block and let whatever follows run as
    /// code. <c>UnsafeRelaxedJsonEscaping</c> leaves &lt; and &gt; alone and would have shipped
    /// exactly that. The encoder below allows every letter through, so accents stay readable, and
    /// still escapes the handful of characters that are dangerous in this position.
    /// </remarks>
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = ScriptSafeEncoder(),
    };

    /// <summary>
    /// Every letter allowed through, so an accent stays an accent, and the three characters that
    /// could end the script element forbidden by name. Allowing a range does not re-forbid what is
    /// dangerous, so the second call is what does the work here - without it, allowing all of
    /// Unicode also lets a less-than through, which is the whole attack.
    /// </summary>
    private static System.Text.Encodings.Web.JavaScriptEncoder ScriptSafeEncoder()
    {
        System.Text.Encodings.Web.TextEncoderSettings settings = new();

        settings.AllowRange(System.Text.Unicode.UnicodeRanges.All);
        settings.ForbidCharacters('<', '>', '&');

        return System.Text.Encodings.Web.JavaScriptEncoder.Create(settings);
    }

    /// <summary>The JSON-LD for a page, as the layout renders it.</summary>
    public static string ForPage(CompanyOptions company, string origin, string culture, ProductDetail? product)
    {
        List<object> graph = [Business(company, origin, culture)];

        if (product is not null)
        {
            graph.Add(Product(product, culture));
        }

        Dictionary<string, object?> document = new(StringComparer.Ordinal)
        {
            ["@context"] = "https://schema.org",
            ["@graph"] = graph,
        };

        return JsonSerializer.Serialize(document, Json);
    }

    private static Dictionary<string, object?> Business(CompanyOptions company, string origin, string culture)
    {
        Dictionary<string, object?> business = new(StringComparer.Ordinal)
        {
            ["@type"] = "LocalBusiness",
            ["name"] = Written(company.TradeName),
            ["legalName"] = Written(company.LegalName),
            ["address"] = Written(company.Address),
            ["telephone"] = Written(company.Phone),
            ["email"] = Written(company.Email),
            ["openingHours"] = Written(company.Hours),
            ["url"] = origin,
            ["inLanguage"] = culture,
        };

        return Kept(business);
    }

    private static Dictionary<string, object?> Product(ProductDetail product, string culture)
    {
        Dictionary<string, object?> item = new(StringComparer.Ordinal)
        {
            ["@type"] = "Product",
            ["name"] = Written(product.Name),
            ["description"] = Written(product.Tagline),
            ["category"] = product.Category.ToString(),
            ["inLanguage"] = culture,
            ["width"] = Inches(product.WidthIn),
            ["depth"] = Inches(product.LengthIn),
        };

        return Kept(item);
    }

    private static Dictionary<string, object?>? Inches(decimal? value) =>
        value is decimal measured
            ? new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["@type"] = "QuantitativeValue",
                ["value"] = measured,
                ["unitCode"] = "INH",
            }
            : null;

    /// <summary>
    /// A value only when somebody wrote one. A marked placeholder counts as nothing written, which
    /// is the whole point: control C16 of the foundation keeps those markers alive on purpose, and
    /// they must not travel from a configuration file into a search result.
    /// </summary>
    private static string? Written(string? value) =>
        string.IsNullOrWhiteSpace(value) || value.StartsWith("TODO-", StringComparison.Ordinal)
            ? null
            : value;

    private static Dictionary<string, object?> Kept(Dictionary<string, object?> source) =>
        source
            .Where(pair => pair.Value is not null)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
}
