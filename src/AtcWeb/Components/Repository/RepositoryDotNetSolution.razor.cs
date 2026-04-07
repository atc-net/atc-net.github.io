namespace AtcWeb.Components.Repository;

public partial class RepositoryDotNetSolution
{
    [Inject]
    private StateContainer? StateContainer { get; set; }

    [Parameter]
    public AtcRepository? Repository { get; set; }
}