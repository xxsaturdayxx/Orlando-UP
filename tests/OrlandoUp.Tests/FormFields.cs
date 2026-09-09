using System.Text.RegularExpressions;
using System.Web;

namespace OrlandoUp.Tests;

/// <summary>
/// The fields of a rendered form, read out of the HTML the way a browser would gather them before
/// posting: every input with a value, every textarea's content, the selected option of every
/// select, and a checkbox only when it is checked.
/// </summary>
/// <remarks>
/// Reading the real markup rather than typing the field names into each test is what makes an
/// assertion about "the editor saved what it was given" mean anything: a field the page stopped
/// rendering disappears from the post too, and the test that depended on it fails instead of
/// silently posting a name the handler no longer binds.
///
/// It is a list of pairs and not a dictionary because a form is allowed to repeat a name — the
/// checkbox-plus-hidden pair that carries a bool, and the add-on links, both do.
/// </remarks>
public sealed class FormFields
{
    private static readonly Regex Tag = new(
        """<(?<kind>input|textarea|select)\b(?<attrs>[^>]*)>""",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

    private static readonly Regex Attribute = new(
        """"(?<name>[A-Za-z-]+)\s*=\s*"(?<value>[^"]*)"""",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private readonly List<KeyValuePair<string, string>> _pairs = [];

    public IReadOnlyList<KeyValuePair<string, string>> Pairs => _pairs;

    /// <summary>
    /// The fields of the page's own form — the LAST one in the document.
    /// </summary>
    /// <remarks>
    /// Scoping matters and the reason was measured rather than guessed: every administration page
    /// carries two forms, because <c>_AdminLayout</c> puts the sign-out button in the header. Read
    /// over the whole document, the antiforgery field is gathered twice, posts as
    /// <c>token,token</c>, and every write is answered with 400 — a failure that looks exactly like
    /// a missing token and is not one. The page's own form is the last, in every layout here.
    /// </remarks>
    public static FormFields ReadFrom(string html)
    {
        FormFields fields = new();
        int opening = html.LastIndexOf("<form", StringComparison.OrdinalIgnoreCase);

        if (opening >= 0)
        {
            int closing = html.IndexOf("</form>", opening, StringComparison.OrdinalIgnoreCase);

            html = closing < 0 ? html[opening..] : html[opening..closing];
        }

        foreach (Match tag in Tag.Matches(html))
        {
            Dictionary<string, string> attributes = new(StringComparer.OrdinalIgnoreCase);

            foreach (Match attribute in Attribute.Matches(tag.Groups["attrs"].Value))
            {
                attributes[attribute.Groups["name"].Value] = attribute.Groups["value"].Value;
            }

            if (!attributes.TryGetValue("name", out string? name) || name.Length == 0)
            {
                continue;
            }

            string kind = tag.Groups["kind"].Value.ToLowerInvariant();

            if (kind == "input")
            {
                string type = attributes.TryGetValue("type", out string? t) ? t.ToLowerInvariant() : "text";

                // An unchecked box sends nothing at all, which is exactly how "false" reaches the
                // handler; a box that only LOOKS unchecked would otherwise still post.
                if ((type == "checkbox" || type == "radio")
                    && !tag.Groups["attrs"].Value.Contains("checked", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                fields.Add(name, attributes.TryGetValue("value", out string? value) ? Decode(value) : string.Empty);

                continue;
            }

            if (kind == "textarea")
            {
                fields.Add(name, Decode(Between(html, tag.Index + tag.Length, "</textarea>")));

                continue;
            }

            string options = Between(html, tag.Index + tag.Length, "</select>");
            Match selected = Regex.Match(
                options,
                """"<option\s[^>]*selected[^>]*value="(?<value>[^"]*)"""",
                RegexOptions.IgnoreCase);

            if (!selected.Success)
            {
                selected = Regex.Match(
                    options,
                    """<option\s[^>]*value="(?<value>[^"]*)"[^>]*selected""",
                    RegexOptions.IgnoreCase);
            }

            if (selected.Success)
            {
                fields.Add(name, Decode(selected.Groups["value"].Value));
            }
        }

        return fields;
    }

    public FormFields Add(string name, string value)
    {
        _pairs.Add(new KeyValuePair<string, string>(name, value));

        return this;
    }

    /// <summary>Replaces every pair carrying this name with one pair.</summary>
    public FormFields Set(string name, string value)
    {
        Remove(name);

        return Add(name, value);
    }

    public FormFields Remove(string name)
    {
        _pairs.RemoveAll(pair => string.Equals(pair.Key, name, StringComparison.Ordinal));

        return this;
    }

    public string? Value(string name) => _pairs
        .Where(pair => string.Equals(pair.Key, name, StringComparison.Ordinal))
        .Select(pair => pair.Value)
        .LastOrDefault();

    public FormUrlEncodedContent AsContent() => new(_pairs);

    private static string Between(string html, int start, string closing)
    {
        int end = html.IndexOf(closing, start, StringComparison.OrdinalIgnoreCase);

        return end < 0 ? string.Empty : html[start..end];
    }

    private static string Decode(string value) => HttpUtility.HtmlDecode(value);
}
