namespace OrlandoUp.Domain;

/// <summary>
/// The bounds a booking lives inside: how long it may be, how soon it may start, and what its
/// number looks like.
/// </summary>
public static class BookingRules
{
    /// <summary>A rental is at least one day — the day it is delivered.</summary>
    public const int MinDays = 1;

    /// <summary>
    /// Sixty days. Not a price limit — the open-ended band prices any length — but a bound on the
    /// day loop of the availability rule and a refusal of the typo that asks for a year. A rental
    /// longer than this is a conversation, and the page says so.
    /// </summary>
    public const int MaxDays = 60;

    /// <summary>
    /// The soonest day a VISITOR may ask for delivery, given the wall clock in Orlando and the
    /// hour the office stops taking next-day work (D3/03).
    /// </summary>
    /// <remarks>
    /// Before the cut-off, tomorrow. From the cut-off onwards, the day after tomorrow. The whole
    /// value must be Orlando wall time — reading a UTC instant here would move the cut-off by five
    /// hours and, in the evening, by a whole day.
    /// </remarks>
    public static DateOnly EarliestPublicStart(DateTime nowInOrlando, int cutoffHour) =>
        DateOnly.FromDateTime(nowInOrlando).AddDays(nowInOrlando.Hour < cutoffHour ? 1 : 2);

    /// <summary>
    /// The soonest day STAFF may enter, which is today (D9/03).
    /// </summary>
    /// <remarks>
    /// The cut-off does not bind them: they enter what they have already decided to deliver,
    /// including tomorrow morning at ten the night before. What they may not enter is a day
    /// already past, because that booking would hold equipment on days that will never be
    /// delivered.
    /// </remarks>
    public static DateOnly EarliestStaffStart(DateOnly todayInOrlando) => todayInOrlando;

    /// <summary>
    /// The booking number: <c>OU-</c> and the row's key padded to six digits (D6/03).
    /// </summary>
    /// <remarks>
    /// Assigned from the identity key, in the same transaction as the insert, and then stored —
    /// it is what a customer reads aloud on the phone and what a search box looks for, so it must
    /// be a column and not a computation. The year is not in it: a year-scoped sequence needs a
    /// database sequence, which the test provider does not have, and the year is already in
    /// <c>CreatedAtUtc</c>.
    /// </remarks>
    public static string FormatNumber(int id) => $"OU-{id:D6}";
}
