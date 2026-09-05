using Ganss.Xss;
using Markdig;

namespace OrlandoUp.Application;

/// <summary>
/// The single gate between stored Markdown and rendered HTML. Both packages enter the code base
/// here and nowhere else (control C11), because they only work as a pair: Markdig turns Markdown
/// into HTML and does not sanitise it, and the sanitiser is what makes the result safe to render
/// from a database column an admin can edit.
/// </summary>
public sealed class RichText
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAutoLinks()
        .UsePipeTables()
        .DisableHtml()
        .Build();

    private static readonly HtmlSanitizer Sanitizer = new();

    /// <summary>Renders Markdown as sanitised HTML. Absent or blank input renders as nothing.</summary>
    public string ToHtml(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        string html = Markdown.ToHtml(markdown, Pipeline);

        return Sanitizer.Sanitize(html);
    }

    /// <summary>Renders Markdown as plain text, for meta descriptions and card summaries.</summary>
    public string ToPlainText(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        return Markdown.ToPlainText(markdown, Pipeline).Trim();
    }
}
