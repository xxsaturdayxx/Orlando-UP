namespace OrlandoUp.Domain;

/// <summary>Something extra a customer can put on a rental.</summary>
public class AddOn
{
    public int Id { get; set; }

    /// <summary>Stable identifier used in code and URLs, never shown to the customer.</summary>
    public string Code { get; set; } = string.Empty;

    public AddOnPricingMode PricingMode { get; set; }

    public decimal Amount { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public ICollection<AddOnTranslation> Translations { get; set; } = new List<AddOnTranslation>();

    public ICollection<ProductAddOn> Products { get; set; } = new List<ProductAddOn>();
}
