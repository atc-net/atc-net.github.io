using System;
using Ganss.XSS;
using Markdig;
using Microsoft.AspNetCore.Components;

namespace AtcWeb.Components
{
    public class MarkdownContentBase : ComponentBase
    {
        private readonly MarkdownPipeline markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        private string content;

        [Inject]
        protected IHtmlSanitizer HtmlSanitizer { get; set; }

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
            if (string.IsNullOrWhiteSpace(markdownContent))
            {
                return default;
            }

            var html = Markdown.ToHtml(markdownContent, markdownPipeline);

            var sanitizedHtml = HtmlSanitizer
                    .Sanitize(html)
                    .Replace(
                        "<img src=\"https://raw.githubusercontent.com/",
                        "<img style='height: 100%; width: 100%; object-fit: contain' src=\"https://raw.githubusercontent.com/",
                        StringComparison.Ordinal)
                    .Replace(
                        "<a href=\"https://github.com/atc-net/",
                        "<a target=\"_blank\" href=\"https://github.com/atc-net/",
                        StringComparison.Ordinal);

            return new MarkupString(sanitizedHtml);
        }
    }
}