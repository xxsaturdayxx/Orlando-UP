namespace OrlandoUp.Application;

/// <summary>
/// The company data the footer and the contact page show. The defaults are deliberately visible
/// placeholders: a page that still shows them is visibly unfinished, and open question Q9 is what
/// closes them. Control C16 counts them and expires when Q9 is answered.
/// </summary>
public sealed class CompanyOptions
{
    public const string SectionName = "Company";

    public string LegalName { get; set; } = string.Empty;

    public string TradeName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string WhatsApp { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Hours { get; set; } = string.Empty;
}
