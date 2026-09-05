namespace OrlandoUp.Domain;

/// <summary>A named address inside a zone — a resort, a hotel, a neighbourhood.</summary>
public class DeliveryLocation
{
    public int Id { get; set; }

    public int ZoneId { get; set; }

    public DeliveryZone? Zone { get; set; }

    /// <summary>The name as the customer would recognise it; it is not translated.</summary>
    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}
