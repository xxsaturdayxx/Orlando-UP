using OrlandoUp.Application;

namespace OrlandoUp.Infrastructure;

/// <summary>
/// The one place in the application that reads the machine clock. Control C06 asserts that this
/// file is the only one, by name: if a second file starts reading it, the control turns red and
/// names both.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <summary>
    /// Orlando keeps Eastern time. The IANA identifier is tried first and the Windows one second,
    /// so the same build runs on the developer's Windows machine and on the Linux App Service.
    /// </summary>
    private static readonly TimeZoneInfo OrlandoZone = ResolveOrlandoZone();

    private readonly Func<DateTime> _utcNowSource;

    public SystemClock()
        : this(static () => DateTime.UtcNow)
    {
    }

    /// <summary>
    /// Takes the instant from somewhere else. This exists for the test that pins a moment on each
    /// side of the spring-forward switch and then asks for the Orlando date; the application always
    /// uses the parameterless constructor.
    /// </summary>
    public SystemClock(Func<DateTime> utcNowSource) => _utcNowSource = utcNowSource;

    public DateTime UtcNow => _utcNowSource();

    public DateOnly TodayInOrlando() =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(UtcNow, OrlandoZone));

    private static TimeZoneInfo ResolveOrlandoZone()
    {
        string[] identifiers = ["America/New_York", "Eastern Standard Time"];

        foreach (string id in identifiers)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        throw new InvalidOperationException(
            $"No Orlando time zone on this machine: tried {string.Join(" and ", identifiers)}.");
    }
}
