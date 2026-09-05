using OrlandoUp.Application;
using OrlandoUp.Infrastructure;

namespace OrlandoUp.Tests;

/// <summary>
/// The clock is tested on the two instants that sit on either side of the spring-forward switch of
/// 2026, because that is where a time zone that failed to resolve stops being invisible: with the
/// zone missing the conversion would be a no-op and both answers would still look plausible.
/// Running this on the current machine is also what proves the zone identifier resolves on it.
/// </summary>
public class ClockTests
{
    [Fact]
    public void An_instant_before_the_spring_forward_switch_is_still_the_same_Orlando_day()
    {
        // 06:30 UTC is 01:30 in Orlando, standard time, half an hour before the switch.
        IClock clock = At(new DateTime(2026, 3, 8, 6, 30, 0, DateTimeKind.Utc));

        Assert.Equal(new DateOnly(2026, 3, 8), clock.TodayInOrlando());
    }

    [Fact]
    public void An_instant_after_the_spring_forward_switch_is_the_same_Orlando_day()
    {
        // 07:30 UTC is 03:30 in Orlando, daylight time: the clock jumped, the date did not.
        IClock clock = At(new DateTime(2026, 3, 8, 7, 30, 0, DateTimeKind.Utc));

        Assert.Equal(new DateOnly(2026, 3, 8), clock.TodayInOrlando());
    }

    [Fact]
    public void Late_evening_in_Orlando_is_already_the_next_day_in_UTC()
    {
        // 02:00 UTC on the 9th is 22:00 on the 8th in Orlando. A server reading its own local date
        // would answer the 9th here, and every rental booked that evening would land a day late.
        IClock clock = At(new DateTime(2026, 3, 9, 2, 0, 0, DateTimeKind.Utc));

        Assert.Equal(new DateOnly(2026, 3, 8), clock.TodayInOrlando());
    }

    [Fact]
    public void The_instant_it_reports_is_the_instant_it_was_given()
    {
        DateTime instant = new(2026, 7, 4, 15, 45, 0, DateTimeKind.Utc);

        Assert.Equal(instant, At(instant).UtcNow);
    }

    private static IClock At(DateTime utcInstant) => new SystemClock(() => utcInstant);
}
