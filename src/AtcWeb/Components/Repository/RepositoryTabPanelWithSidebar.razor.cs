namespace AtcWeb.Components.Repository;

public partial class RepositoryTabPanelWithSidebar : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public AtcRepository? Repository { get; set; }
}