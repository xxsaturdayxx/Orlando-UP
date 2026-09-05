namespace OrlandoUp.Application;

/// <summary>
/// Indexing stays off while the catalog carries placeholder prices; phase 5 flips it.
/// </summary>
public sealed class SeoOptions
{
    public const string SectionName = "Seo";

    public bool AllowIndexing { get; set; }

    public string CanonicalHost { get; set; } = string.Empty;
}
