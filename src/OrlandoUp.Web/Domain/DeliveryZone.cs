namespace OrlandoUp.Domain;

/// <summary>A group of delivery addresses that share a fee, a hand-over rule and a tax rate.</summary>
public class DeliveryZone
{
    public int Id { get; set; }

    /// <summary>Stable identifier used in code, never shown to the customer.</summary>
    public string Code { get; set; } = string.Empty;

    public ZoneKind Kind { get; set; }

    public decimal DeliveryFee { get; set; }

    public HandoverMode HandoverMode { get; set; }

    /// <summary>Sales tax as a rate, four decimal places; open question Q4 keeps it at zero for now.</summary>
    public decimal SalesTaxRate { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public ICollection<DeliveryZoneTranslation> Translations { get; set; } = new List<DeliveryZoneTranslation>();

    public ICollection<DeliveryLocation> Locations { get; set; } = new List<DeliveryLocation>();
}
