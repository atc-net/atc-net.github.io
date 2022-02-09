using System.Text.RegularExpressions;
using System.Web;
using ColorCode.Styling;
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
    public string HeaderName { get; set; }

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

        if (!string.IsNullOrEmpty(HeaderName))
        {
            var mdHeader = $"# {HeaderName}\n\n";
            if (markdownContent.StartsWith(mdHeader, StringComparison.OrdinalIgnoreCase))
            {
                markdownContent = markdownContent.Substring(mdHeader.Length);
            }
        }

        var html = Markdown.ToHtml(markdownContent, markdownPipeline);

        const string imgSrcRaw = "https://raw.githubusercontent.com/";
        var imgSrc2from = $"https://github.com/atc-net/{RepositoryName}/blob/{RepositoryBranch}/";
        var imgSrc2to = $"https://github.com/atc-net/{RepositoryName}/raw/{RepositoryBranch}/";
        const string hrefBase = "https://github.com/atc-net/";

        if (html.Contains("language-csharp", StringComparison.Ordinal))
        {
            html = FormatHtmlCodeLanguage(html, "language-csharp", ColorCode.Languages.CSharp);
        }

        if (html.Contains("language-powershell", StringComparison.Ordinal))
        {
            html = FormatHtmlCodeLanguage(html, "language-powershell", ColorCode.Languages.PowerShell);
        }

        if (html.Contains("language-html", StringComparison.Ordinal))
        {
            html = FormatHtmlCodeLanguage(html, "language-html", ColorCode.Languages.Html);
        }

        if (html.Contains("<code>", StringComparison.Ordinal))
        {
            html = html.Replace("<code>", "<code class=\"language-unknown\">", StringComparison.Ordinal);
        }

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

    [SuppressMessage("Performance", "MA0023:Add RegexOptions.ExplicitCapture", Justification = "OK.")]
    private static string FormatHtmlCodeLanguage(string html, string classLanguage, ColorCode.ILanguage colorCodeLanguages)
    {
        var matches = Regex.Matches(html, $"<code class=\"{classLanguage}\">(.*?)</code>", RegexOptions.Singleline | RegexOptions.Compiled, TimeSpan.FromSeconds(5));
        foreach (Match match in matches)
        {
            var csharp = match.Value
                .Replace($"<code class=\"{classLanguage}\">", string.Empty, StringComparison.Ordinal)
                .Replace("</code>", string.Empty, StringComparison.Ordinal);

            var csharpToFormat = HttpUtility.HtmlDecode(csharp).Trim();

            var formatter = new ColorCode.HtmlFormatter(StyleDictionary.DefaultDark);
            var csharpHtml = formatter.GetHtmlString(csharpToFormat, colorCodeLanguages);

            html = html.Replace(csharp, csharpHtml, StringComparison.Ordinal);
        }

        return html;
    }
}