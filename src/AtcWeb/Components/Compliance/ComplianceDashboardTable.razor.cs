namespace AtcWeb.Components.Compliance;

public partial class ComplianceDashboardTable : ComponentBase
{
    [Parameter]
    public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];

    private static string Bool(bool ok) => ok ? "✓" : "✗";

    private static ChipState UpdaterState(RepositoryComplianceSummary s)
    {
        if (!s.Signals.UpdaterPresent)
        {
            return ChipState.Warning;
        }

        return s.Signals.UpdaterTargetIsLatest ? ChipState.Ok : ChipState.Warning;
    }

    private static ChipState XunitState(XunitV3Status status) => status switch
    {
        XunitV3Status.Yes => ChipState.Ok,
        XunitV3Status.No => ChipState.Warning,
        _ => ChipState.NotApplicable,
    };

    private static ChipState WorkflowsState(WorkflowsStatus w)
    {
        if (w.HasJavaSetup)
        {
            return ChipState.Error;
        }

        if (!w.CheckoutIsLatest || !w.SetupDotnetIsLatest || !w.DotnetVersionIsLatest)
        {
            return ChipState.Warning;
        }

        return ChipState.Ok;
    }

    private static string WorkflowsLabel(WorkflowsStatus w)
    {
        if (w.HasJavaSetup)
        {
            return "Java!";
        }

        if (!w.CheckoutIsLatest)
        {
            return "co<v6";
        }

        if (!w.SetupDotnetIsLatest)
        {
            return "sd<v5";
        }

        if (!w.DotnetVersionIsLatest)
        {
            return ".NET<10";
        }

        return "OK";
    }
}