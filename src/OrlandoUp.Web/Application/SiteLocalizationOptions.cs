namespace OrlandoUp.Application;

/// <summary>
/// The cultures the site serves. Named with the Site prefix because the framework already has a
/// type called LocalizationOptions, and two types with one name in the same file is a trap.
/// </summary>
public sealed class SiteLocalizationOptions
{
    public const string SectionName = "Localization";

    /// <summary>The culture served without a URL prefix.</summary>
    public string DefaultCulture { get; set; } = "en-US";

    /// <summary>Every UI culture the site answers in, the default one included.</summary>
    public string[] SupportedUICultures { get; set; } = ["en-US", "pt-BR"];
}
