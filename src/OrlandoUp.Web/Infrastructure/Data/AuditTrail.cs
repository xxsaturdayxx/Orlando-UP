using Microsoft.EntityFrameworkCore;
using OrlandoUp.Application;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data;

/// <summary>
/// Writes and reads the administration's record. A scoped service holding the context, registered
/// beside <see cref="CatalogQueries"/>, because it is the same shape of thing.
/// </summary>
/// <remarks>
/// <see cref="Record"/> only stages the row; the caller's <c>SaveChangesAsync</c> is what commits
/// it, so a write that fails leaves no line claiming it happened. The instant comes from
/// <see cref="IClock"/> and never from the machine's own clock — control C06 of
/// <c>foundation.tsv</c> expects the real clock to be read in one file, and this is not it.
/// </remarks>
public sealed class AuditTrail
{
    /// <summary>What <c>/admin/audit</c> shows. A hundred lines is one screen of scrolling.</summary>
    public const int RecentCount = 100;

    private readonly AppDbContext _db;
    private readonly IClock _clock;

    public AuditTrail(AppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    /// <summary>
    /// Stages one line. <paramref name="entityType"/> is written from <c>nameof</c> by every caller
    /// and never typed out, so a renamed class takes its audit rows' vocabulary with it.
    /// </summary>
    public void Record(string? actorEmail, string entityType, int entityId, AuditAction action, string summary)
    {
        _db.AuditEntries.Add(new AuditEntry
        {
            OccurredAtUtc = _clock.UtcNow,

            // An unnamed actor is still a fact worth recording; refusing the row would lose the
            // change as well as the name.
            ActorEmail = string.IsNullOrWhiteSpace(actorEmail) ? "unknown" : actorEmail,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Summary = summary,
        });
    }

    /// <summary>The most recent lines, newest first — the only read this table has.</summary>
    public async Task<IReadOnlyList<AuditEntry>> RecentAsync(CancellationToken cancellation)
    {
        return await _db.AuditEntries
            .AsNoTracking()
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .ThenByDescending(entry => entry.Id)
            .Take(RecentCount)
            .ToListAsync(cancellation);
    }
}
