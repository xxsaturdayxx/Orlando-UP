namespace OrlandoUp.Application;

/// <summary>
/// The only source of "now" and "today" in the application (Docs/decisions.md D16). Reading the
/// machine's local wall clock directly is what control C05 forbids: the server runs in one place
/// and the rentals happen in another, so a local reading would be right only by accident.
/// </summary>
public interface IClock
{
    /// <summary>The current instant, in UTC. Audit fields store exactly this.</summary>
    DateTime UtcNow { get; }

    /// <summary>The calendar date in Orlando right now — a date, never an instant.</summary>
    DateOnly TodayInOrlando();
}
