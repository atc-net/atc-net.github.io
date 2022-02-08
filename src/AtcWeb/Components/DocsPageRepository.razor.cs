namespace AtcWeb.Components;

public class DocsPageRepositoryBase : ComponentBase
{
    [Parameter]
    public AtcRepository? Repository { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}