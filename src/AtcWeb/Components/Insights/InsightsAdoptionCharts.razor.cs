namespace AtcWeb.Components.Insights;

public partial class InsightsAdoptionCharts : ComponentBase
{
    [Parameter]
    public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];

    protected List<ChartSeries<double>> TfmSeries { get; private set; } = [];

    protected string[] TfmLabels { get; private set; } = [];

    protected string[] AnalyzerLabels { get; private set; } = [];

    protected List<ChartSeries<double>> AnalyzerSeries { get; private set; } = [];

    protected List<ChartSeries<double>> LanguageSeries { get; private set; } = [];

    protected string[] LanguageLabels { get; private set; } = [];

    protected override void OnParametersSet()
    {
        var tfms = Summaries
            .Select(s => s.Signals.GlobalTargetFramework)
            .Where(v => !string.IsNullOrEmpty(v))
            .Cast<string>()
            .GroupBy(v => v, StringComparer.Ordinal)
            .OrderByDescending(g => g.Count())
            .ToList();
        TfmLabels = tfms.Select(g => g.Key).ToArray();
        TfmSeries =
        [
            new ChartSeries<double>
            {
                Name = "Repos",
                Data = tfms.Select(g => (double)g.Count()).ToArray(),
            },
        ];

        var analyzerVersions = Summaries
            .SelectMany(s => s.Detail.AnalyzerPackages)
            .Where(p => string.Equals(p.PackageId, "Atc.Analyzer", StringComparison.Ordinal))
            .GroupBy(p => p.Version, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();
        AnalyzerLabels = analyzerVersions.Select(g => g.Key).ToArray();
        AnalyzerSeries =
        [
            new ChartSeries<double>
            {
                Name = "Repos",
                Data = analyzerVersions.Select(g => (double)g.Count()).ToArray(),
            },
        ];

        var langs = Summaries
            .GroupBy(
                s => string.IsNullOrEmpty(s.Language) ? "Unknown" : s.Language!,
                StringComparer.Ordinal)
            .OrderByDescending(g => g.Count())
            .ToList();
        LanguageLabels = langs.Select(g => g.Key).ToArray();
        LanguageSeries =
        [
            new ChartSeries<double>
            {
                Name = "Repos",
                Data = langs.Select(g => (double)g.Count()).ToArray(),
            },
        ];
    }
}