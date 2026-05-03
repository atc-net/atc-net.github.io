namespace AtcWeb.Components.Insights;

public partial class InsightsHealthByCategoryChart : ComponentBase
{
    [Parameter]
    public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];

    protected string[] XAxis { get; private set; } = [];

    protected List<ChartSeries<double>> Series { get; private set; } = [];

    protected override void OnParametersSet()
    {
        var byCategory = Summaries
            .GroupBy(s => RepositoryCategoryHelper.GetCategory(s.Name), StringComparer.Ordinal)
            .OrderBy(g => RepositoryCategoryHelper.GetSortOrder(g.Key))
            .ToList();

        XAxis = byCategory.Select(g => g.Key).ToArray();

        var ok = new double[XAxis.Length];
        var warn = new double[XAxis.Length];
        var err = new double[XAxis.Length];

        for (var i = 0; i < byCategory.Count; i++)
        {
            foreach (var s in byCategory[i])
            {
                switch (ComplianceHealth.Compute(s))
                {
                    case HealthStatus.Ok:
                        ok[i]++;
                        break;
                    case HealthStatus.Warning:
                        warn[i]++;
                        break;
                    case HealthStatus.Error:
                        err[i]++;
                        break;
                }
            }
        }

        Series =
        [
            new ChartSeries<double> { Name = "OK", Data = ok },
            new ChartSeries<double> { Name = "Warning", Data = warn },
            new ChartSeries<double> { Name = "Error", Data = err },
        ];
    }
}