namespace OrlandoUp.Domain;

/// <summary>One rentable model of the fleet, with one row per culture in <see cref="Translations"/>.</summary>
public class Product
{
    public int Id { get; set; }

    /// <summary>URL segment, English, stable after publication.</summary>
    public string Slug { get; set; } = string.Empty;

    public ProductCategory Category { get; set; }

    /// <summary>Only strollers carry a seat configuration.</summary>
    public SeatConfiguration? Configuration { get; set; }

    /// <summary>Strollers carry the per-child limit in the highlights instead.</summary>
    public int? MaxRiderWeightLb { get; set; }

    public decimal WidthIn { get; set; }

    public decimal LengthIn { get; set; }

    /// <summary>Scooters and wheelchairs.</summary>
    public decimal? SeatWidthIn { get; set; }

    /// <summary>Scooters.</summary>
    public decimal? RangeMiles { get; set; }

    /// <summary>Buffer between two rentals of the same unit; availability reads it.</summary>
    public int TurnaroundDays { get; set; }

    /// <summary>Soft hide. A product with history is never deleted.</summary>
    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    /// <summary>Relative path under the images folder; when absent the category illustration shows.</summary>
    public string? ImagePath { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<ProductTranslation> Translations { get; set; } = new List<ProductTranslation>();

    public ICollection<PricingTier> PricingTiers { get; set; } = new List<PricingTier>();

    public ICollection<Unit> Units { get; set; } = new List<Unit>();

    public ICollection<ProductAddOn> AddOns { get; set; } = new List<ProductAddOn>();

    /// <summary>
    /// The Disney bus and Skyliner limit is 30 in by 48 in. Computed here and never stored:
    /// a stored copy would drift the day someone edits the dimensions and forgets the flag.
    /// </summary>
    public bool FitsDisneyTransport => WidthIn <= 30m && LengthIn <= 48m;

    /// <summary>
    /// The lowest daily amount to advertise as "from US$ X/day", or <c>null</c> when the tiers
    /// cannot express one. Absence is returned as absence (Docs/decisions.md D15): a rental with
    /// no readable daily amount must show nothing, and control C17 exists so that it stays that way.
    /// </summary>
    public decimal? FromPricePerDay()
    {
        decimal? lowest = null;

        foreach (PricingTier tier in PricingTiers)
        {
            decimal? candidate = tier.DailyEquivalent();

            if (candidate is null)
            {
                continue;
            }

            if (lowest is null || candidate < lowest)
            {
                lowest = candidate;
            }
        }

        return lowest;
    }
}
