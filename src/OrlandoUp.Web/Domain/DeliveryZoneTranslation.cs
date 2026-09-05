namespace OrlandoUp.Domain;

/// <summary>The customer-facing text of a delivery zone in one culture.</summary>
public class DeliveryZoneTranslation
{
    public int Id { get; set; }

    public int ZoneId { get; set; }

    public DeliveryZone? Zone { get; set; }

    public string Culture { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>Markdown shown to the customer, rendered through the rich-text gate.</summary>
    public string? Instructions { get; set; }
}
