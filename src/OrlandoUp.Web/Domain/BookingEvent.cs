namespace OrlandoUp.Domain;

/// <summary>One line of a booking's history: what happened to it, when, and who did it.</summary>
/// <remarks>
/// Shaped after <see cref="AuditEntry"/>, and for the same two reasons: the actor is copied as
/// text so the record outlives the account that wrote it, and there is no link to the Identity
/// tables because <c>ArchitectureTests</c> forbids this layer from depending on another.
///
/// What differs is where it is written. A booking's history IS its audit, so the booking screens
/// record here and never into <c>AuditEntries</c> — one record per write, in the table whose screen
/// shows it. The summary is one English sentence a human reads, never a serialized diff.
/// </remarks>
public class BookingEvent
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public Booking? Booking { get; set; }

    /// <summary>An instant, from <c>IClock</c> (D16).</summary>
    public DateTime OccurredAtUtc { get; set; }

    /// <summary>The member of staff who acted; null when the customer or the system did.</summary>
    public string? ActorEmail { get; set; }

    public BookingEventType Type { get; set; }

    /// <summary>One sentence in English for a human to read.</summary>
    public string Summary { get; set; } = string.Empty;
}
