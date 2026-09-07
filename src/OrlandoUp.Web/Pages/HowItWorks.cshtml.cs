using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrlandoUp.Application.Catalog;
using OrlandoUp.Domain;
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

        // By what the zone DOES, not by what it is called: the page wants the place where the
        // hand-over happens in person, and a code typed here would be fleet data living in source.
        DisneyZone = zones.FirstOrDefault(zone => zone.Handover == HandoverMode.MeetAndGreet);
    }
}
