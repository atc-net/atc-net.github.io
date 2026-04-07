namespace AtcWeb.Components.Repository;

public partial class RepositoryDotNetProjectPackageReferences
{
    [Inject]
    private StateContainer? StateContainer { get; set; }

    [Parameter]
    public List<DotnetNugetPackage>? PackageReferences { get; set; }
}