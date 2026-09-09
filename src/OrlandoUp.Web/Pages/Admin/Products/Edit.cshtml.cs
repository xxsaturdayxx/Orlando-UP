using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using OrlandoUp.Application;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin.Products;

/// <summary>
/// The product editor: one form, four blocks — the product, the two translations, the price bands,
/// the add-on links.
/// </summary>
/// <remarks>
/// Absence is a first-class value here. A dimension nobody measured is an empty field, is saved as
/// <c>NULL</c>, and comes back empty; it is never shown as <c>0</c> and never saved as <c>0</c>
/// (D15, control C17).
///
/// A refusal saves nothing and returns the whole form, typing included, with the reason named (K3).
/// Razor re-renders from the bound properties, so nothing the operator wrote is lost — and the
/// alternative, saving everything except the flag, can leave somebody believing a product is on
/// sale when the flag was quietly refused.
/// </remarks>
public class EditModel : PageModel
{
    /// <summary>Blank rows the form offers beyond the bands that already exist.</summary>
    private const int SpareTierRows = 3;

    private readonly CatalogWriter _writer;
    private readonly AuditTrail _audit;
    private readonly IStringLocalizer<SharedResource> _text;

    public EditModel(CatalogWriter writer, AuditTrail audit, IStringLocalizer<SharedResource> text)
    {
        _writer = writer;
        _audit = audit;
        _text = text;
    }

    public sealed class TierRow
    {
        public int? MinDays { get; set; }

        /// <summary>Empty means open-ended, which is a value and not a missing one.</summary>
        public int? MaxDays { get; set; }

        public TierMode Mode { get; set; } = TierMode.PerDay;

        public decimal? Amount { get; set; }

        public bool IsBlank => MinDays is null && MaxDays is null && Amount is null;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public string Slug { get; set; } = string.Empty;

    [BindProperty]
    public ProductCategory Category { get; set; }

    [BindProperty]
    public SeatConfiguration? Configuration { get; set; }

    [BindProperty]
    public int? MaxRiderWeightLb { get; set; }

    [BindProperty]
    public decimal? WidthIn { get; set; }

    [BindProperty]
    public decimal? LengthIn { get; set; }

    [BindProperty]
    public decimal? SeatWidthIn { get; set; }

    [BindProperty]
    public decimal? RangeMiles { get; set; }

    [BindProperty]
    public int TurnaroundDays { get; set; }

    [BindProperty]
    public int SortOrder { get; set; }

    [BindProperty]
    public string? ImagePath { get; set; }

    [BindProperty]
    public bool IsActive { get; set; }

    [BindProperty]
    public bool IsBookable { get; set; }

    [BindProperty]
    public string EnglishName { get; set; } = string.Empty;

    [BindProperty]
    public string? EnglishTagline { get; set; }

    [BindProperty]
    public string? EnglishDescription { get; set; }

    [BindProperty]
    public string? EnglishHighlights { get; set; }

    [BindProperty]
    public string? PortugueseName { get; set; }

    [BindProperty]
    public string? PortugueseTagline { get; set; }

    [BindProperty]
    public string? PortugueseDescription { get; set; }

    [BindProperty]
    public string? PortugueseHighlights { get; set; }

    [BindProperty]
    public List<TierRow> Tiers { get; set; } = [];

    [BindProperty]
    public List<int> AddOnIds { get; set; } = [];

    public IReadOnlyList<AddOn> AvailableAddOns { get; private set; } = [];

    public string? Problem { get; private set; }

    public bool Saved { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Product? product = await _writer.FindProductAsync(Id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        ReadFrom(product);

        AvailableAddOns = await _writer.ActiveAddOnsAsync(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Product? product = await _writer.FindProductAsync(Id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        AvailableAddOns = await _writer.ActiveAddOnsAsync(cancellationToken);

        Slug = (Slug ?? string.Empty).Trim();
        EnglishName = (EnglishName ?? string.Empty).Trim();

        string? refusal = await FindRefusalAsync(cancellationToken);

        if (refusal is not null)
        {
            Problem = refusal;

            return Page();
        }

        product.Slug = Slug;
        product.Category = Category;
        product.Configuration = Category == ProductCategory.Stroller ? Configuration : null;
        product.MaxRiderWeightLb = MaxRiderWeightLb;
        product.WidthIn = WidthIn;
        product.LengthIn = LengthIn;
        product.SeatWidthIn = SeatWidthIn;
        product.RangeMiles = RangeMiles;
        product.TurnaroundDays = TurnaroundDays;
        product.SortOrder = SortOrder;
        product.ImagePath = string.IsNullOrWhiteSpace(ImagePath) ? null : ImagePath.Trim();
        product.IsActive = IsActive;
        product.IsBookable = IsBookable;

        _writer.ApplyTranslation(
            product,
            SiteCultures.English,
            EnglishName,
            EnglishTagline,
            EnglishDescription,
            CatalogWriter.SplitLines(EnglishHighlights));

        _writer.ApplyTranslation(
            product,
            SiteCultures.Portuguese,
            PortugueseName,
            PortugueseTagline,
            PortugueseDescription,
            CatalogWriter.SplitLines(PortugueseHighlights));

        _writer.ReplaceTiers(product, ProposedTiers());
        _writer.ReplaceAddOnLinks(product, AddOnIds);
        _writer.TouchProduct(product);

        _audit.Record(
            User.Identity?.Name,
            nameof(Product),
            product.Id,
            IsActive ? AuditAction.Updated : AuditAction.Deactivated,
            $"Edited the product {product.Slug}: {(IsActive ? "visible" : "hidden")}, " +
            $"{(IsBookable ? "on sale" : "not on sale")}, " +
            $"{product.PricingTiers.Count} price band(s), {AddOnIds.Count} add-on link(s).");

        await _writer.SaveAsync(cancellationToken);

        ReadFrom(product);

        Saved = true;

        return Page();
    }

    /// <summary>
    /// The first reason this form cannot be saved, or <c>null</c>. Every refusal is a resource key
    /// present in both cultures, and each names what is wrong rather than that something is.
    /// </summary>
    private async Task<string?> FindRefusalAsync(CancellationToken cancellationToken)
    {
        if (EnglishName.Length == 0)
        {
            return _text["Admin_ErrorEnglishNameRequired"];
        }

        if (Slug.Length == 0 || Slug.Length > 80)
        {
            return _text["Admin_ErrorSlugRequired"];
        }

        if (await _writer.SlugIsTakenAsync(Slug, exceptProductId: Id, cancellationToken))
        {
            return _text["Admin_ErrorSlugTaken"];
        }

        // A number the binder could not read is refused, never absorbed: the field would otherwise
        // arrive null and a measured dimension would silently become an unmeasured one, which is
        // the shape of D20's warning about a comma meeting a decimal. It is checked AFTER the two
        // text fields on purpose — both are non-nullable strings, so the framework's implicit
        // Required also lands in ModelState, and this generic message would otherwise answer for a
        // blank name and say nothing true about it.
        if (!ModelState.IsValid)
        {
            return _text["Admin_ErrorNumberFormat"];
        }

        IReadOnlyList<TierRow> filled = Tiers.Where(row => !row.IsBlank).ToList();

        foreach (TierRow row in filled)
        {
            // A row somebody started and did not finish is an invalid band, not a reason to invent
            // a number for the half that is missing.
            if (row.MinDays is null || row.Amount is null)
            {
                return TierProblemText(PricingTierSetProblem.InvalidBand);
            }
        }

        if (IsBookable)
        {
            PricingTierSetProblem problem = PricingTierRules.Validate(ProposedTiers());

            if (problem != PricingTierSetProblem.None)
            {
                return TierProblemText(problem);
            }
        }
        else
        {
            // The other half of the seeder's guard: a price nobody can pay is worse than no price,
            // because it is the number a visitor remembers and quotes back on the phone.
            if (filled.Count > 0)
            {
                return _text["Admin_ErrorNotBookableCarriesPrice"];
            }

            if (AddOnIds.Count > 0)
            {
                return _text["Admin_ErrorNotBookableCarriesAddOn"];
            }
        }

        return null;
    }

    private string TierProblemText(PricingTierSetProblem problem) => problem switch
    {
        PricingTierSetProblem.Empty => _text["Admin_ErrorTierEmpty"],
        PricingTierSetProblem.InvalidBand => _text["Admin_ErrorTierInvalidBand"],
        PricingTierSetProblem.DoesNotStartAtOneDay => _text["Admin_ErrorTierDoesNotStartAtOneDay"],
        PricingTierSetProblem.Overlap => _text["Admin_ErrorTierOverlap"],
        PricingTierSetProblem.Gap => _text["Admin_ErrorTierGap"],
        PricingTierSetProblem.NoOpenEndedBand => _text["Admin_ErrorTierNoOpenEndedBand"],
        _ => _text["Admin_ErrorTierInvalidBand"],
    };

    private List<PricingTier> ProposedTiers()
    {
        List<PricingTier> proposed = [];

        foreach (TierRow row in Tiers.Where(row => !row.IsBlank))
        {
            if (row.MinDays is not int min || row.Amount is not decimal amount)
            {
                continue;
            }

            proposed.Add(new PricingTier
            {
                MinDays = min,
                MaxDays = row.MaxDays,
                Mode = row.Mode,
                Amount = amount,
            });
        }

        return proposed;
    }

    private void ReadFrom(Product product)
    {
        Id = product.Id;
        Slug = product.Slug;
        Category = product.Category;
        Configuration = product.Configuration;
        MaxRiderWeightLb = product.MaxRiderWeightLb;
        WidthIn = product.WidthIn;
        LengthIn = product.LengthIn;
        SeatWidthIn = product.SeatWidthIn;
        RangeMiles = product.RangeMiles;
        TurnaroundDays = product.TurnaroundDays;
        SortOrder = product.SortOrder;
        ImagePath = product.ImagePath;
        IsActive = product.IsActive;
        IsBookable = product.IsBookable;

        ProductTranslation? english = product.Translations
            .FirstOrDefault(text => text.Culture == SiteCultures.English);
        ProductTranslation? portuguese = product.Translations
            .FirstOrDefault(text => text.Culture == SiteCultures.Portuguese);

        EnglishName = english?.Name ?? string.Empty;
        EnglishTagline = english?.Tagline;
        EnglishDescription = english?.Description;
        EnglishHighlights = LinesOf(english);

        PortugueseName = portuguese?.Name;
        PortugueseTagline = portuguese?.Tagline;
        PortugueseDescription = portuguese?.Description;
        PortugueseHighlights = LinesOf(portuguese);

        Tiers = product.PricingTiers
            .OrderBy(tier => tier.MinDays)
            .Select(tier => new TierRow
            {
                MinDays = tier.MinDays,
                MaxDays = tier.MaxDays,
                Mode = tier.Mode,
                Amount = tier.Amount,
            })
            .ToList();

        for (int spare = 0; spare < SpareTierRows; spare++)
        {
            Tiers.Add(new TierRow());
        }

        AddOnIds = product.AddOns.Select(link => link.AddOnId).ToList();
    }

    private static string LinesOf(ProductTranslation? translation) =>
        translation is null
            ? string.Empty
            : string.Join("\n", CatalogWriter.ReadHighlights(translation.Highlights));
}
