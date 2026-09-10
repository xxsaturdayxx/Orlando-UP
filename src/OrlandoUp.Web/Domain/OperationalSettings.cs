namespace OrlandoUp.Domain;

/// <summary>
/// The handful of numbers the administration changes without a deploy. One row, always id
/// <see cref="SingletonId"/> — a settings sheet, not a list.
/// </summary>
/// <remarks>
/// It is deliberately NOT the company settings: those are bound from configuration through
/// <c>IOptions</c> in five places, and control C16 of <c>foundation.tsv</c> counts the four
/// <c>TODO-</c> markers inside the committed <c>appsettings.json</c>. Moving them into the database
/// would retire that control and rewire those five call sites for the sake of two numbers, and that
/// is a front of its own (D4/04b).
///
/// The row is created by the migration. The screen edits it; it never creates and never deletes,
/// which control C03 of <c>fleet-batteries.tsv</c> states as a prohibition over the pages folder.
/// </remarks>
public class OperationalSettings
{
    /// <summary>The only key this table ever has.</summary>
    public const int SingletonId = 1;

    public int Id { get; set; }

    /// <summary>
    /// How many chargers exist. A count and not a table, because any charger fits any battery of
    /// either model and they carry no identity of their own (D3/04b). It is independent of the
    /// battery count and is not derived from it, which is why it is stored.
    /// </summary>
    public int ChargerCount { get; set; }

    /// <summary>What a second battery is sold for per day, by default. Zero is legitimate.</summary>
    public decimal SecondBatteryPerDay { get; set; }

    /// <summary>What a lost charger costs to replace.</summary>
    public decimal LostChargerFee { get; set; }

    /// <summary>An instant, from <c>IClock</c>, so an edit is dated.</summary>
    public DateTime? UpdatedAtUtc { get; set; }
}
