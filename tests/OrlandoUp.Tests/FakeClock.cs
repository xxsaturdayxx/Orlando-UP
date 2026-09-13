using OrlandoUp.Application;
using OrlandoUp.Infrastructure;

namespace OrlandoUp.Tests;

/// <summary>
/// A clock a test can move. Availability and the next-day cut-off are functions of what day and
/// what hour it is in Orlando, and a test that read the machine clock would pass on a Tuesday
/// afternoon and fail on a Saturday evening.
/// </summary>
/// <remarks>
/// <b>It does not resolve the time zone itself; it delegates to <see cref="SystemClock"/>.</b> That
/// class already takes the instant from a caller-supplied source — the constructor exists for the
/// test that pins a moment on each side of the spring-forward switch — so handing it a lambda that
/// reads this object's field gives the fake the real conversion table for free. The alternative was
/// to copy the zone lookup here, which would have meant the fake could agree with itself while
/// disagreeing with the application, and that is the one thing a fake clock must never do.
///
/// It reads <c>DateTime.UtcNow</c> once, to start frozen at the moment it was built. That is a
/// reading of the machine clock, and it is allowed here because the control that forbids it scans
/// the source folder and not this one — the application still has exactly one clock reader.
/// </remarks>
public sealed class FakeClock : IClock
{
    private readonly SystemClock _zone;

    private DateTime _utcNow;

    public FakeClock()
        : this(DateTime.UtcNow)
    {
    }

    public FakeClock(DateTime utcNow)
    {
        _utcNow = utcNow;

        // The lambda closes over this instance, so every later Set is seen through it.
        _zone = new SystemClock(() => _utcNow);
    }

    public DateTime UtcNow => _utcNow;

    public DateOnly TodayInOrlando() => _zone.TodayInOrlando();

    public DateTime NowInOrlando() => _zone.NowInOrlando();

    /// <summary>Moves the clock. The instant is UTC, the way every instant in this project is.</summary>
    public void Set(DateTime utcNow) => _utcNow = utcNow;
}
