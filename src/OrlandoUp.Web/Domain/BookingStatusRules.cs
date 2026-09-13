namespace OrlandoUp.Domain;

/// <summary>
/// Which statuses hold equipment, and which changes of status are legal.
/// </summary>
/// <remarks>
/// Both answers live here and nowhere else. <see cref="HoldsInventory"/> is the set the
/// availability loader filters on, so a status added to it silently changes what the site may
/// promise; <see cref="CanTransition"/> is the machine of <c>Docs/architecture.md</c> §3, written
/// whole now so that no later front redraws it from memory.
///
/// This leva uses exactly one of the edges — a staff booking is born <see cref="BookingStatus.Confirmed"/>
/// and the only thing that happens to it is being cancelled. The rest are here because a table
/// half-written is a table nobody trusts, and because the test that pins them costs one method.
/// </remarks>
public static class BookingStatusRules
{
    /// <summary>
    /// True when a booking in this status is occupying equipment on its dates.
    /// </summary>
    /// <remarks>
    /// <see cref="BookingStatus.PendingPayment"/> is in the set on purpose (D5/03): a customer
    /// halfway through checkout is holding the machine, and the payment front must be able to
    /// create such bookings without touching the availability rule. <see cref="BookingStatus.Draft"/>
    /// is not — nothing has been promised. <see cref="BookingStatus.PickedUp"/> is not either: the
    /// equipment is back in the van and the turnaround, not the booking, is what protects the
    /// following days.
    /// </remarks>
    public static bool HoldsInventory(BookingStatus status) => status switch
    {
        BookingStatus.PendingPayment => true,
        BookingStatus.Confirmed => true,
        BookingStatus.Scheduled => true,
        BookingStatus.OutForDelivery => true,
        BookingStatus.Active => true,
        _ => false,
    };

    /// <summary>Whether a booking may move from one status to another.</summary>
    public static bool CanTransition(BookingStatus from, BookingStatus to)
    {
        // A status never transitions to itself: "confirm an already confirmed booking" is a
        // double submit, not a state change, and letting it through would write a second event
        // saying something happened twice.
        if (from == to)
        {
            return false;
        }

        return (from, to) switch
        {
            // The ordinary life of a booking, in order.
            (BookingStatus.Draft, BookingStatus.PendingPayment) => true,
            (BookingStatus.PendingPayment, BookingStatus.Confirmed) => true,
            (BookingStatus.Confirmed, BookingStatus.Scheduled) => true,
            (BookingStatus.Scheduled, BookingStatus.OutForDelivery) => true,
            (BookingStatus.OutForDelivery, BookingStatus.Active) => true,
            (BookingStatus.Active, BookingStatus.PickedUp) => true,
            (BookingStatus.PickedUp, BookingStatus.Completed) => true,

            // The side exits. A draft or a pending session expires; anything still alive can be
            // cancelled, up to and including a booking already out for delivery — the van turns
            // around, and that is a cancellation and not a new kind of state.
            (BookingStatus.Draft, BookingStatus.Expired) => true,
            (BookingStatus.PendingPayment, BookingStatus.Expired) => true,
            (BookingStatus.Draft, BookingStatus.Cancelled) => true,
            (BookingStatus.PendingPayment, BookingStatus.Cancelled) => true,
            (BookingStatus.Confirmed, BookingStatus.Cancelled) => true,
            (BookingStatus.Scheduled, BookingStatus.Cancelled) => true,
            (BookingStatus.OutForDelivery, BookingStatus.Cancelled) => true,

            // Money goes back after the booking is already off the fleet, never instead of
            // cancelling: a refund is the second step, which is why Cancelled is the only door in.
            (BookingStatus.Cancelled, BookingStatus.Refunded) => true,
            (BookingStatus.Completed, BookingStatus.Refunded) => true,

            _ => false,
        };
    }
}
