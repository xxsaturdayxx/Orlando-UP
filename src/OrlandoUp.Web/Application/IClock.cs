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

    /// <summary>
    /// The wall clock in Orlando right now: the same moment as <see cref="UtcNow"/>, read off the
    /// clock on the office wall.
    /// </summary>
    /// <remarks>
    /// The next-day cut-off needs the HOUR and not only the day (D3/03), and the hour is the part
    /// that differs: at 23:00 UTC it is six in the evening in Orlando, which is past a cut-off of
    /// eighteen, while the UTC hour is not. Returning it from here rather than letting a caller
    /// convert is what keeps the zone in one place.
    /// </remarks>
    DateTime NowInOrlando();
}
