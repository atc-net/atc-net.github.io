namespace AtcWeb.Components.Compliance;

public partial class ComplianceDashboardRowDetail : ComponentBase
{
    [Parameter]
    public RepositoryComplianceSummary? Summary { get; set; }

    private static string JoinOrDash(IReadOnlyList<string> values)
        => values.Count == 0 ? "–" : string.Join(", ", values);

    private static string FormatDate(DateTimeOffset? d)
        => d?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "–";
}