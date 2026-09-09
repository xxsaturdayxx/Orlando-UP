using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin.Units;

/// <summary>One physical machine joins the fleet.</summary>
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
    public string AssetTag { get; set; } = string.Empty;

    [BindProperty]
    public int ProductId { get; set; }

    [BindProperty]
    public UnitStatus Status { get; set; } = UnitStatus.Available;

    [BindProperty]
    public string? SerialNumber { get; set; }

    [BindProperty]
    public string? Notes { get; set; }

    /// <summary>A calendar date in Orlando, never an instant (D16).</summary>
    [BindProperty]
    public DateOnly? PurchasedOn { get; set; }

    public IReadOnlyList<Product> Products { get; private set; } = [];

    public string? Problem { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Products = await _writer.AllProductsAsync(cancellationToken);

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Products = await _writer.AllProductsAsync(cancellationToken);

        AssetTag = (AssetTag ?? string.Empty).Trim();

        if (AssetTag.Length == 0 || AssetTag.Length > 40)
        {
            Problem = _text["Admin_ErrorAssetTagRequired"];

            return Page();
        }

        if (!Products.Any(product => product.Id == ProductId))
        {
            Problem = _text["Admin_ErrorProductRequired"];

            return Page();
        }

        if (await _writer.AssetTagIsTakenAsync(AssetTag, exceptUnitId: 0, cancellationToken))
        {
            Problem = _text["Admin_ErrorAssetTagTaken"];

            return Page();
        }

        await using IDbContextTransaction transaction = await _writer.BeginAsync(cancellationToken);

        Unit unit = _writer.AddUnit(new Unit
        {
            AssetTag = AssetTag,
            ProductId = ProductId,
            Status = Status,
            SerialNumber = string.IsNullOrWhiteSpace(SerialNumber) ? null : SerialNumber.Trim(),
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            PurchasedOn = PurchasedOn,
        });

        try
        {
            // Checked above and still caught here: two operators can ask for the same tag between
            // the check and the write, and the unique index is the only thing that cannot race.
            await _writer.SaveAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            Problem = _text["Admin_ErrorAssetTagTaken"];

            return Page();
        }

        _audit.Record(
            User.Identity?.Name,
            nameof(Unit),
            unit.Id,
            AuditAction.Created,
            $"Created the unit {AssetTag} on the product " +
            $"{Products.First(product => product.Id == ProductId).Slug}.");

        await _writer.SaveAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return RedirectToPage("/Admin/Units/Index");
    }
}
