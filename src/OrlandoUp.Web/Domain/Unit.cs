namespace OrlandoUp.Domain;

/// <summary>One physical piece of equipment of a product.</summary>
public class Unit
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    /// <summary>The tag stuck on the equipment; unique across the fleet.</summary>
    public string AssetTag { get; set; } = string.Empty;

    public string? SerialNumber { get; set; }

    public UnitStatus Status { get; set; } = UnitStatus.Available;

    public string? Notes { get; set; }

    /// <summary>A calendar date in Orlando, not an instant.</summary>
    public DateOnly? PurchasedOn { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
