namespace AtcWeb.Domain.Compliance;

public static class ComplianceFilterEngine
{
    public static IReadOnlyList<RepositoryComplianceSummary> Apply(
        IEnumerable<RepositoryComplianceSummary> source,
        ComplianceFilterState state)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(state);

        var query = source.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(state.SearchText))
        {
            var needle = state.SearchText.Trim();
            query = query.Where(s =>
                s.Name.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                (s.Description ?? string.Empty).Contains(needle, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(state.Language))
        {
            query = query.Where(s => string.Equals(s.Language, state.Language, StringComparison.Ordinal));
        }

        if (state.Health is { } targetHealth)
        {
            query = query.Where(s => ComplianceHealth.Compute(s) == targetHealth);
        }

        if (state.FailingSignals.Count > 0)
        {
            query = query.Where(s => state.FailingSignals.All(key => SignalIsFailing(s, key)));
        }

        return query.OrderBy(s => s.Name, StringComparer.Ordinal).ToList();
    }

    private static bool SignalIsFailing(
        RepositoryComplianceSummary s,
        string key)
        => key switch
    {
        "ReadmeMissing" => !s.Signals.HasGoodReadme,
        "LicenseWrong" => !s.Signals.LicenseIsMit,
        "HomepageWrong" => !s.Signals.HomepageIsAtcWeb,
        "EditorConfigBehind" => !s.Signals.EditorConfigStatus.RootIsLatest
                             || !s.Signals.EditorConfigStatus.SrcIsLatest
                             || !s.Signals.EditorConfigStatus.TestIsLatest,
        "UpdaterBehind" => !s.Signals.UpdaterPresent || !s.Signals.UpdaterTargetIsLatest,
        "LangVersionBehind" => !s.Signals.GlobalLangVersionIsLatest,
        "TfmBehind" => !s.Signals.GlobalTargetFrameworkIsLatest,
        "XunitNotV3" => s.Signals.XunitV3Status == XunitV3Status.No,
        "WorkflowsBehind" => !s.Signals.WorkflowsStatus.CheckoutIsLatest
                          || !s.Signals.WorkflowsStatus.SetupDotnetIsLatest
                          || !s.Signals.WorkflowsStatus.DotnetVersionIsLatest
                          || s.Signals.WorkflowsStatus.HasJavaSetup,
        "NoReleasePlease" => !s.Signals.ReleasePleasePresent,
        _ => false,
    };
}