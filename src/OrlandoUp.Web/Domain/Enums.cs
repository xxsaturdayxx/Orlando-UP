namespace OrlandoUp.Domain;

/// <summary>What kind of equipment a product is.</summary>
/// <remarks>
/// Every member carries an explicit number. The values are persisted, so a member reordered
/// or inserted later must not silently repoint existing rows at a different meaning.
/// </remarks>
public enum ProductCategory
{
    MobilityScooter = 1,
    Wheelchair = 2,
    Stroller = 3,
}

/// <summary>How many riders a stroller seats. Only strollers carry it.</summary>
public enum SeatConfiguration
{
    Single = 1,
    Double = 2,
    Triple = 3,
    Infant = 4,
}

/// <summary>Where one physical unit of the fleet stands today.</summary>
public enum UnitStatus
{
    Available = 1,
    Maintenance = 2,
    Retired = 3,
}

/// <summary>How the amount of a pricing tier is read.</summary>
public enum TierMode
{
    /// <summary>One amount for the whole rental, whatever its length inside the tier.</summary>
    FlatPerRental = 1,

    /// <summary>The amount is multiplied by the number of rental days.</summary>
    PerDay = 2,
}

/// <summary>How the amount of an add-on is read.</summary>
public enum AddOnPricingMode
{
    PerRental = 1,
    PerDay = 2,
}

/// <summary>The kind of place a delivery zone groups.</summary>
public enum ZoneKind
{
    DisneyResort = 1,
    UniversalResort = 2,
    HotelOrResort = 3,
    VacationHome = 4,
    Other = 9,
}

/// <summary>How the equipment changes hands at the delivery address.</summary>
public enum HandoverMode
{
    MeetAndGreet = 1,
    FrontDesk = 2,
    Doorstep = 3,
}

/// <summary>What an audited write did to the row it names.</summary>
public enum AuditAction
{
    Created = 1,
    Updated = 2,
    Deactivated = 3,
    Reactivated = 4,
}

/// <summary>What grade of battery this is — an attribute, never an allocation dimension (D2/04b).</summary>
public enum BatteryKind
{
    Normal = 1,
    ExtendedRange = 2,
}

/// <summary>Where a booking stands in its life (D5/03).</summary>
/// <remarks>
/// Every member the architecture's machine names is written now, numbered explicitly, even though
/// this leva reaches only two of the edges between them. The reason is that the set of statuses
/// which HOLD INVENTORY is what the availability rule filters on: adding a member later would mean
/// revisiting that rule with bookings already in the table. <c>PendingPayment</c> is here for the
/// same reason — the payment front adds bookings in that status without touching either file.
///
/// The numbering has a gap on purpose: the states a booking passes through while it is alive are
/// 1 to 8, and the ways it can end are 20 and up. A member inserted later goes at the end of its
/// own band.
/// </remarks>
public enum BookingStatus
{
    Draft = 1,
    PendingPayment = 2,
    Confirmed = 3,
    Scheduled = 4,
    OutForDelivery = 5,
    Active = 6,
    PickedUp = 7,
    Completed = 8,
    Expired = 20,
    Cancelled = 21,
    Refunded = 22,
}

/// <summary>Who put the booking into the system.</summary>
public enum BookingSource
{
    /// <summary>The customer, through the site. Nothing creates one of these yet.</summary>
    Online = 1,

    /// <summary>A member of staff, from a reservation that arrived some other way.</summary>
    Staff = 2,
}

/// <summary>The four windows delivery and pickup are offered in (D3/03).</summary>
/// <remarks>
/// Four rows are not a table. They are an enum with explicit numbers, and the hours they stand for
/// live in <see cref="DeliveryWindows"/> — never in a page, so that changing one is a one-line
/// adjustment in a single file.
/// </remarks>
public enum DeliveryWindow
{
    Morning = 1,
    LateMorning = 2,
    Afternoon = 3,
    Evening = 4,
}

/// <summary>What one line of a booking's history records.</summary>
public enum BookingEventType
{
    Created = 1,
    Cancelled = 2,
    Note = 3,
}
