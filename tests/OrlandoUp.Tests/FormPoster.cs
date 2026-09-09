using System.Net.Http;
using System.Text.RegularExpressions;

namespace OrlandoUp.Tests;

/// <summary>
/// Posts a form the way a browser does: reads the page first, takes the antiforgery token out of
/// the rendered HTML, and sends it in the body (D2/04).
/// </summary>
/// <remarks>
/// Antiforgery stays ON in the test host. Turning it off would make the suite green on a form that
/// cannot be posted in production, and it would stop proving that the markup is a real Razor
/// <c>&lt;form method="post"&gt;</c> — the only shape into which the hidden field is injected. The
/// cookie half of the token travels on its own, because the client of
/// <c>WebApplicationFactory</c> keeps cookies.
/// </remarks>
public static class FormPoster
{
    /// <summary>The name Razor Pages gives the hidden input and the form field.</summary>
    public const string TokenFieldName = "__RequestVerificationToken";

    private static readonly Regex HiddenToken = new(
        """<input\s[^>]*name="__RequestVerificationToken"[^>]*value="(?<token>[^"]+)""",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Reads <paramref name="pagePath"/> and returns the antiforgery token rendered in it. Throws
    /// when the page carries no token, because a helper that quietly returned an empty string would
    /// turn every "the token was accepted" assertion into a test of nothing.
    /// </summary>
    public static async Task<string> ReadTokenAsync(HttpClient client, string pagePath)
    {
        HttpResponseMessage page = await client.GetAsync(pagePath);

        page.EnsureSuccessStatusCode();

        string html = await page.Content.ReadAsStringAsync();
        Match match = HiddenToken.Match(html);

        if (!match.Success)
        {
            throw new InvalidOperationException(
                $"No {TokenFieldName} was rendered by {pagePath}. Either the page has no form with " +
                "method=\"post\", or it was not reached.");
        }

        return match.Groups["token"].Value;
    }

    /// <summary>
    /// Posts <paramref name="fields"/> to <paramref name="formPath"/> carrying a token read from
    /// <paramref name="tokenPagePath"/>.
    /// </summary>
    public static async Task<HttpResponseMessage> PostWithTokenAsync(
        HttpClient client,
        string tokenPagePath,
        string formPath,
        IDictionary<string, string>? fields = null)
    {
        string token = await ReadTokenAsync(client, tokenPagePath);

        Dictionary<string, string> body = fields is null
            ? []
            : new Dictionary<string, string>(fields, StringComparer.Ordinal);

        body[TokenFieldName] = token;

        return await client.PostAsync(formPath, new FormUrlEncodedContent(body));
    }

    /// <summary>
    /// Reads a page, gathers the fields its form actually rendered, and hands them back with the
    /// antiforgery token already in place, for a test to adjust and post.
    /// </summary>
    public static async Task<FormFields> ReadFormAsync(HttpClient client, string pagePath)
    {
        HttpResponseMessage page = await client.GetAsync(pagePath);

        page.EnsureSuccessStatusCode();

        string html = await page.Content.ReadAsStringAsync();
        FormFields fields = FormFields.ReadFrom(html);

        if (fields.Value(TokenFieldName) is null)
        {
            throw new InvalidOperationException(
                $"No {TokenFieldName} was rendered by {pagePath}. Either the page has no form with " +
                "method=\"post\", or it was not reached.");
        }

        return fields;
    }

    /// <summary>Posts fields gathered by <see cref="ReadFormAsync"/>, token included.</summary>
    public static Task<HttpResponseMessage> PostAsync(HttpClient client, string formPath, FormFields fields) =>
        client.PostAsync(formPath, fields.AsContent());

    /// <summary>
    /// The same post with no token in the body. It exists so a test can show that the accepted post
    /// was accepted BECAUSE of the token, which a single green post never shows on its own.
    /// </summary>
    public static async Task<HttpResponseMessage> PostWithoutTokenAsync(
        HttpClient client,
        string formPath,
        IDictionary<string, string>? fields = null)
    {
        Dictionary<string, string> body = fields is null
            ? []
            : new Dictionary<string, string>(fields, StringComparer.Ordinal);

        return await client.PostAsync(formPath, new FormUrlEncodedContent(body));
    }
}
