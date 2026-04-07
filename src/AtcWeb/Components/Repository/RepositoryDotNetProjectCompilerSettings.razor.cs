namespace AtcWeb.Components.Repository;

public partial class RepositoryDotNetProjectCompilerSettings
{
    [Inject]
    private StateContainer? StateContainer { get; set; }

    [Parameter]
    public DotnetProjectCompilerSettings? CompilerSettings { get; set; }
}