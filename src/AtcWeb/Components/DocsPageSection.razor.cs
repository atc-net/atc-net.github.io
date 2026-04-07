namespace AtcWeb.Components;

public partial class DocsPageSection
{
    [CascadingParameter]
    private DocsPage? DocsPage { get; set; }

    [CascadingParameter]
    public DocsPageSection? ParentSection { get; protected set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> UserAttributes { get; set; } = new(StringComparer.Ordinal);

    public int Level { get; private set; }

    public string? HeaderTitle { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Level = (ParentSection?.Level ?? -1) + 1;
    }
}