using Microsoft.EntityFrameworkCore;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Api;

/// <summary>The only endpoint of this release, and the one the cloud health probe will call.</summary>
public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/healthz", async (AppDbContext db, CancellationToken cancellation) =>
        {
            bool reachable = await db.Database.CanConnectAsync(cancellation);

            return reachable
                ? Results.Json(new { status = "ok", database = "ok" }, statusCode: StatusCodes.Status200OK)
                : Results.Json(new { status = "degraded", database = "unreachable" }, statusCode: StatusCodes.Status503ServiceUnavailable);
        }).AllowAnonymous();
    }
}
