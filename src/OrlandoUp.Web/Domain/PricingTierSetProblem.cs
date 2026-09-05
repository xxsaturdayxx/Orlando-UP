namespace OrlandoUp.Domain;

/// <summary>What is wrong with the price list of one product, if anything.</summary>
public enum PricingTierSetProblem
{
    /// <summary>The bands start at one day, do not overlap, and reach the open end.</summary>
    None = 0,

    /// <summary>The product has no band at all.</summary>
    Empty = 1,

    /// <summary>A band starts below one day, or ends before it starts, or asks for a non-positive amount.</summary>
    InvalidBand = 2,

    /// <summary>The shortest band does not start at one day.</summary>
    DoesNotStartAtOneDay = 3,

    /// <summary>Two bands cover the same length.</summary>
    Overlap = 4,

    /// <summary>A length between the shortest and the longest band is covered by none.</summary>
    Gap = 5,

    /// <summary>Every band has an upper bound, so a long rental would have no price.</summary>
    NoOpenEndedBand = 6,
}
