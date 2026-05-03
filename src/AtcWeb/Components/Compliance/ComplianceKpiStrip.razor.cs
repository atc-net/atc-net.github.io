namespace AtcWeb.Components.Compliance;

public partial class ComplianceKpiStrip : ComponentBase
{
    [Parameter]
    public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];

    private int Total => Summaries.Count;

    private int CountByLanguage(string lang)
        => Summaries.Count(s => string.Equals(s.Language, lang, StringComparison.Ordinal));

    private string PercentText(
        Func<RepositoryComplianceSummary, bool> predicate)
    {
        if (Summaries.Count == 0)
        {
            return "0%";
        }

        var n = Summaries.Count(predicate);
        var pct = n * 100 / Summaries.Count;
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{pct}% ({n})");
    }
}