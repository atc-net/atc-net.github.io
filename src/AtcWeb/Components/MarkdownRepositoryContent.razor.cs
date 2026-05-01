namespace AtcWeb.Components;

public partial class MarkdownRepositoryContent : ComponentBase
{
    private readonly MarkdownPipeline markdownPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseEmojiAndSmiley()
        .Build();

    private string content;
    private List<MarkdownHeadingInfo>? pendingHeadings;

    [Inject]
    private IHtmlSanitizer HtmlSanitizer { get; set; }

    [Inject]
    private IJSRuntime JS { get; set; }

    [Parameter]
    public string RepositoryName { get; set; }

    [Parameter]
    public string RepositoryBranch { get; set; }

    [Parameter]
    public string HeaderName { get; set; }

    [Parameter]
    public string IdPrefix { get; set; }

    [Parameter]
    public EventCallback<List<MarkdownHeadingInfo>> OnHeadingsExtracted { get; set; }

    [Parameter]
    [SuppressMessage("Usage", "BL0007:Component parameters should be auto properties", Justification = "OK.")]
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (pendingHeadings is not null)
        {
            var headings = pendingHeadings;
            pendingHeadings = null;

            if (OnHeadingsExtracted.HasDelegate)
            {
                await OnHeadingsExtracted.InvokeAsync(headings);
            }
        }

        await JS.InvokeVoidAsync("renderMermaidDiagrams");
    }

    [SuppressMessage("Performance", "MA0023:Add RegexOptions.ExplicitCapture", Justification = "Capture groups are used in wiki link replacements.")]
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

        // Strip inline TOC and extract headings when IdPrefix is set
        List<MarkdownHeadingInfo>? headings = null;
        if (!string.IsNullOrEmpty(IdPrefix))
        {
            markdownContent = MarkdownHeadingHelper.StripInlineToc(markdownContent);
            headings = MarkdownHeadingHelper.ExtractHeadings(markdownContent, markdownPipeline, IdPrefix);
        }

        // Convert wiki-style links [[Display|slug]] and [[Page Name]] to standard markdown links
        markdownContent = Regex.Replace(
            markdownContent,
            @"\[\[([^|\]]+)\|([^\]]+)\]\]",
            $"[$1](https://github.com/atc-net/{RepositoryName}/wiki/$2)",
            RegexOptions.None,
            TimeSpan.FromSeconds(5));
        markdownContent = Regex.Replace(
            markdownContent,
            @"\[\[([^\]]+)\]\]",
            m => $"[{m.Groups[1].Value}](https://github.com/atc-net/{RepositoryName}/wiki/{m.Groups[1].Value.Replace(' ', '-')})",
            RegexOptions.None,
            TimeSpan.FromSeconds(5));

        var html = Markdown.ToHtml(markdownContent, markdownPipeline);

        // Inject prefixed heading IDs after converting to HTML
        if (headings is not null && headings.Count > 0)
        {
            html = MarkdownHeadingHelper.InjectHeadingIds(html, headings);
            pendingHeadings = headings;
        }

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

        return SanitizeHtmlToMarkup(html);
    }

    [SuppressMessage("Performance", "MA0023:Add RegexOptions.ExplicitCapture", Justification = "Capture groups are used in replacements.")]
    private MarkupString SanitizeHtmlToMarkup(string html)
    {
        const string hrefBase = "https://github.com/atc-net/";
        const string imgStyle = "max-width: 100%; height: auto;";
        var hrefRepo = $"{hrefBase}{RepositoryName}/blob/{RepositoryBranch}/";
        var hrefRepoTree = $"{hrefBase}{RepositoryName}/tree/{RepositoryBranch}/";

        var sanitizedHtml = HtmlSanitizer.Sanitize(html);

        // Rewrite relative <img src="..."> to the atc-api raw-file proxy
        sanitizedHtml = Regex.Replace(
            sanitizedHtml,
            @"<img src=""(?!https?://)(?:\./)?([^""]+)""",
            m => $@"<img style='{imgStyle}' src=""{AtcApiUrlBuilder.BuildRawFileUrl(RepositoryName, RepositoryBranch, m.Groups[1].Value)}""",
            RegexOptions.None,
            TimeSpan.FromSeconds(5));

        // Rewrite absolute github.com/atc-net/{repo}/blob/{branch}/{path} <img> URLs to the proxy
        sanitizedHtml = Regex.Replace(
            sanitizedHtml,
            @"<img src=""https://github\.com/atc-net/([^/""]+)/blob/([^/""]+)/([^""]+)""",
            m => $@"<img style='{imgStyle}' src=""{AtcApiUrlBuilder.BuildRawFileUrl(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value)}""",
            RegexOptions.None,
            TimeSpan.FromSeconds(5));

        // Rewrite absolute raw.githubusercontent.com/atc-net/{repo}/{branch}/{path} <img> URLs to the proxy
        sanitizedHtml = Regex.Replace(
            sanitizedHtml,
            @"<img src=""https://raw\.githubusercontent\.com/atc-net/([^/""]+)/([^/""]+)/([^""]+)""",
            m => $@"<img style='{imgStyle}' src=""{AtcApiUrlBuilder.BuildRawFileUrl(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value)}""",
            RegexOptions.None,
            TimeSpan.FromSeconds(5));

        // Normalize non-atc-net github.com/{owner}/{repo}/blob/{branch}/{path} <img> URLs
        // to raw.githubusercontent.com so the next step can apply a single routing rule.
        // (github.com/.../blob/... serves the HTML web view, not the image bytes.)
        sanitizedHtml = Regex.Replace(
            sanitizedHtml,
            @"<img src=""https://github\.com/([^/""]+)/([^/""]+)/blob/([^""]+)""",
            @"<img src=""https://raw.githubusercontent.com/$1/$2/$3""",
            RegexOptions.None,
            TimeSpan.FromSeconds(5));

        // Route remaining non-atc-net raw.githubusercontent.com images through the atc-api
        // external proxy when the owner is allow-listed, otherwise leave the direct CDN URL
        // in place. Responsive styling is applied either way.
        sanitizedHtml = Regex.Replace(
            sanitizedHtml,
            @"<img src=""https://raw\.githubusercontent\.com/([^/""]+)/([^/""]+)/([^/""]+)/([^""]+)""",
            m =>
            {
                var owner = m.Groups[1].Value;
                var repo = m.Groups[2].Value;
                var branch = m.Groups[3].Value;
                var path = m.Groups[4].Value;
                var src = AtcApiUrlBuilder.IsExternalProxyOwner(owner)
                    ? AtcApiUrlBuilder.BuildExternalRawFileUrl(owner, repo, branch, path)
                    : $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/{path}";
                return $@"<img style='{imgStyle}' src=""{src}""";
            },
            RegexOptions.None,
            TimeSpan.FromSeconds(5));

        // Rewrite relative src/ links to GitHub tree URLs
        sanitizedHtml = Regex.Replace(
            sanitizedHtml,
            @"<a href=""(?!https?://|#|mailto:)(?:\./)?(src/)([^""]+)""",
            $@"<a target=""_blank"" href=""{hrefRepoTree}src/$2""",
            RegexOptions.None,
            TimeSpan.FromSeconds(5));

        // Rewrite remaining relative links to GitHub blob URLs
        sanitizedHtml = Regex.Replace(
            sanitizedHtml,
            @"<a href=""(?!https?://|#|mailto:)(?:\./)?([^""]+)""",
            $@"<a target=""_blank"" href=""{hrefRepo}$1""",
            RegexOptions.None,
            TimeSpan.FromSeconds(5));

        // Add target="_blank" to absolute atc-net GitHub links
        sanitizedHtml = sanitizedHtml.Replace(
            $"<a href=\"{hrefBase}",
            $"<a target=\"_blank\" href=\"{hrefBase}",
            StringComparison.Ordinal);

        return new MarkupString(sanitizedHtml);
    }

    [SuppressMessage("Performance", "MA0023:Add RegexOptions.ExplicitCapture", Justification = "OK.")]
    private static string FormatHtmlCodeLanguage(
        string html,
        string classLanguage,
        ColorCode.ILanguage colorCodeLanguages)
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