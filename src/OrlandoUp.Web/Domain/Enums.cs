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
