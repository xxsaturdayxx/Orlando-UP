namespace OrlandoUp.Domain;

/// <summary>One battery of the fleet, belonging to a scooter model rather than to a machine.</summary>
/// <remarks>
/// It mirrors <see cref="Unit"/> on purpose: the product row IS the model, and a battery belongs to
/// the model (D37, D1/04b). Reusing <see cref="Unit"/> itself would overload a type that means
/// "a rentable machine" and would move the dashboard counts, the seed cardinals and the assertion
/// that only a product on sale carries units.
///
/// Like <see cref="Unit"/>, it carries no <c>UpdatedAtUtc</c>: the audit line is its record.
/// </remarks>
public class Battery
{
    public int Id { get; set; }

    /// <summary>The scooter MODEL this battery belongs to, never a single machine.</summary>
    public int ProductId { get; set; }

    public Product? Product { get; set; }

    /// <summary>
    /// The tag stuck on the battery, unique across the whole fleet. D38 gives it the shape
    /// <c>LLL-NN</c> — <c>BSC-01</c> for a Drive Scout battery, <c>BSP-01</c> for a Spitfire one.
    /// The distinct prefixes make a collision with a unit's tag impossible by construction, which
    /// is a reason to keep the unique index rather than to drop it: construction is not proof.
    /// </summary>
    public string AssetTag { get; set; } = string.Empty;

    /// <summary>
    /// What this battery is, and never how it is allocated (D2/04b). An XL satisfies every promise
    /// a Normal satisfies and costs the customer nothing more, so availability treats all batteries
    /// of a model as one pool. The grade is NOT in the tag: a tag identifies, a column describes.
    /// </summary>
    public BatteryKind Kind { get; set; }

    /// <summary>
    /// What this battery actually delivers. Absent until somebody measures it — a battery nobody
    /// measured says nothing rather than zero (D15).
    /// </summary>
    public decimal? RangeMiles { get; set; }

    /// <summary>Reuses the unit's states: a retired battery is exactly the broken one (D5/04b).</summary>
    public UnitStatus Status { get; set; } = UnitStatus.Available;

    public string? SerialNumber { get; set; }

    /// <summary>Where "a customer broke this one" is written.</summary>
    public string? Notes { get; set; }

    /// <summary>A calendar date in Orlando, not an instant.</summary>
    public DateOnly? PurchasedOn { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
