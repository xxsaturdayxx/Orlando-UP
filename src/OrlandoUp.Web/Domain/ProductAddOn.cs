namespace OrlandoUp.Domain;

/// <summary>Which add-ons a product offers.</summary>
public class ProductAddOn
{
    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int AddOnId { get; set; }

    public AddOn? AddOn { get; set; }
}
