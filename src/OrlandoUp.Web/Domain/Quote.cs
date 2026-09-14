namespace OrlandoUp.Domain;

/// <summary>Why a quote could not be produced. There is no zero-priced fallback (D15).</summary>
public enum QuoteProblem
{
    None = 1,

    /// <summary>The pickup day is before the delivery day.</summary>
    EndBeforeStart = 2,

    /// <summary>Longer than <see cref="BookingRules.MaxDays"/> days — a conversation, not a form.</summary>
    TooLong = 3,

    /// <summary>Nothing to price.</summary>
    NoLines = 4,

    /// <summary>A product's price list does not cover every length without gap or overlap.</summary>
    PriceListInvalid = 5,

    /// <summary>No band of a valid price list covers this many days.</summary>
    NoTierCovers = 6,

    /// <summary>An amount the caller supplied is below zero.</summary>
    NegativeAmount = 7,

    /// <summary>A line asks for fewer than one machine. A line of nothing is not a line.</summary>
    QuantityOutOfRange = 8,

    /// <summary>More second batteries than machines — at most one per machine (D36).</summary>
    ExtrasAboveQuantity = 9,

    /// <summary>A second battery on something that has no battery at all.</summary>
    ExtrasOnNonScooter = 10,
}

/// <summary>One extra asked for on a line, at the price the catalog says today.</summary>
public sealed record QuoteAddOnRequest(int AddOnId, string Name, AddOnPricingMode Mode, decimal Amount);

/// <summary>One product asked for, with its live price list.</summary>
/// <param name="IsScooter">
/// Whether this product has a battery at all. The quote needs it to refuse a second battery on a
/// wheelchair — which availability would simply ignore, leaving the customer charged for a battery
/// that was never counted and never packed.
/// </param>
public sealed record QuoteLineRequest(
    int ProductId,
    string ProductName,
    bool IsScooter,
    int Quantity,
    int ExtraBatteryCount,
    decimal ExtraBatteryPerDay,
    IReadOnlyList<PricingTier> Tiers,
    IReadOnlyList<QuoteAddOnRequest> AddOns);

/// <summary>Everything the quote needs, and nothing that would let it read a database.</summary>
public sealed record QuoteRequest(
    DateOnly Start,
    DateOnly End,
    decimal DeliveryFee,
    decimal TaxRate,
    IReadOnlyList<QuoteLineRequest> Lines);

/// <summary>One priced extra, frozen.</summary>
public sealed record QuotedAddOn(
    int AddOnId,
    string Name,
    AddOnPricingMode Mode,
    decimal Amount,
    int Quantity,
    decimal Total);

/// <summary>One priced product, with the band that priced it, frozen.</summary>
public sealed record QuotedLine(
    int ProductId,
    string ProductName,
    int Quantity,
    int ExtraBatteryCount,
    int TierMinDays,
    int? TierMaxDays,
    TierMode TierMode,
    decimal TierAmount,
    decimal UnitPrice,
    decimal LineTotal,
    decimal ExtraBatteryPerDay,
    decimal ExtraBatteriesTotal,
    IReadOnlyList<QuotedAddOn> AddOns);

/// <summary>The whole price, broken down the way the customer will read it.</summary>
public sealed record QuoteBreakdown(
    int Days,
    decimal Subtotal,
    decimal ExtraBatteriesTotal,
    decimal AddOnsTotal,
    decimal DeliveryFee,
    decimal TaxRate,
    decimal Tax,
    decimal Total,
    IReadOnlyList<QuotedLine> Lines);

/// <summary>Either a price or a named reason there is none — never both, and never a zero.</summary>
public sealed record QuoteResult
{
    private QuoteResult(QuoteProblem problem, QuoteBreakdown? breakdown, PricingTierSetProblem tierProblem, int? productId)
    {
        Problem = problem;
        Breakdown = breakdown;
        TierProblem = tierProblem;
        ProductId = productId;
    }

    public QuoteProblem Problem { get; }

    /// <summary>The price. Null whenever <see cref="Problem"/> is not <see cref="QuoteProblem.None"/>.</summary>
    public QuoteBreakdown? Breakdown { get; }

    /// <summary>Which way the price list was wrong, when that is the problem.</summary>
    public PricingTierSetProblem TierProblem { get; }

    /// <summary>Which product failed, when the problem belongs to one.</summary>
    public int? ProductId { get; }

    public static QuoteResult Priced(QuoteBreakdown breakdown) =>
        new(QuoteProblem.None, breakdown, PricingTierSetProblem.None, null);

    public static QuoteResult Failed(QuoteProblem problem, int? productId = null) =>
        new(problem, null, PricingTierSetProblem.None, productId);

    public static QuoteResult FailedOnPriceList(PricingTierSetProblem tierProblem, int productId) =>
        new(QuoteProblem.PriceListInvalid, null, tierProblem, productId);
}

/// <summary>
/// The only thing in the application that computes what a rental costs.
/// </summary>
/// <remarks>
/// <b>It fails closed.</b> A price list with a gap, a length no band covers, an amount below zero:
/// each one returns the reason and no amount. Nothing here falls back to zero, because a zero is a
/// number a customer would read and quote back on the phone, and no test of value can tell an
/// honest zero from a missing one (D15).
///
/// Rounding happens ONCE, on the tax line, away from zero — every other amount is an integer
/// multiple of a two-decimal price and needs none. That matches what
/// <see cref="PricingTier.DailyEquivalent"/> already does.
/// </remarks>
public static class Quote
{
    public static QuoteResult For(QuoteRequest request)
    {
        if (request.End < request.Start)
        {
            return QuoteResult.Failed(QuoteProblem.EndBeforeStart);
        }

        int days = request.End.DayNumber - request.Start.DayNumber + 1;

        if (days > BookingRules.MaxDays)
        {
            return QuoteResult.Failed(QuoteProblem.TooLong);
        }

        if (request.Lines.Count == 0)
        {
            return QuoteResult.Failed(QuoteProblem.NoLines);
        }

        if (request.DeliveryFee < 0 || request.TaxRate < 0)
        {
            return QuoteResult.Failed(QuoteProblem.NegativeAmount);
        }

        List<QuotedLine> quotedLines = [];

        foreach (QuoteLineRequest line in request.Lines)
        {
            // The shape of the line is checked HERE and not only on the screen. A screen is one
            // caller; the payment front is a second, and an invariant that only the first one
            // knows does not survive the second (architecture.md §2 — writes go through services
            // so that booking invariants live in one place).
            if (line.Quantity < 1)
            {
                return QuoteResult.Failed(QuoteProblem.QuantityOutOfRange, line.ProductId);
            }

            if (line.ExtraBatteryCount > 0 && !line.IsScooter)
            {
                return QuoteResult.Failed(QuoteProblem.ExtrasOnNonScooter, line.ProductId);
            }

            // At most one second battery per machine (D36), and never a negative count: both ends
            // matter, because a negative would subtract from a total nobody checked.
            if (line.ExtraBatteryCount < 0 || line.ExtraBatteryCount > line.Quantity)
            {
                return QuoteResult.Failed(QuoteProblem.ExtrasAboveQuantity, line.ProductId);
            }

            if (line.ExtraBatteryPerDay < 0)
            {
                return QuoteResult.Failed(QuoteProblem.NegativeAmount, line.ProductId);
            }

            // The same validator the catalog screen and the seeder use. A second one would be a
            // second opinion, and two opinions about a price list is how a gap reaches a customer.
            PricingTierSetProblem tierProblem = PricingTierRules.Validate(line.Tiers);

            if (tierProblem != PricingTierSetProblem.None)
            {
                return QuoteResult.FailedOnPriceList(tierProblem, line.ProductId);
            }

            PricingTier? band = null;

            foreach (PricingTier tier in line.Tiers)
            {
                if (tier.Covers(days))
                {
                    band = tier;
                    break;
                }
            }

            // A valid list covers every length, so this is unreachable through the screens — and it
            // is here anyway, because the alternative to naming the impossible is dereferencing it.
            if (band is null)
            {
                return QuoteResult.Failed(QuoteProblem.NoTierCovers, line.ProductId);
            }

            if (band.Amount < 0)
            {
                return QuoteResult.Failed(QuoteProblem.NegativeAmount, line.ProductId);
            }

            decimal unitPrice = band.Mode == TierMode.FlatPerRental
                ? band.Amount
                : band.Amount * days;

            decimal lineTotal = unitPrice * line.Quantity;
            decimal extraBatteries = line.ExtraBatteryCount * line.ExtraBatteryPerDay * days;

            List<QuotedAddOn> quotedAddOns = [];

            foreach (QuoteAddOnRequest addOn in line.AddOns)
            {
                if (addOn.Amount < 0)
                {
                    return QuoteResult.Failed(QuoteProblem.NegativeAmount, line.ProductId);
                }

                decimal addOnTotal = addOn.Mode == AddOnPricingMode.PerDay
                    ? addOn.Amount * line.Quantity * days
                    : addOn.Amount * line.Quantity;

                quotedAddOns.Add(new QuotedAddOn(
                    addOn.AddOnId, addOn.Name, addOn.Mode, addOn.Amount, line.Quantity, addOnTotal));
            }

            quotedLines.Add(new QuotedLine(
                line.ProductId,
                line.ProductName,
                line.Quantity,
                line.ExtraBatteryCount,
                band.MinDays,
                band.MaxDays,
                band.Mode,
                band.Amount,
                unitPrice,
                lineTotal,
                line.ExtraBatteryPerDay,
                extraBatteries,
                quotedAddOns));
        }

        decimal subtotal = quotedLines.Sum(line => line.LineTotal);
        decimal extraBatteriesTotal = quotedLines.Sum(line => line.ExtraBatteriesTotal);
        decimal addOnsTotal = quotedLines.Sum(line => line.AddOns.Sum(addOn => addOn.Total));

        decimal taxable = subtotal + extraBatteriesTotal + addOnsTotal + request.DeliveryFee;
        decimal tax = decimal.Round(taxable * request.TaxRate, 2, MidpointRounding.AwayFromZero);

        return QuoteResult.Priced(new QuoteBreakdown(
            days,
            subtotal,
            extraBatteriesTotal,
            addOnsTotal,
            request.DeliveryFee,
            request.TaxRate,
            tax,
            taxable + tax,
            quotedLines));
    }
}
