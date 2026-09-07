using OrlandoUp.Domain;

namespace OrlandoUp.Application.Catalog;

/// <summary>What a product card shows. The price is nullable and stays nullable (D15).</summary>
/// <remarks>
/// <paramref name="FitsDisneyTransport"/> carries three answers and the badge shows only on
/// <c>true</c>; <paramref name="IsBookable"/> is what separates a price from a "coming soon" (D32).
/// </remarks>
public sealed record ProductCard(
    string Slug,
    ProductCategory Category,
    string Name,
    string? Tagline,
    bool? FitsDisneyTransport,
    bool IsBookable,
    decimal? FromPricePerDay,
    string? ImagePath);

/// <summary>One band of the price table of a product.</summary>
public sealed record PricingRow(int MinDays, int? MaxDays, TierMode Mode, decimal Amount);

/// <summary>One add-on offered with a product.</summary>
public sealed record AddOnRow(string Code, string Name, string? Description, AddOnPricingMode PricingMode, decimal Amount);

/// <summary>Everything the product page shows.</summary>
public sealed record ProductDetail(
    string Slug,
    ProductCategory Category,
    SeatConfiguration? Configuration,
    string Name,
    string? Tagline,
    string DescriptionHtml,
    IReadOnlyList<string> Highlights,
    int? MaxRiderWeightLb,
    decimal? WidthIn,
    decimal? LengthIn,
    decimal? SeatWidthIn,
    decimal? RangeMiles,
    bool? FitsDisneyTransport,
    bool IsBookable,
    string? ImagePath,
    IReadOnlyList<PricingRow> PricingRows,
    IReadOnlyList<AddOnRow> AddOns);

/// <summary>The hand-over text of one delivery zone, as the public pages show it.</summary>
/// <remarks>
/// The hand-over MODE travels with it so that a page can ask for "the zone where we meet you in
/// person" instead of naming a zone by its code. A code typed into a page is fleet data living in
/// source: rename the zone in the administration and the page quietly stops finding it, with
/// nothing failing anywhere.
/// </remarks>
public sealed record ZoneInstructions(string Code, string Name, string InstructionsHtml, HandoverMode Handover);
