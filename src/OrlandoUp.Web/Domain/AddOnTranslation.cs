namespace OrlandoUp.Domain;

/// <summary>The customer-facing text of an add-on in one culture.</summary>
public class AddOnTranslation
{
    public int Id { get; set; }

    public int AddOnId { get; set; }

    public AddOn? AddOn { get; set; }

    public string Culture { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
