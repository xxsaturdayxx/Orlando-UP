using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin.Products;

/// <summary>
/// Asks only what a product needs in order to exist, then hands over to the editor of the product
/// it just made.
/// </summary>
/// <remarks>
/// The four blocks of the editor — the product, the two translations, the price bands, the add-on
/// links — are all edited against a row that exists, instead of being assembled in the air against
/// one that does not. The product is born hidden; publishing it is a second gesture, taken on the
/// editor with the whole thing in front of you.
/// </remarks>
public class CreateModel : PageModel
{
    private readonly CatalogWriter _writer;
    private readonly AuditTrail _audit;
    private readonly IStringLocalizer<SharedResource> _text;

    public CreateModel(CatalogWriter writer, AuditTrail audit, IStringLocalizer<SharedResource> text)
    {
        _writer = writer;
        _audit = audit;
        _text = text;
    }

    [BindProperty]
    public string Slug { get; set; } = string.Empty;

    [BindProperty]
    public ProductCategory Category { get; set; } = ProductCategory.MobilityScooter;

    [BindProperty]
    public string EnglishName { get; set; } = string.Empty;

    [BindProperty]
    public int SortOrder { get; set; }

    public string? Problem { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Slug = (Slug ?? string.Empty).Trim();
        EnglishName = (EnglishName ?? string.Empty).Trim();

        if (EnglishName.Length == 0)
        {
            Problem = _text["Admin_ErrorEnglishNameRequired"];

            return Page();
        }

        if (Slug.Length == 0 || Slug.Length > 80)
        {
            Problem = _text["Admin_ErrorSlugRequired"];

            return Page();
        }

        if (await _writer.SlugIsTakenAsync(Slug, exceptProductId: 0, cancellationToken))
        {
            Problem = _text["Admin_ErrorSlugTaken"];

            return Page();
        }

        await using IDbContextTransaction transaction = await _writer.BeginAsync(cancellationToken);

        Product product = _writer.AddProduct(Slug, Category, EnglishName, SortOrder);

        // Saved first, because the audit line names the key and a new row has none until it is
        // written. The transaction is what keeps the pair from coming apart.
        await _writer.SaveAsync(cancellationToken);

        _audit.Record(
            User.Identity?.Name,
            nameof(Product),
            product.Id,
            AuditAction.Created,
            $"Created the product {Slug}, hidden, named \"{EnglishName}\" in English.");

        await _writer.SaveAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return RedirectToPage("/Admin/Products/Edit", new { id = product.Id });
    }
}
