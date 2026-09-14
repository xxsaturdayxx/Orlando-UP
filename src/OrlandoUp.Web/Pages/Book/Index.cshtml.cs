using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OrlandoUp.Application;
using OrlandoUp.Application.Catalog;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;
using OrlandoUp.Pages.Admin.Bookings;

namespace OrlandoUp.Pages.Book;

/// <summary>
/// What a visitor gets instead of a disabled button: whether the equipment is free on his dates,
/// and what it would cost.
/// </summary>
/// <remarks>
/// <b>It is a barrier, not a warning</b> (D4/03): if the fleet cannot serve the request the page
/// says so and offers no price to act on. And it tells him something no other rental site in
/// Orlando tells him — when the machines are free but the batteries are not, it says the second
/// battery is unavailable rather than selling one that does not exist.
///
/// There is no JavaScript and no payment. The form is a GET whose state is the query string, so
/// the page re-renders honestly after every change and a visitor can bookmark or share a quote.
/// </remarks>
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly AvailabilityQueries _availability;
    private readonly QuoteBuilder _quotes;
    private readonly IClock _clock;

    public IndexModel(AppDbContext db, AvailabilityQueries availability, QuoteBuilder quotes, IClock clock)
    {
        _db = db;
        _availability = availability;
        _quotes = quotes;
        _clock = clock;
    }

    public sealed record ProductChoice(string Slug, string Name, bool IsScooter);

    public sealed record AddOnChoice(int Id, string Name);

    public IReadOnlyList<ProductChoice> Products { get; private set; } = [];

    public IReadOnlyList<AddOnChoice> AddOns { get; private set; } = [];

    public IReadOnlyList<CreateModel.PlaceChoice> Places { get; private set; } = [];

    [BindProperty(SupportsGet = true)] public string? Product { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? Start { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? End { get; set; }
    [BindProperty(SupportsGet = true)] public int Quantity { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int ExtraBatteries { get; set; }
    [BindProperty(SupportsGet = true)] public string? Place { get; set; }
    [BindProperty(SupportsGet = true)] public List<int> Extras { get; set; } = [];

    /// <summary>The earliest day the cut-off allows, shown whether or not the visitor asked yet.</summary>
    public DateOnly Earliest { get; private set; }

    /// <summary>
    /// True when the operational settings row is absent, which is a deployment defect and not a
    /// state the site can quote from. The page says it cannot check rather than inventing a
    /// cut-off of midnight and a courtesy battery.
    /// </summary>
    public bool SettingsMissing { get; private set; }

    public decimal DefaultExtraBatteryPerDay { get; private set; }

    /// <summary>Set when the visitor asked a complete question and it could be answered.</summary>
    public bool Answered { get; private set; }

    public bool IsAvailable { get; private set; }

    public int MaxQuantity { get; private set; }

    /// <summary>True when the machines are free and the second battery is not.</summary>
    public bool SecondBatteryUnavailable { get; private set; }

    public QuoteBreakdown? Price { get; private set; }

    /// <summary>A resource key. Never a sentence — the view localizes it.</summary>
    public string? Error { get; private set; }

    public string? ErrorArgument { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);

        if (SettingsMissing)
        {
            Error = "Book_ErrorUnavailableNow";

            return;
        }

        if (string.IsNullOrWhiteSpace(Product) || Start is null || End is null || string.IsNullOrWhiteSpace(Place))
        {
            // Nothing asked yet, or asked by halves: the form stands, with no verdict attached.
            return;
        }

        ProductChoice? chosen = Products.SingleOrDefault(row => row.Slug == Product);

        if (chosen is null)
        {
            Error = "Book_ErrorProductRequired";

            return;
        }

        if (Quantity < 1)
        {
            Error = "Book_ErrorQuantity";

            return;
        }

        if (Start.Value < Earliest)
        {
            Error = "Book_EarliestStart";
            ErrorArgument = Earliest.ToString("yyyy-MM-dd");

            return;
        }

        if (End.Value < Start.Value)
        {
            Error = "Book_ErrorEndBeforeStart";

            return;
        }

        if (End.Value.DayNumber - Start.Value.DayNumber + 1 > BookingRules.MaxDays)
        {
            Error = "Book_ErrorTooLong";
            ErrorArgument = BookingRules.MaxDays.ToString(System.Globalization.CultureInfo.InvariantCulture);

            return;
        }

        if (ExtraBatteries > 0 && !chosen.IsScooter)
        {
            Error = "Book_ErrorExtraOnlyScooters";

            return;
        }

        if (ExtraBatteries > Quantity)
        {
            Error = "Book_ErrorExtraAboveQuantity";

            return;
        }

        if (!TryReadPlace(out int zoneId))
        {
            Error = "Book_ErrorPlaceRequired";

            return;
        }

        int productId = await _db.Products
            .AsNoTracking()
            .Where(row => row.Slug == chosen.Slug)
            .Select(row => row.Id)
            .SingleAsync(cancellationToken);

        AvailabilityResult answer;

        try
        {
            answer = await _availability.ForProductAsync(
                productId, Start.Value, End.Value, Quantity, ExtraBatteries, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // A host without its settings row is a deployment defect, and the page fails closed:
            // it says it cannot check rather than quoting against an operation with no chargers.
            Error = "Book_ErrorUnavailableNow";

            return;
        }

        Answered = true;
        IsAvailable = answer.IsAvailable;
        MaxQuantity = answer.MaxQuantity;

        int quotedExtras = ExtraBatteries;

        if (!answer.IsAvailable && ExtraBatteries > 0)
        {
            // Asked again without the second battery, because that is the only question whose
            // answer decides this: comparing the quantity against a maximum would not distinguish
            // "the machine is free and the spare battery is not" from "the pool has no battery for
            // the machine either", and those two deserve different sentences.
            AvailabilityResult withoutSpare = await _availability.ForProductAsync(
                productId, Start.Value, End.Value, Quantity, 0, cancellationToken);

            if (withoutSpare.IsAvailable)
            {
                SecondBatteryUnavailable = true;
                IsAvailable = true;
                quotedExtras = 0;
            }
        }

        if (!IsAvailable)
        {
            return;
        }

        QuoteResult quote = await _quotes.BuildAsync(
            [new QuoteLineAsked(productId, Quantity, quotedExtras, DefaultExtraBatteryPerDay, Extras)],
            zoneId,
            Start.Value,
            End.Value,
            System.Globalization.CultureInfo.CurrentUICulture.Name,
            cancellationToken);

        if (quote.Breakdown is not QuoteBreakdown breakdown)
        {
            // Fails closed: a price list the catalog screen left invalid produces no number here
            // (D15). The visitor is told to write to us rather than shown a zero.
            Error = "Book_ErrorUnavailableNow";
            Answered = false;

            return;
        }

        Price = breakdown;
    }

    private bool TryReadPlace(out int zoneId)
    {
        zoneId = 0;

        CreateModel.PlaceChoice? choice = Places.SingleOrDefault(place => place.Value == Place);

        if (choice is null)
        {
            return false;
        }

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

        return true;
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        string culture = System.Globalization.CultureInfo.CurrentUICulture.Name;

        List<Domain.Product> products = await _db.Products
            .AsNoTracking()
            .Include(product => product.Translations)
            .Include(product => product.AddOns)
                .ThenInclude(link => link.AddOn!.Translations)
            .Where(product => product.IsActive && product.IsBookable)
            .OrderBy(product => product.SortOrder)
            .ToListAsync(cancellationToken);

        Products = products
            .Select(product => new ProductChoice(
                product.Slug,
                NameOf(product, culture),
                product.Category == ProductCategory.MobilityScooter))
            .ToList();

        Domain.Product? chosen = products.SingleOrDefault(product => product.Slug == Product);

        AddOns = chosen is null
            ? []
            : chosen.AddOns
                .Where(link => link.AddOn!.IsActive)
                .OrderBy(link => link.AddOn!.SortOrder)
                .Select(link => new AddOnChoice(link.AddOn!.Id, NameOf(link.AddOn!, culture)))
                .ToList();

        Places = await CreateModel.PlaceChoicesAsync(_db, culture, cancellationToken);

        // Read as a ROW and not as two projected values. Projecting an int through SingleOrDefault
        // answers zero for a missing row, and a cut-off of zero is not an absence — it is a claim
        // that the office stopped taking next-day work at midnight, which would quietly move every
        // visitor's earliest delivery day. The absence has to survive as an absence (D12/03).
        OperationalSettings? settings = await _db.OperationalSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        SettingsMissing = settings is null;

        if (settings is null)
        {
            return;
        }

        DefaultExtraBatteryPerDay = settings.SecondBatteryPerDay;
        Earliest = BookingRules.EarliestPublicStart(_clock.NowInOrlando(), settings.NextDayCutoffHour);
    }

    private static string NameOf(Domain.Product product, string culture)
    {
        ProductTranslation? text = TranslationPicker.For(product.Translations, culture, row => row.Culture);

        return text is null ? product.Slug : text.Name;
    }

    private static string NameOf(AddOn addOn, string culture)
    {
        AddOnTranslation? text = TranslationPicker.For(addOn.Translations, culture, row => row.Culture);

        return text is null ? addOn.Code : text.Name;
    }
}
