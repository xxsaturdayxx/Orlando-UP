using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin.Units;

/// <summary>
/// One unit, edited. The product it belongs to is an editable field (K1 a), and when it changes the
/// audit line names the product the unit left and the one it joined — a unit that moves takes its
/// history with it, so the record has to say where it went.
/// </summary>
public class EditModel : PageModel
{
    private readonly CatalogWriter _writer;
    private readonly AuditTrail _audit;
    private readonly IStringLocalizer<SharedResource> _text;

    public EditModel(CatalogWriter writer, AuditTrail audit, IStringLocalizer<SharedResource> text)
    {
        _writer = writer;
        _audit = audit;
        _text = text;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public string AssetTag { get; set; } = string.Empty;

    [BindProperty]
    public int ProductId { get; set; }

    [BindProperty]
    public UnitStatus Status { get; set; }

    [BindProperty]
    public string? SerialNumber { get; set; }

    [BindProperty]
    public string? Notes { get; set; }

    [BindProperty]
    public DateOnly? PurchasedOn { get; set; }

    public IReadOnlyList<Product> Products { get; private set; } = [];

    public string? Problem { get; private set; }

    public bool Saved { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Unit? unit = await _writer.FindUnitAsync(Id, cancellationToken);

        if (unit is null)
        {
            return NotFound();
        }

        Products = await _writer.AllProductsAsync(cancellationToken);

        ReadFrom(unit);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Unit? unit = await _writer.FindUnitAsync(Id, cancellationToken);

        if (unit is null)
        {
            return NotFound();
        }

        Products = await _writer.AllProductsAsync(cancellationToken);

        AssetTag = (AssetTag ?? string.Empty).Trim();

        int leftProductId = unit.ProductId;

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

        if (await _writer.AssetTagIsTakenAsync(AssetTag, exceptUnitId: Id, cancellationToken))
        {
            Problem = _text["Admin_ErrorAssetTagTaken"];

            return Page();
        }

        unit.AssetTag = AssetTag;
        unit.ProductId = ProductId;
        unit.Status = Status;
        unit.SerialNumber = string.IsNullOrWhiteSpace(SerialNumber) ? null : SerialNumber.Trim();
        unit.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim();
        unit.PurchasedOn = PurchasedOn;

        _audit.Record(
            User.Identity?.Name,
            nameof(Unit),
            unit.Id,
            Status == UnitStatus.Retired ? AuditAction.Deactivated : AuditAction.Updated,
            leftProductId == ProductId
                ? $"Edited the unit {AssetTag} on the product {SlugOf(ProductId)}, status {Status}."
                : $"Edited the unit {AssetTag}: moved from the product {SlugOf(leftProductId)} " +
                  $"to {SlugOf(ProductId)}, status {Status}.");

        try
        {
            await _writer.SaveAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            Problem = _text["Admin_ErrorAssetTagTaken"];

            return Page();
        }

        Saved = true;

        return Page();
    }

    private string SlugOf(int productId) =>
        Products.FirstOrDefault(product => product.Id == productId)?.Slug ?? "?";

    private void ReadFrom(Unit unit)
    {
        Id = unit.Id;
        AssetTag = unit.AssetTag;
        ProductId = unit.ProductId;
        Status = unit.Status;
        SerialNumber = unit.SerialNumber;
        Notes = unit.Notes;
        PurchasedOn = unit.PurchasedOn;
    }
}
