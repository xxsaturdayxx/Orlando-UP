using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin.Batteries;

/// <summary>One battery joins the fleet.</summary>
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
    public BatteryKind Kind { get; set; } = BatteryKind.Normal;

    /// <summary>Absent stays absent: an unmeasured battery says nothing, never zero (D15).</summary>
    [BindProperty]
    public decimal? RangeMiles { get; set; }

    [BindProperty]
    public UnitStatus Status { get; set; } = UnitStatus.Available;

    [BindProperty]
    public string? SerialNumber { get; set; }

    [BindProperty]
    public string? Notes { get; set; }

    /// <summary>A calendar date in Orlando, never an instant (D16).</summary>
    [BindProperty]
    public DateOnly? PurchasedOn { get; set; }

    public IReadOnlyList<Product> Scooters { get; private set; } = [];

    public string? Problem { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Scooters = await _writer.ScooterModelsAsync(cancellationToken);

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Scooters = await _writer.ScooterModelsAsync(cancellationToken);

        AssetTag = (AssetTag ?? string.Empty).Trim();

        string? refusal = await FindRefusalAsync(cancellationToken);

        if (refusal is not null)
        {
            Problem = refusal;

            return Page();
        }

        await using IDbContextTransaction transaction = await _writer.BeginAsync(cancellationToken);

        Battery battery = _writer.AddBattery(new Battery
        {
            AssetTag = AssetTag,
            ProductId = ProductId,
            Kind = Kind,
            RangeMiles = RangeMiles,
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
            Problem = _text["Admin_ErrorBatteryTagTaken"];

            return Page();
        }

        _audit.Record(
            User.Identity?.Name,
            nameof(Battery),
            battery.Id,
            AuditAction.Created,
            $"Created the battery {AssetTag} on the model " +
            $"{Scooters.First(product => product.Id == ProductId).Slug}, kind {Kind}.");

        await _writer.SaveAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return RedirectToPage("/Admin/Batteries/Index");
    }

    private async Task<string?> FindRefusalAsync(CancellationToken cancellationToken)
    {
        if (AssetTag.Length == 0 || AssetTag.Length > 40)
        {
            return _text["Admin_ErrorBatteryTagRequired"];
        }

        // A battery belongs to a SCOOTER model (D37). The list this screen offers holds only
        // scooters, so a product that is not in it is either a wheelchair, a stroller, or nothing.
        if (!Scooters.Any(product => product.Id == ProductId))
        {
            return _text["Admin_ErrorBatteryNeedsScooter"];
        }

        if (await _writer.BatteryTagIsTakenAsync(AssetTag, exceptBatteryId: 0, cancellationToken))
        {
            return _text["Admin_ErrorBatteryTagTaken"];
        }

        // Checked after the fields that carry their own message, because AssetTag is a
        // non-nullable string and the framework's implicit Required also lands in ModelState —
        // this generic message would otherwise answer for a blank tag and say nothing true.
        if (!ModelState.IsValid)
        {
            return _text["Admin_ErrorNumberFormat"];
        }

        return null;
    }
}
