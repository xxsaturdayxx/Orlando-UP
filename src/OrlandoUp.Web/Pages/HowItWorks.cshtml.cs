using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrlandoUp.Application.Catalog;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages;

public class HowItWorksModel : PageModel
{
    private readonly CatalogQueries _catalog;

    public HowItWorksModel(CatalogQueries catalog) => _catalog = catalog;

    /// <summary>
    /// The hand-over text comes from the delivery zones, not from a resource file: it is the same
    /// sentence the customer will see on the booking, and one source keeps them from drifting.
    /// </summary>
    public ZoneInstructions? DisneyZone { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<ZoneInstructions> zones =
            await _catalog.ActiveZonesAsync(CultureInfo.CurrentUICulture.Name, cancellationToken);

        DisneyZone = zones.FirstOrDefault(zone => zone.Code == "disney-resorts");
    }
}
