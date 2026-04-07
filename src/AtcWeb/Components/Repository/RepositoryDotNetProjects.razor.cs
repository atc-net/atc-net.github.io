namespace AtcWeb.Components.Repository;

public partial class RepositoryDotNetProjects
{
    [Inject]
    private StateContainer? StateContainer { get; set; }

    [Parameter]
    public AtcRepository? Repository { get; set; }
}