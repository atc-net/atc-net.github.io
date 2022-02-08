using Ganss.XSS;
using Markdig;

namespace AtcWeb.Components;

public class MarkdownRepositoryContentBase : ComponentBase
{
    private readonly MarkdownPipeline markdownPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private string content;

    [Inject]
    protected IHtmlSanitizer HtmlSanitizer { get; set; }

    [Parameter]
    public string RepositoryName { get; set; }

    [Parameter]
    public string RepositoryBranch { get; set; }

    [Parameter]
    public string Content
    {
        get => this.content;
        set
        {
            this.content = value;
            HtmlContent = ConvertMarkdownToHtml(this.content);
        }
    }

    protected MarkupString HtmlContent { get; private set; }

    private MarkupString ConvertMarkdownToHtml(string markdownContent)
    {
        if (string.IsNullOrWhiteSpace(markdownContent) ||
            string.IsNullOrWhiteSpace(RepositoryName) ||
            string.IsNullOrWhiteSpace(RepositoryBranch))
        {
            return default;
        }

        var html = Markdown.ToHtml(markdownContent, markdownPipeline);

        const string imgSrcRaw = "https://raw.githubusercontent.com/";
        var imgSrc2from = $"https://github.com/atc-net/{RepositoryName}/blob/{RepositoryBranch}/";
        var imgSrc2to = $"https://github.com/atc-net/{RepositoryName}/raw/{RepositoryBranch}/";
        const string hrefBase = "https://github.com/atc-net/";

        var sanitizedHtml = HtmlSanitizer
            .Sanitize(html)
            .Replace(
                $"<img src=\"{imgSrcRaw}",
                $"<img style='height: 100%; width: 100%; object-fit: contain' src=\"{imgSrcRaw}",
                StringComparison.Ordinal)
            .Replace(
                $"<img src=\"{imgSrc2from}",
                $"<img style='height: 100%; width: 100%; object-fit: contain' src=\"{imgSrc2to}",
                StringComparison.Ordinal)
            .Replace(
                $"<a href=\"{hrefBase}",
                $"<a target=\"_blank\" href=\"{hrefBase}",
                StringComparison.Ordinal);

        return new MarkupString(sanitizedHtml);
    }
}