namespace AtcWeb.Components;

public partial class DocsPageRepository : ComponentBase
{
    private int activeTabIndex;

    [Parameter]
    public AtcRepository? Repository { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private void OnTabChanged(int index)
    {
        activeTabIndex = index;
    }
}