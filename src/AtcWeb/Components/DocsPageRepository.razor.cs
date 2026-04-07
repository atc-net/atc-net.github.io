namespace AtcWeb.Components;

public partial class DocsPageRepository : ComponentBase
{
    private static readonly MarkdownPipeline MarkdownPipelineInstance = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseEmojiAndSmiley()
        .Build();

    private DocsPage? docsPage;

    [Parameter]
    public AtcRepository? Repository { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private void HandleReadmeHeadings(List<MarkdownHeadingInfo> headings)
    {
        docsPage?.AddMarkdownHeadingSections("readme", headings);
    }
}