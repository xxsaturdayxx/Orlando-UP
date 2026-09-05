namespace OrlandoUp.Domain;

/// <summary>The customer-facing text of a product in one culture.</summary>
public class ProductTranslation
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    /// <summary>A culture name such as en-US or pt-BR.</summary>
    public string Culture { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Tagline { get; set; }

    /// <summary>Markdown, rendered through the rich-text gate before it reaches a page.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>A JSON array of short strings.</summary>
    public string Highlights { get; set; } = "[]";
}
