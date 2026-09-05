using Microsoft.Extensions.Options;
using OrlandoUp.Application;

namespace OrlandoUp.Api;

/// <summary>
/// The crawler instructions. While indexing is off — and it is off by default (D11/01) — the file
/// closes the whole site, because a catalog of placeholder prices indexed once is hard to unindex.
/// </summary>
public static class RobotsEndpoints
{
    public static void MapRobotsTxt(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/robots.txt", (IOptions<SeoOptions> seo) =>
        {
            string body = seo.Value.AllowIndexing
                ? "User-agent: *\nAllow: /\n"
                : "User-agent: *\nDisallow: /\n";

            return Results.Text(body, "text/plain");
        }).AllowAnonymous();
    }
}
