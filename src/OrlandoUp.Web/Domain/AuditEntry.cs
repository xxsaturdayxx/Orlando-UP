namespace OrlandoUp.Domain;

/// <summary>One line of the administration's record: who changed what, when, and to what.</summary>
/// <remarks>
/// A plain row with no navigation property and no foreign key to the Identity tables, and both
/// absences are deliberate. The record must survive the account that wrote it — a member of staff
/// can be renamed or removed and the line must still say who acted — so the actor is copied as
/// text, not linked. And <c>ArchitectureTests</c> forbids <c>OrlandoUp.Domain</c> from depending on
/// any other layer, which a link to Identity would be.
/// </remarks>
public class AuditEntry
{
    public int Id { get; set; }

    /// <summary>An instant, not a calendar date. Read from <c>IClock</c> (D16).</summary>
    public DateTime OccurredAtUtc { get; set; }

    /// <summary>The signed-in user's name at the moment of the write, copied as text.</summary>
    public string ActorEmail { get; set; } = string.Empty;

    /// <summary>The domain type that changed, written from <c>nameof</c> and never typed out.</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The key of the row that changed. For the link table, whose key is composite, it is the
    /// product's key and the summary names the other side.
    /// </summary>
    public int EntityId { get; set; }

    public AuditAction Action { get; set; }

    /// <summary>One sentence in English for a human to read. Never a serialized diff.</summary>
    public string Summary { get; set; } = string.Empty;
}
