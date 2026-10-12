namespace AtcWeb.Components;

public partial class DocsPageRepository : ComponentBase
{
    private int activeTabIndex;
    private List<MarkdownHeadingInfo>? readmeHeadings;
    private List<MarkdownHeadingInfo>? changelogHeadings;

    [Parameter]
    public AtcRepository? Repository { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private void OnTabChanged(int index)
    {
        activeTabIndex = index;
    }

    private void OnReadmeHeadingsExtracted(List<MarkdownHeadingInfo> headings)
        => UpdateHeadings(ref readmeHeadings, headings);

    private void OnChangelogHeadingsExtracted(
        List<MarkdownHeadingInfo> headings)
        => UpdateHeadings(ref changelogHeadings, headings);

    private static void UpdateHeadings(
        ref List<MarkdownHeadingInfo>? target,
        List<MarkdownHeadingInfo> headings)
    {
        // Keep the existing list when nothing changed, so the TOC is not rebuilt on every tab switch
        if (target is not null &&
            target.Select(x => x.Id).SequenceEqual(headings.Select(x => x.Id), StringComparer.Ordinal))
        {
            return;
        }

        target = headings;
    }
}