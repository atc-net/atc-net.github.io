namespace AtcWeb.Components.Insights;

public partial class InsightsKpiTiles : ComponentBase
{
    [Parameter]
    public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];

    private int Total => Summaries.Count;

    private int Pct(Func<RepositoryComplianceSummary, bool> predicate)
    {
        if (Summaries.Count == 0)
        {
            return 0;
        }

        return Summaries.Count(predicate) * 100 / Summaries.Count;
    }
}