using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrlandoUp.Application.Catalog;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages;

public class DeliveryAreasModel : PageModel
{
    private readonly CatalogQueries _catalog;

    public DeliveryAreasModel(CatalogQueries catalog) => _catalog = catalog;

    /// <summary>
    /// The zones as the database holds them, in the order an administrator put them in. The
    /// hand-over sentence a visitor reads here is the same string the booking will show, and one
    /// source is the only way that stays true (D5/02). Nothing is duplicated into a resource file.
    /// </summary>
    public IReadOnlyList<ZoneInstructions> Zones { get; private set; } = [];

    /// <summary>The example places of each zone, by zone code.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Locations { get; private set; } =
        new Dictionary<string, IReadOnlyList<string>>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        string culture = CultureInfo.CurrentUICulture.Name;

        Zones = await _catalog.ActiveZonesAsync(culture, cancellationToken);
        Locations = await _catalog.ActiveLocationsByZoneAsync(cancellationToken);
    }
}
