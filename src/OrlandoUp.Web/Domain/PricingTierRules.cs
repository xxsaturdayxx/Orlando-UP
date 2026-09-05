namespace OrlandoUp.Domain;

/// <summary>
/// The price list of a product is valid only when it starts at one day, never covers the same
/// length twice and never stops covering. The rule lives here, not in a page, so that every
/// caller — seeder, admin screen, booking — gets the same answer.
/// </summary>
public static class PricingTierRules
{
    /// <summary>The first problem found, reading the bands from the shortest up.</summary>
    public static PricingTierSetProblem Validate(IEnumerable<PricingTier> tiers)
    {
        List<PricingTier> ordered = tiers
            .OrderBy(t => t.MinDays)
            .ThenBy(t => t.MaxDays ?? int.MaxValue)
            .ToList();

        if (ordered.Count == 0)
        {
            return PricingTierSetProblem.Empty;
        }

        foreach (PricingTier tier in ordered)
        {
            if (tier.MinDays < 1 || tier.Amount <= 0m)
            {
                return PricingTierSetProblem.InvalidBand;
            }

            if (tier.MaxDays is int max && max < tier.MinDays)
            {
                return PricingTierSetProblem.InvalidBand;
            }
        }

        if (ordered[0].MinDays != 1)
        {
            return PricingTierSetProblem.DoesNotStartAtOneDay;
        }

        for (int i = 1; i < ordered.Count; i++)
        {
            PricingTier previous = ordered[i - 1];
            PricingTier current = ordered[i];

            if (previous.MaxDays is null)
            {
                return PricingTierSetProblem.Overlap;
            }

            int expectedStart = previous.MaxDays.Value + 1;

            if (current.MinDays < expectedStart)
            {
                return PricingTierSetProblem.Overlap;
            }

            if (current.MinDays > expectedStart)
            {
                return PricingTierSetProblem.Gap;
            }
        }

        return ordered[^1].MaxDays is null
            ? PricingTierSetProblem.None
            : PricingTierSetProblem.NoOpenEndedBand;
    }
}
