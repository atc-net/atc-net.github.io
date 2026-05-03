namespace AtcWeb.Components.Compliance;

public partial class ComplianceDashboardTable : ComponentBase
{
    [Parameter]
    public IReadOnlyList<RepositoryComplianceSummary> Summaries { get; set; } = [];

    private static bool IsDotnet(RepositoryComplianceSummary s)
        => string.Equals(s.Language, "C#", StringComparison.Ordinal);

    private static string Bool(bool ok) => ok ? "✓" : "✗";

    private static ChipState EditorConfigState(RepositoryComplianceSummary s)
    {
        if (!IsDotnet(s))
        {
            return ChipState.NotApplicable;
        }

        var ec = s.Signals.EditorConfigStatus;
        var latestCount =
            (ec.RootIsLatest ? 1 : 0) +
            (ec.SrcIsLatest ? 1 : 0) +
            (ec.TestIsLatest ? 1 : 0);

        return latestCount switch
        {
            3 => ChipState.Ok,
            0 => ChipState.Error,
            _ => ChipState.Warning,
        };
    }

    private static string EditorConfigLabel(RepositoryComplianceSummary s)
    {
        if (!IsDotnet(s))
        {
            return "N/A";
        }

        var ec = s.Signals.EditorConfigStatus;
        return $"{Bool(ec.RootIsLatest)}/{Bool(ec.SrcIsLatest)}/{Bool(ec.TestIsLatest)}";
    }

    private static ChipState UpdaterState(RepositoryComplianceSummary s)
    {
        if (!IsDotnet(s))
        {
            return ChipState.NotApplicable;
        }

        if (!s.Signals.UpdaterPresent)
        {
            return ChipState.Warning;
        }

        return s.Signals.UpdaterTargetIsLatest ? ChipState.Ok : ChipState.Warning;
    }

    private static string UpdaterLabel(RepositoryComplianceSummary s)
    {
        if (!IsDotnet(s))
        {
            return "N/A";
        }

        return s.Signals.UpdaterProjectTarget ?? "–";
    }

    private static ChipState LangVerState(RepositoryComplianceSummary s)
    {
        if (!IsDotnet(s))
        {
            return ChipState.NotApplicable;
        }

        return s.Signals.GlobalLangVersionIsLatest ? ChipState.Ok : ChipState.Warning;
    }

    private static string LangVerLabel(RepositoryComplianceSummary s)
    {
        if (!IsDotnet(s))
        {
            return "N/A";
        }

        return s.Signals.GlobalLangVersion ?? "–";
    }

    private static ChipState TfmState(RepositoryComplianceSummary s)
    {
        if (!IsDotnet(s))
        {
            return ChipState.NotApplicable;
        }

        return s.Signals.GlobalTargetFrameworkIsLatest ? ChipState.Ok : ChipState.Error;
    }

    private static string TfmLabel(RepositoryComplianceSummary s)
    {
        if (!IsDotnet(s))
        {
            return "N/A";
        }

        return s.Signals.GlobalTargetFramework ?? "–";
    }

    private static ChipState XunitState(XunitV3Status status) => status switch
    {
        XunitV3Status.Yes => ChipState.Ok,
        XunitV3Status.No => ChipState.Warning,
        _ => ChipState.NotApplicable,
    };

    private static ChipState WorkflowsState(RepositoryComplianceSummary s)
    {
        var w = s.Signals.WorkflowsStatus;
        if (w.HasJavaSetup)
        {
            return ChipState.Error;
        }

        if (!IsDotnet(s))
        {
            return ChipState.NotApplicable;
        }

        if (!w.CheckoutIsLatest || !w.SetupDotnetIsLatest || !w.DotnetVersionIsLatest)
        {
            return ChipState.Warning;
        }

        return ChipState.Ok;
    }

    private static string WorkflowsLabel(RepositoryComplianceSummary s)
    {
        var w = s.Signals.WorkflowsStatus;
        if (w.HasJavaSetup)
        {
            return "Java!";
        }

        if (!IsDotnet(s))
        {
            return "N/A";
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

    private static ChipState ReleasePleaseState(RepositoryComplianceSummary s)
    {
        if (!IsDotnet(s))
        {
            return ChipState.NotApplicable;
        }

        return s.Signals.ReleasePleasePresent ? ChipState.Ok : ChipState.Warning;
    }

    private static string ReleasePleaseLabel(RepositoryComplianceSummary s)
    {
        if (!IsDotnet(s))
        {
            return "N/A";
        }

        return s.Signals.ReleasePleasePresent ? "Yes" : "No";
    }
}