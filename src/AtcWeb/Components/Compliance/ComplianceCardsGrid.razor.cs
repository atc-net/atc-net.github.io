namespace AtcWeb.Components.Compliance;

public partial class ComplianceCardsGrid : ComponentBase
{
    [Parameter]
    public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];

    private static Color HealthColor(HealthStatus h) => h switch
    {
        HealthStatus.Ok => Color.Success,
        HealthStatus.Warning => Color.Warning,
        _ => Color.Error,
    };
}