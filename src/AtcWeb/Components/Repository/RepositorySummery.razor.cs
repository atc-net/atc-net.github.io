namespace AtcWeb.Components.Repository;

public partial class RepositorySummery
{
    [Inject]
    private StateContainer? StateContainer { get; set; }

    [Parameter]
    public AtcRepository? Repository { get; set; }
}