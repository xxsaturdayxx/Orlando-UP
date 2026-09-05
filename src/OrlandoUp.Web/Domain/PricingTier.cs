namespace OrlandoUp.Domain;

/// <summary>One length band of a product's price list.</summary>
public class PricingTier
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    /// <summary>First rental length, in days, that this tier covers. Never below one.</summary>
    public int MinDays { get; set; }

    /// <summary>Last length covered, or <c>null</c> for the open-ended tier.</summary>
    public int? MaxDays { get; set; }

    public TierMode Mode { get; set; }

    public decimal Amount { get; set; }

    /// <summary>
    /// The daily amount this tier advertises, or <c>null</c> when it cannot be read as one:
    /// a flat tier with no upper bound has no number of days to divide by, and inventing one
    /// would be the same defect D15 forbids. The caller decides what to show for absence.
    /// </summary>
    public decimal? DailyEquivalent()
    {
        if (Mode == TierMode.PerDay)
        {
            return Amount;
        }

        if (MaxDays is int days && days > 0)
        {
            return decimal.Round(Amount / days, 2, MidpointRounding.AwayFromZero);
        }

        return null;
    }

    /// <summary>True when a rental of <paramref name="days"/> days falls inside this tier.</summary>
    public bool Covers(int days) => days >= MinDays && (MaxDays is null || days <= MaxDays);
}
