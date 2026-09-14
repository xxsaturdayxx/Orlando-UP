using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrlandoUp.Application;
using OrlandoUp.Application.Catalog;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin.Bookings;

/// <summary>
/// Where a reservation that arrived by WhatsApp, phone or e-mail lands.
/// </summary>
/// <remarks>
/// It computes the same availability the visitor sees and refuses by default what the fleet cannot
/// serve — and it lets the operator enter it anyway, marked, when he has decided to solve it by
/// hand (D4/03). That is the difference between the two surfaces: the public page is a barrier,
/// this one is a warning.
/// </remarks>
public class CreateModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly BookingWriter _writer;
    private readonly BookingTimeline _timeline;
    private readonly IClock _clock;

    public CreateModel(AppDbContext db, BookingWriter writer, BookingTimeline timeline, IClock clock)
    {
        _db = db;
        _writer = writer;
        _timeline = timeline;
        _clock = clock;
    }

    /// <summary>One row of the equipment table: every bookable product, whether wanted or not.</summary>
    public sealed record LineRow(int ProductId, string Name, bool IsScooter, IReadOnlyList<AddOnChoice> AddOns);

    public sealed record AddOnChoice(int Id, string Name);

    public sealed record PlaceChoice(string Value, string Label, string Group, bool NeedsAddress);

    public IReadOnlyList<LineRow> Lines { get; private set; } = [];

    public IReadOnlyList<PlaceChoice> Places { get; private set; } = [];

    public decimal DefaultExtraBatteryPerDay { get; private set; }

    [BindProperty] public string FirstName { get; set; } = string.Empty;
    [BindProperty] public string LastName { get; set; } = string.Empty;
    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Phone { get; set; } = string.Empty;
    [BindProperty] public string CustomerCulture { get; set; } = SiteCultures.English;
    [BindProperty] public DateOnly StartDate { get; set; }
    [BindProperty] public DateOnly EndDate { get; set; }
    [BindProperty] public DeliveryWindow DeliveryWindow { get; set; } = Domain.DeliveryWindow.Morning;
    [BindProperty] public DeliveryWindow PickupWindow { get; set; } = Domain.DeliveryWindow.Afternoon;
    [BindProperty] public string Place { get; set; } = string.Empty;
    [BindProperty] public string? Address { get; set; }
    [BindProperty] public string? DeliveryNotes { get; set; }
    [BindProperty] public string? StaffNotes { get; set; }
    [BindProperty] public bool Overbook { get; set; }

    /// <summary>Quantity per product id, posted as <c>Quantity_{id}</c>.</summary>
    [BindProperty] public Dictionary<int, int> Quantity { get; set; } = [];

    [BindProperty] public Dictionary<int, int> ExtraBatteries { get; set; } = [];

    [BindProperty] public Dictionary<int, decimal> ExtraBatteryPerDay { get; set; } = [];

    /// <summary>Chosen add-on ids per product id, posted as <c>AddOns_{id}</c>.</summary>
    [BindProperty] public Dictionary<int, List<int>> AddOns { get; set; } = [];

    /// <summary>A resource key, never a sentence: the view localizes it.</summary>
    public string? Error { get; private set; }

    /// <summary>Filled into <see cref="Error"/>'s placeholder when it has one.</summary>
    public string? ErrorArgument { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);

        StartDate = BookingRules.EarliestStaffStart(_clock.TodayInOrlando());
        EndDate = StartDate.AddDays(2);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);

        List<QuoteLineAsked> asked = [];

        foreach (LineRow row in Lines)
        {
            int quantity = Quantity.TryGetValue(row.ProductId, out int q) ? q : 0;

            if (quantity <= 0)
            {
                continue;
            }

            int extras = ExtraBatteries.TryGetValue(row.ProductId, out int e) ? e : 0;
            decimal perDay = ExtraBatteryPerDay.TryGetValue(row.ProductId, out decimal p)
                ? p
                : DefaultExtraBatteryPerDay;

            List<int> chosen = AddOns.TryGetValue(row.ProductId, out List<int>? ids) ? ids : [];

            asked.Add(new QuoteLineAsked(row.ProductId, quantity, extras, perDay, chosen));
        }

        if (asked.Count == 0)
        {
            return Refuse("Admin_ErrorNoLine");
        }

        // The calendar, refused here because the writer has no opinion about it: a delivery day
        // already past would hold equipment on days that will never happen (D9/03). The cut-off
        // does NOT bind staff — they enter what they have already decided to deliver.
        if (StartDate < BookingRules.EarliestStaffStart(_clock.TodayInOrlando()))
        {
            return Refuse("Admin_ErrorStartInPast");
        }

        if (EndDate < StartDate)
        {
            return Refuse("Admin_ErrorEndBeforeStart");
        }

        if (!TryReadPlace(out int zoneId, out int? locationId, out bool needsAddress))
        {
            return Refuse("Admin_ErrorAddressRequired");
        }

        if (needsAddress && string.IsNullOrWhiteSpace(Address))
        {
            return Refuse("Admin_ErrorAddressRequired");
        }

        StaffBookingDetails details = new(
            CustomerCulture,
            (FirstName ?? string.Empty).Trim(),
            (LastName ?? string.Empty).Trim(),
            (Email ?? string.Empty).Trim(),
            (Phone ?? string.Empty).Trim(),
            zoneId,
            locationId,
            string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
            string.IsNullOrWhiteSpace(DeliveryNotes) ? null : DeliveryNotes.Trim(),
            StartDate,
            EndDate,
            DeliveryWindow,
            PickupWindow,
            string.IsNullOrWhiteSpace(StaffNotes) ? null : StaffNotes.Trim());

        BookingWriteResult result = await _writer.CreateByStaffAsync(
            details, asked, User.Identity?.Name, Overbook, cancellationToken);

        if (!result.Succeeded)
        {
            // The quote's refusals are translated, never re-validated: the shape of a line is the
            // domain's opinion and this page only puts words to it.
            if (result.Quote is QuoteResult quote)
            {
                return Refuse(KeyFor(quote.Problem), quote.Problem == QuoteProblem.TooLong
                    ? BookingRules.MaxDays.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : null);
            }

            return Refuse("Admin_ErrorNotAvailable", Describe(result.Shortfalls));
        }

        Booking booking = result.Booking!;

        _timeline.Record(
            User.Identity?.Name,
            booking.Id,
            BookingEventType.Created,
            booking.IsOverbooked
                ? $"Booking {booking.Number} entered by staff, overbooked above the fleet."
                : $"Booking {booking.Number} entered by staff.");

        await _db.SaveChangesAsync(cancellationToken);

        TempData["Flash"] = "Admin_BookingCreated";
        TempData["FlashArgument"] = booking.Number;

        return RedirectToPage("/Admin/Bookings/Details", new { id = booking.Id });
    }

    /// <summary>
    /// "Drive Scout 4: 3 of 4 available; 1 second battery short" — composed from what the rule
    /// answered, so the operator can see which of the three pools ran out.
    /// </summary>
    private static string Describe(IReadOnlyList<Shortfall> shortfalls)
    {
        List<string> parts = [];

        foreach (Shortfall shortfall in shortfalls)
        {
            string part = $"{shortfall.ProductName}: {shortfall.MaxQuantity} de {shortfall.Asked}";

            if (shortfall.AskedExtras > shortfall.MaxExtras)
            {
                part += $"; {shortfall.AskedExtras - shortfall.MaxExtras} segunda(s) bateria(s) a menos";
            }

            parts.Add(part);
        }

        return string.Join("; ", parts);
    }

    private static string KeyFor(QuoteProblem problem) => problem switch
    {
        QuoteProblem.EndBeforeStart => "Admin_ErrorEndBeforeStart",
        QuoteProblem.TooLong => "Admin_ErrorTooLong",
        QuoteProblem.NoLines => "Admin_ErrorNoLine",
        QuoteProblem.QuantityOutOfRange => "Admin_ErrorNoLine",
        QuoteProblem.ExtrasOnNonScooter => "Admin_ErrorExtraOnlyScooters",
        QuoteProblem.ExtrasAboveQuantity => "Admin_ErrorExtraAboveQuantity",
        QuoteProblem.NegativeAmount => "Admin_ErrorNegativeAmount",
        _ => "Book_ErrorUnavailableNow",
    };

    private IActionResult Refuse(string key, string? argument = null)
    {
        Error = key;
        ErrorArgument = argument;

        return Page();
    }

    /// <summary>
    /// The select carries <c>L{id}</c> for a curated location and <c>Z{id}</c> for a zone that has
    /// no list. A zone option needs a typed address; a location does not.
    /// </summary>
    private bool TryReadPlace(out int zoneId, out int? locationId, out bool needsAddress)
    {
        zoneId = 0;
        locationId = null;
        needsAddress = false;

        PlaceChoice? choice = Places.SingleOrDefault(place => place.Value == Place);

        if (choice is null)
        {
            return false;
        }

        needsAddress = choice.NeedsAddress;

        int key = int.Parse(choice.Value[1..], System.Globalization.CultureInfo.InvariantCulture);

        if (choice.Value[0] == 'Z')
        {
            zoneId = key;

            return true;
        }

        DeliveryLocation? location = _db.DeliveryLocations.AsNoTracking().SingleOrDefault(row => row.Id == key);

        if (location is null)
        {
            return false;
        }

        zoneId = location.ZoneId;
        locationId = location.Id;

        return true;
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        string culture = System.Globalization.CultureInfo.CurrentUICulture.Name;

        List<Product> products = await _db.Products
            .AsNoTracking()
            .Include(product => product.Translations)
            .Include(product => product.AddOns)
                .ThenInclude(link => link.AddOn!.Translations)
            .Where(product => product.IsActive && product.IsBookable)
            .OrderBy(product => product.SortOrder)
            .ToListAsync(cancellationToken);

        Lines = products.Select(product => new LineRow(
            product.Id,
            NameOf(product, culture),
            product.Category == ProductCategory.MobilityScooter,
            product.AddOns
                .Where(link => link.AddOn!.IsActive)
                .OrderBy(link => link.AddOn!.SortOrder)
                .Select(link => new AddOnChoice(link.AddOn!.Id, NameOf(link.AddOn!, culture)))
                .ToList()))
            .ToList();

        Places = await PlaceChoicesAsync(_db, culture, cancellationToken);

        DefaultExtraBatteryPerDay = await _db.OperationalSettings
            .AsNoTracking()
            .Select(row => row.SecondBatteryPerDay)
            .SingleAsync(cancellationToken);
    }

    /// <summary>
    /// The place select, shared with the public page: every active location grouped by its zone,
    /// and every active zone that has no location as an option of its own (D11/03).
    /// </summary>
    public static async Task<IReadOnlyList<PlaceChoice>> PlaceChoicesAsync(
        AppDbContext db, string culture, CancellationToken cancellationToken)
    {
        List<DeliveryZone> zones = await db.DeliveryZones
            .AsNoTracking()
            .Include(zone => zone.Translations)
            .Include(zone => zone.Locations)
            .Where(zone => zone.IsActive)
            .OrderBy(zone => zone.SortOrder)
            .ToListAsync(cancellationToken);

        List<PlaceChoice> places = [];

        foreach (DeliveryZone zone in zones)
        {
            DeliveryZoneTranslation? text = TranslationPicker.For(zone.Translations, culture, row => row.Culture);
            string zoneName = text is null ? zone.Code : text.Name;

            List<DeliveryLocation> active = zone.Locations
                .Where(location => location.IsActive)
                .OrderBy(location => location.SortOrder)
                .ToList();

            if (active.Count == 0)
            {
                places.Add(new PlaceChoice($"Z{zone.Id}", zoneName, zoneName, true));

                continue;
            }

            foreach (DeliveryLocation location in active)
            {
                places.Add(new PlaceChoice($"L{location.Id}", location.Name, zoneName, false));
            }
        }

        return places;
    }

    private static string NameOf(Product product, string culture)
    {
        ProductTranslation? text = TranslationPicker.For(product.Translations, culture, row => row.Culture);

        return text is null ? product.Slug : text.Name;
    }

    private static string NameOf(AddOn addOn, string culture)
    {
        AddOnTranslation? text = TranslationPicker.For(addOn.Translations, culture, row => row.Culture);

        return text is null ? addOn.Code : text.Name;
    }

    /// <summary>The four windows as a select, labelled by resource key.</summary>
    public static IEnumerable<SelectListItem> WindowItems(DeliveryWindow selected) =>
        Enum.GetValues<DeliveryWindow>().Select(window => new SelectListItem(
            window.ToString(), window.ToString(), window == selected));
}
