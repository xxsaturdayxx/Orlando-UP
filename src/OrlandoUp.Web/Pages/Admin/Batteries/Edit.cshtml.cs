using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin.Batteries;

/// <summary>
/// One battery, edited. Retiring one is the case this screen exists for: a customer broke one, and
/// the fleet has to stop counting it without losing that it existed (D5/04b).
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
    public BatteryKind Kind { get; set; }

    [BindProperty]
    public decimal? RangeMiles { get; set; }

    [BindProperty]
    public UnitStatus Status { get; set; }

    [BindProperty]
    public string? SerialNumber { get; set; }

    [BindProperty]
    public string? Notes { get; set; }

    [BindProperty]
    public DateOnly? PurchasedOn { get; set; }

    public IReadOnlyList<Product> Scooters { get; private set; } = [];

    public string? Problem { get; private set; }

    public bool Saved { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Battery? battery = await _writer.FindBatteryAsync(Id, cancellationToken);

        if (battery is null)
        {
            return NotFound();
        }

        Scooters = await _writer.ScooterModelsAsync(cancellationToken);

        ReadFrom(battery);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Battery? battery = await _writer.FindBatteryAsync(Id, cancellationToken);

        if (battery is null)
        {
            return NotFound();
        }

        Scooters = await _writer.ScooterModelsAsync(cancellationToken);

        AssetTag = (AssetTag ?? string.Empty).Trim();

        UnitStatus leftStatus = battery.Status;

        string? refusal = await FindRefusalAsync(cancellationToken);

        if (refusal is not null)
        {
            Problem = refusal;

            return Page();
        }

        battery.AssetTag = AssetTag;
        battery.ProductId = ProductId;
        battery.Kind = Kind;
        battery.RangeMiles = RangeMiles;
        battery.Status = Status;
        battery.SerialNumber = string.IsNullOrWhiteSpace(SerialNumber) ? null : SerialNumber.Trim();
        battery.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim();
        battery.PurchasedOn = PurchasedOn;

        _audit.Record(
            User.Identity?.Name,
            nameof(Battery),
            battery.Id,
            Status == UnitStatus.Retired ? AuditAction.Deactivated
                : leftStatus == UnitStatus.Retired ? AuditAction.Reactivated
                : AuditAction.Updated,
            $"Edited the battery {AssetTag}: {SlugOf(ProductId)}, kind {Kind}, status {Status}.");

        try
        {
            await _writer.SaveAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            Problem = _text["Admin_ErrorBatteryTagTaken"];

            return Page();
        }

        Saved = true;

        return Page();
    }

    private async Task<string?> FindRefusalAsync(CancellationToken cancellationToken)
    {
        if (AssetTag.Length == 0 || AssetTag.Length > 40)
        {
            return _text["Admin_ErrorBatteryTagRequired"];
        }

        if (!Scooters.Any(product => product.Id == ProductId))
        {
            return _text["Admin_ErrorBatteryNeedsScooter"];
        }

        if (await _writer.BatteryTagIsTakenAsync(AssetTag, exceptBatteryId: Id, cancellationToken))
        {
            return _text["Admin_ErrorBatteryTagTaken"];
        }

        if (!ModelState.IsValid)
        {
            return _text["Admin_ErrorNumberFormat"];
        }

        return null;
    }

    private string SlugOf(int productId) =>
        Scooters.FirstOrDefault(product => product.Id == productId)?.Slug ?? "?";

    private void ReadFrom(Battery battery)
    {
        Id = battery.Id;
        AssetTag = battery.AssetTag;
        ProductId = battery.ProductId;
        Kind = battery.Kind;
        RangeMiles = battery.RangeMiles;
        Status = battery.Status;
        SerialNumber = battery.SerialNumber;
        Notes = battery.Notes;
        PurchasedOn = battery.PurchasedOn;
    }
}
