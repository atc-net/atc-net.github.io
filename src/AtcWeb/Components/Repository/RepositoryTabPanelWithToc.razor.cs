namespace AtcWeb.Components.Repository;

public partial class RepositoryTabPanelWithToc : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string TocTitle { get; set; } = "On this page";

    [Parameter]
    public IReadOnlyList<MarkdownHeadingInfo>? Headings { get; set; }

    private bool HasToc => Headings?.Count > 1;
}