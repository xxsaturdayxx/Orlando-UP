using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Pages.Admin.Settings;

/// <summary>
/// The handful of numbers the operation changes without a deploy: how many chargers exist, what a
/// second battery is sold for by default, and what a lost charger costs.
/// </summary>
/// <remarks>
/// <b>This screen edits and does nothing else.</b> It never creates the row and never deletes it —
/// the migration made it, and control C03 of <c>fleet-batteries.tsv</c> states that as a
/// prohibition over this folder rather than as a habit. A database with no row is a state the
/// application cannot produce (control C09 of <c>foundation.tsv</c> keeps schema creation out of
/// start-up), so this page says so plainly instead of quietly inventing values nobody decided.
///
/// It is not the company settings, and that is D4/04b: those are bound from configuration in five
/// places and control C16 of <c>foundation.tsv</c> counts the four markers inside
/// <c>appsettings.json</c>. Moving them here is a front of its own.
/// </remarks>
public class IndexModel : PageModel
{
    private readonly CatalogWriter _writer;
    private readonly AuditTrail _audit;
    private readonly IStringLocalizer<SharedResource> _text;

    public IndexModel(CatalogWriter writer, AuditTrail audit, IStringLocalizer<SharedResource> text)
    {
        _writer = writer;
        _audit = audit;
        _text = text;
    }

    [BindProperty]
    public int ChargerCount { get; set; }

    [BindProperty]
    public decimal SecondBatteryPerDay { get; set; }

    [BindProperty]
    public decimal LostChargerFee { get; set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>False when the row is missing, which the screen reports and never repairs.</summary>
    public bool RowExists { get; private set; }

    public string? Problem { get; private set; }

    public bool Saved { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        OperationalSettings? settings = await _writer.FindSettingsAsync(cancellationToken);

        if (settings is null)
        {
            return;
        }

        ReadFrom(settings);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        OperationalSettings? settings = await _writer.FindSettingsAsync(cancellationToken);

        if (settings is null)
        {
            // Reported, never repaired: creating the row here is exactly what C03 forbids, and a
            // screen that silently made one would hide a database the migration never reached.
            Problem = _text["Admin_ErrorSettingsMissing"];

            return Page();
        }

        RowExists = true;

        string? refusal = FindRefusal();

        if (refusal is not null)
        {
            Problem = refusal;

            return Page();
        }

        settings.ChargerCount = ChargerCount;
        settings.SecondBatteryPerDay = SecondBatteryPerDay;
        settings.LostChargerFee = LostChargerFee;

        _writer.TouchSettings(settings);

        _audit.Record(
            User.Identity?.Name,
            nameof(OperationalSettings),
            settings.Id,
            AuditAction.Updated,
            $"Edited the operational settings: {ChargerCount} charger(s), " +
            $"second battery {SecondBatteryPerDay:0.00}/day, lost charger {LostChargerFee:0.00}.");

        await _writer.SaveAsync(cancellationToken);

        ReadFrom(settings);

        Saved = true;

        return Page();
    }

    private string? FindRefusal()
    {
        // A number the binder could not read is refused by name, never absorbed (D20).
        if (!ModelState.IsValid)
        {
            return _text["Admin_ErrorNumberFormat"];
        }

        if (ChargerCount < 0)
        {
            return _text["Admin_ErrorChargerCountNegative"];
        }

        // Zero is legitimate on both amounts and negative is not: a courtesy battery is zero, and
        // that is a decision Rod makes per reservation (D37).
        if (SecondBatteryPerDay < 0m || LostChargerFee < 0m)
        {
            return _text["Admin_ErrorAmountNegative"];
        }

        return null;
    }

    private void ReadFrom(OperationalSettings settings)
    {
        RowExists = true;
        ChargerCount = settings.ChargerCount;
        SecondBatteryPerDay = settings.SecondBatteryPerDay;
        LostChargerFee = settings.LostChargerFee;
        UpdatedAtUtc = settings.UpdatedAtUtc;
    }
}
