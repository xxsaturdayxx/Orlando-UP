using Microsoft.AspNetCore.Mvc.RazorPages;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin.Audit;

/// <summary>
/// The hundred most recent lines of the record, newest first, read-only and unfiltered (K2 a). A
/// filter buys little while one person writes, and becomes its own front when there are two.
/// </summary>
public class IndexModel : PageModel
{
    private readonly AuditTrail _audit;

    public IndexModel(AuditTrail audit) => _audit = audit;

    public IReadOnlyList<AuditEntry> Entries { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Entries = await _audit.RecentAsync(cancellationToken);
}
