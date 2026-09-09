using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrlandoUp.Application;
using OrlandoUp.Application.Catalog;
using OrlandoUp.Domain;

namespace OrlandoUp.Infrastructure.Data;

/// <summary>
/// Everything the administration writes into the catalog. The mirror of
/// <see cref="CatalogQueries"/>, and registered the same way: a scoped class holding the context,
/// never a static — a static holding a <c>DbContext</c> would be holding somebody else's request.
/// </summary>
/// <remarks>
/// It lives in <c>Infrastructure/Data/</c> and not in <c>Application/</c> because it knows the
/// context, and <c>ArchitectureTests.The_application_layer_knows_nothing_about_infrastructure</c>
/// is what keeps that honest.
/// </remarks>
public sealed class CatalogWriter
{
    /// <summary>
    /// The same relaxed encoder the seeder writes highlights with. Two writers of one column that
    /// escaped differently would make the column's contents depend on who last touched the row.
    /// It is safe here for a measured reason and not by luck: highlights never reach the JSON-LD
    /// block, whose own encoder forbids the three characters that matter there.
    /// </summary>
    private static readonly JsonSerializerOptions HighlightsJson = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly AppDbContext _db;
    private readonly IClock _clock;

    public CatalogWriter(AppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    // -----------------------------------------------------------------------------------------
    // Reading, for the screens that then write
    // -----------------------------------------------------------------------------------------

    /// <summary>The product with everything the editor shows, tracked so a save can change it.</summary>
    public Task<Product?> FindProductAsync(int id, CancellationToken cancellation) =>
        _db.Products
            .Include(product => product.Translations)
            .Include(product => product.PricingTiers)
            .Include(product => product.AddOns)
            .FirstOrDefaultAsync(product => product.Id == id, cancellation);

    public Task<Unit?> FindUnitAsync(int id, CancellationToken cancellation) =>
        _db.Units.FirstOrDefaultAsync(unit => unit.Id == id, cancellation);

    /// <summary>
    /// One add-on as the editor's checkbox list needs it: the key to post back, and the name a
    /// person reads.
    /// </summary>
    /// <remarks>
    /// It is declared here and not in <c>Application/Catalog/CatalogViews.cs</c> on purpose
    /// (D13/04): that file's records are built positionally by tests — <c>ProductDetail</c> with
    /// seventeen arguments — and a new member there costs every one of those call sites.
    /// </remarks>
    public sealed record AddOnChoice(int Id, string Label);

    /// <summary>
    /// Every add-on the editor can link to, active ones only, in display order, each labelled in
    /// the culture being served.
    /// </summary>
    /// <remarks>
    /// The label goes through <see cref="TranslationPicker"/>, which answers the requested culture,
    /// else English, else nothing. When it answers nothing the label falls back to
    /// <see cref="AddOn.Code"/> — an add-on nobody has translated is still an add-on the operator
    /// must be able to tick, and a blank label would be a checkbox with no meaning at all. Showing
    /// the code as the ordinary case is the defect this method exists to have stopped doing: it is
    /// the identifier <c>Domain/AddOn.cs</c> itself calls "never shown to the customer".
    /// </remarks>
    public async Task<IReadOnlyList<AddOnChoice>> ActiveAddOnsAsync(string culture, CancellationToken cancellation)
    {
        List<AddOn> addOns = await _db.AddOns
            .AsNoTracking()
            .Include(addOn => addOn.Translations)
            .Where(addOn => addOn.IsActive)
            .OrderBy(addOn => addOn.SortOrder)
            .ToListAsync(cancellation);

        return addOns
            .Select(addOn => new AddOnChoice(
                addOn.Id,
                TranslationPicker.For(addOn.Translations, culture, text => text.Culture)?.Name ?? addOn.Code))
            .ToList();
    }

    /// <summary>Every product, for the picker on the unit screen.</summary>
    public async Task<IReadOnlyList<Product>> AllProductsAsync(CancellationToken cancellation) =>
        await _db.Products
            .AsNoTracking()
            .OrderBy(product => product.SortOrder)
            .ToListAsync(cancellation);

    public Task<bool> SlugIsTakenAsync(string slug, int exceptProductId, CancellationToken cancellation) =>
        _db.Products.AnyAsync(product => product.Slug == slug && product.Id != exceptProductId, cancellation);

    public Task<bool> AssetTagIsTakenAsync(string tag, int exceptUnitId, CancellationToken cancellation) =>
        _db.Units.AnyAsync(unit => unit.AssetTag == tag && unit.Id != exceptUnitId, cancellation);

    // -----------------------------------------------------------------------------------------
    // Writing
    // -----------------------------------------------------------------------------------------

    /// <summary>
    /// Stages a new product. It is born <b>hidden</b>, and that is a deliberate override of the
    /// <c>= true</c> initialiser on the domain class: the initialiser is right for a row the seeder
    /// writes whole, and fail-open for a row a half-filled form writes. Publishing is a second,
    /// deliberate gesture on the edit screen.
    /// </summary>
    public Product AddProduct(string slug, ProductCategory category, string englishName, int sortOrder)
    {
        Product product = new()
        {
            Slug = slug,
            Category = category,
            SortOrder = sortOrder,
            IsActive = false,
            CreatedAtUtc = _clock.UtcNow,
        };

        product.Translations.Add(new ProductTranslation
        {
            Culture = SiteCultures.English,
            Name = englishName,
            Description = string.Empty,
            Highlights = "[]",
        });

        _db.Products.Add(product);

        return product;
    }

    public Unit AddUnit(Unit unit)
    {
        unit.CreatedAtUtc = _clock.UtcNow;

        _db.Units.Add(unit);

        return unit;
    }

    /// <summary>Stamps the moment of the last edit. Nothing else in the project writes it.</summary>
    public void TouchProduct(Product product) => product.UpdatedAtUtc = _clock.UtcNow;

    /// <summary>
    /// Writes one culture's text, or removes the row when every field of it is blank. Storing a row
    /// of empty strings would make the list screen say a translation exists when nobody wrote one.
    /// </summary>
    public void ApplyTranslation(
        Product product,
        string culture,
        string? name,
        string? tagline,
        string? description,
        IReadOnlyList<string> highlights)
    {
        ProductTranslation? existing = product.Translations
            .FirstOrDefault(text => text.Culture == culture);

        bool blank = string.IsNullOrWhiteSpace(name)
            && string.IsNullOrWhiteSpace(tagline)
            && string.IsNullOrWhiteSpace(description)
            && highlights.Count == 0;

        if (blank)
        {
            if (existing is not null)
            {
                product.Translations.Remove(existing);
                _db.ProductTranslations.Remove(existing);
            }

            return;
        }

        ProductTranslation row = existing ?? new ProductTranslation { Culture = culture };

        row.Name = name ?? string.Empty;
        row.Tagline = string.IsNullOrWhiteSpace(tagline) ? null : tagline;
        row.Description = description ?? string.Empty;
        row.Highlights = WriteHighlights(highlights);

        if (existing is null)
        {
            product.Translations.Add(row);
        }
    }

    /// <summary>Replaces the whole price list. The rule that judges it lives in the domain.</summary>
    public void ReplaceTiers(Product product, IReadOnlyList<PricingTier> tiers)
    {
        foreach (PricingTier gone in product.PricingTiers.ToList())
        {
            product.PricingTiers.Remove(gone);
            _db.PricingTiers.Remove(gone);
        }

        foreach (PricingTier tier in tiers)
        {
            product.PricingTiers.Add(tier);
        }
    }

    /// <summary>Replaces the add-on links. The composite key makes a duplicate impossible.</summary>
    public void ReplaceAddOnLinks(Product product, IReadOnlyList<int> addOnIds)
    {
        foreach (ProductAddOn gone in product.AddOns.ToList())
        {
            product.AddOns.Remove(gone);
            _db.ProductAddOns.Remove(gone);
        }

        foreach (int addOnId in addOnIds.Distinct())
        {
            product.AddOns.Add(new ProductAddOn { AddOnId = addOnId });
        }
    }

    public Task<int> SaveAsync(CancellationToken cancellation) => _db.SaveChangesAsync(cancellation);

    /// <summary>
    /// One transaction around the two saves a creation needs. A new row has no key until it is
    /// written, and the audit line has to name the key — so the row is saved, then the line is
    /// staged with the key it now has, then both are committed together. Without this, a failure
    /// between the two would leave a product nothing in the record accounts for.
    /// </summary>
    public Task<IDbContextTransaction> BeginAsync(CancellationToken cancellation) =>
        _db.Database.BeginTransactionAsync(cancellation);

    // -----------------------------------------------------------------------------------------
    // Highlights, which travel as lines on the screen and as JSON in the column
    // -----------------------------------------------------------------------------------------

    /// <summary>One highlight per line; blank lines are not highlights.</summary>
    public static IReadOnlyList<string> SplitLines(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? []
            : text.ReplaceLineEndings("\n")
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .ToList();

    public static string WriteHighlights(IReadOnlyList<string> highlights) =>
        JsonSerializer.Serialize(highlights, HighlightsJson);

    /// <summary>
    /// The column back as lines for the textarea. A malformed column comes back as no lines rather
    /// than as an exception: it is a content defect, and the editor is where it gets fixed.
    /// </summary>
    public static IReadOnlyList<string> ReadHighlights(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
