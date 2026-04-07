namespace AtcWeb.Components.Repository;

public partial class RepositoryDotNetProjectAnalyzerSettings
{
    [Inject]
    private StateContainer? StateContainer { get; set; }

    [Parameter]
    public DotnetProjectAnalyzerSettings? AnalyzerSettings { get; set; }
}