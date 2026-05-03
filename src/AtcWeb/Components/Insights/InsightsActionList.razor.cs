namespace AtcWeb.Components.Insights;

public partial class InsightsActionList : ComponentBase
{
    [Parameter]
    public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];

    protected sealed record ActionItem(
        RepositoryComplianceSummary Repo,
        HealthStatus Health,
        IReadOnlyList<string> FailingRules);

    protected sealed record ActionGroup(
        string Category,
        IReadOnlyList<ActionItem> Items);

    protected List<ActionGroup> Groups { get; private set; } = [];

    protected override void OnParametersSet()
    {
        var items = Summaries
            .Select(s => new
            {
                Repo = s,
                Health = ComplianceHealth.Compute(s),
                Rules = ComputeFailingRules(s),
            })
            .Where(x => x.Health != HealthStatus.Ok)
            .Select(x => new ActionItem(x.Repo, x.Health, x.Rules))
            .ToList();

        Groups = items
            .GroupBy(i => RepositoryCategoryHelper.GetCategory(i.Repo.Name), StringComparer.Ordinal)
            .OrderBy(g => RepositoryCategoryHelper.GetSortOrder(g.Key))
            .Select(g => new ActionGroup(g.Key, g.OrderBy(x => x.Repo.Name, StringComparer.Ordinal).ToList()))
            .ToList();
    }

    private static IReadOnlyList<string> ComputeFailingRules(
        RepositoryComplianceSummary s)
    {
        var rules = new List<string>();
        if (!s.Signals.LicenseIsMit)
        {
            rules.Add("License not MIT");
        }

        if (s.Signals.WorkflowsStatus.HasJavaSetup)
        {
            rules.Add("setup-java in workflow");
        }

        if (!s.Signals.HasGoodReadme)
        {
            rules.Add("README too thin");
        }

        if (!s.Signals.HomepageIsAtcWeb)
        {
            rules.Add("Homepage URL");
        }

        if (string.Equals(s.Language, "C#", StringComparison.Ordinal))
        {
            if (!s.Signals.UpdaterPresent || !s.Signals.UpdaterTargetIsLatest)
            {
                rules.Add("updater not DotNet10");
            }

            if (!s.Signals.EditorConfigStatus.RootIsLatest ||
                !s.Signals.EditorConfigStatus.SrcIsLatest ||
                !s.Signals.EditorConfigStatus.TestIsLatest)
            {
                rules.Add(".editorconfig behind");
            }

            if (!s.Signals.GlobalLangVersionIsLatest)
            {
                rules.Add("LangVersion behind");
            }

            if (!s.Signals.GlobalTargetFrameworkIsLatest)
            {
                rules.Add("TFM not net10.0");
            }

            if (s.Signals.XunitV3Status == XunitV3Status.No)
            {
                rules.Add("xUnit not v3");
            }

            if (!s.Signals.WorkflowsStatus.CheckoutIsLatest)
            {
                rules.Add("checkout < v6");
            }

            if (!s.Signals.WorkflowsStatus.SetupDotnetIsLatest)
            {
                rules.Add("setup-dotnet < v5");
            }

            if (!s.Signals.WorkflowsStatus.DotnetVersionIsLatest)
            {
                rules.Add(".NET version < 10");
            }

            if (!s.Signals.ReleasePleasePresent)
            {
                rules.Add("no release-please");
            }
        }

        return rules;
    }

    private static string StatusIcon(HealthStatus h) => h switch
    {
        HealthStatus.Error => Icons.Material.Filled.Error,
        HealthStatus.Warning => Icons.Material.Filled.Warning,
        _ => Icons.Material.Filled.CheckCircle,
    };

    private static Color StatusColor(HealthStatus h) => h switch
    {
        HealthStatus.Error => Color.Error,
        HealthStatus.Warning => Color.Warning,
        _ => Color.Success,
    };
}