namespace AtcWeb.Components.Repository;

public partial class NuGetPillButton : ComponentBase
{
    [Parameter]
    public string Href { get; set; } = string.Empty;

    [Parameter]
    public string Text { get; set; } = "Install from NuGet";
}