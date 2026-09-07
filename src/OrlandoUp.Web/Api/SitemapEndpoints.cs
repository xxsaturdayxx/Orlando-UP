using System.Text;
using System.Xml;
using Microsoft.Extensions.Options;
using OrlandoUp.Application;
using OrlandoUp.Infrastructure.Data;
using OrlandoUp.Infrastructure.Localization;

namespace OrlandoUp.Api;

/// <summary>
/// The machine-readable index of the site: every public page in both cultures, and every product
/// that is on the catalog today.
/// </summary>
/// <remarks>
/// It is served while indexing is off, and says nothing about it (D8/02): the file is correct from
/// the day the site is closed, and phase 5 flips the flag with the sitemap already right.
///
/// There is no lastmod. The schema has no per-page modification date, and a fabricated one is
/// worse than none - it teaches a crawler to come back for changes that never happened. It is also
/// the mechanical reason this file reads no clock at all.
/// </remarks>
public static class SitemapEndpoints
{
    public static void MapSitemap(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/sitemap.xml",
            async (
                PublicPages pages,
                CatalogQueries catalog,
                IOptions<SeoOptions> seo,
                HttpContext context,
                CancellationToken cancellation) =>
            {
                string origin = $"{context.Request.Scheme}://{seo.Value.CanonicalHost}";

                // The same read the pages do, so a product hidden by an administrator cannot leak
                // through a second code path into a search engine.
                IReadOnlyList<Application.Catalog.ProductCard> cards =
                    await catalog.ActiveCardsAsync(SiteCultures.English, cancellation);

                string xml = Render(origin, pages, cards.Select(card => card.Slug).ToList());

                return Results.Text(xml, "application/xml", Encoding.UTF8);
            }).AllowAnonymous();
    }

    private static string Render(string origin, PublicPages pages, IReadOnlyList<string> slugs)
    {
        StringBuilder builder = new();

        XmlWriterSettings settings = new()
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false,
        };

        // The writer takes the encoding of the writer it is given, not the one in its settings, and
        // a StringWriter is UTF-16. Writing to a plain StringBuilder therefore produced a document
        // that DECLARED utf-16 while the response shipped utf-8 - a header contradicting its own
        // bytes, which is the kind of thing a parser is entitled to refuse.
        using (Utf8StringWriter text = new(builder))
        using (XmlWriter writer = XmlWriter.Create(text, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
            writer.WriteAttributeString("xmlns", "xhtml", null, "http://www.w3.org/1999/xhtml");

            foreach (string name in pages.Names())
            {
                WriteAddress(writer, origin, culture => pages.PathFor(name, culture));
            }

            foreach (string slug in slugs)
            {
                WriteAddress(writer, origin, culture => pages.PathFor("/Rentals/Details", culture, slug));
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return builder.ToString();
    }

    /// <summary>
    /// One entry per culture, each carrying the whole alternate set - which is what a search engine
    /// asks for, and the same set of links the page itself puts in its head.
    /// </summary>
    private static void WriteAddress(XmlWriter writer, string origin, Func<string, string?> pathFor)
    {
        List<(string Culture, string Url)> addresses = [];

        foreach ((string segment, string culture) in PublicPages.Cultures)
        {
            string? path = pathFor(segment);

            if (path is not null)
            {
                addresses.Add((culture, origin + path));
            }
        }

        if (addresses.Count == 0)
        {
            return;
        }

        string english = addresses[0].Url;

        foreach ((_, string url) in addresses)
        {
            writer.WriteStartElement("url");
            writer.WriteElementString("loc", url);

            foreach ((string culture, string alternate) in addresses)
            {
                WriteAlternate(writer, culture, alternate);
            }

            WriteAlternate(writer, "x-default", english);

            writer.WriteEndElement();
        }
    }

    private static void WriteAlternate(XmlWriter writer, string hreflang, string href)
    {
        writer.WriteStartElement("link", "http://www.w3.org/1999/xhtml");
        writer.WriteAttributeString("rel", "alternate");
        writer.WriteAttributeString("hreflang", hreflang);
        writer.WriteAttributeString("href", href);
        writer.WriteEndElement();
    }

    /// <summary>A string writer that admits to being UTF-8, so the declaration tells the truth.</summary>
    private sealed class Utf8StringWriter : System.IO.StringWriter
    {
        public Utf8StringWriter(StringBuilder builder)
            : base(builder)
        {
        }

        public override Encoding Encoding => Encoding.UTF8;
    }
}
