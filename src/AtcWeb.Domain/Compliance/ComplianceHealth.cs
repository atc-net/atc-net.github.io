namespace AtcWeb.Domain.Compliance;

public enum HealthStatus
{
    Ok,
    Warning,
    Error,
}

public static class ComplianceHealth
{
    public static HealthStatus Compute(RepositoryComplianceSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        var s = summary.Signals;
        var isDotnet = string.Equals(summary.Language, "C#", StringComparison.Ordinal);

        if (!s.LicenseIsMit)
        {
            return HealthStatus.Error;
        }

        if (s.WorkflowsStatus.HasJavaSetup)
        {
            return HealthStatus.Error;
        }

        if (isDotnet && !s.GlobalTargetFrameworkIsLatest && !string.IsNullOrEmpty(s.GlobalTargetFramework))
        {
            return HealthStatus.Error;
        }

        if (!s.HasGoodReadme)
        {
            return HealthStatus.Warning;
        }

        if (!s.HomepageIsAtcWeb)
        {
            return HealthStatus.Warning;
        }

        if (isDotnet)
        {
            if (!s.UpdaterPresent || !s.UpdaterTargetIsLatest)
            {
                return HealthStatus.Warning;
            }

            if (!s.EditorConfigStatus.RootIsLatest ||
                !s.EditorConfigStatus.SrcIsLatest ||
                !s.EditorConfigStatus.TestIsLatest)
            {
                return HealthStatus.Warning;
            }

            if (!s.GlobalLangVersionIsLatest)
            {
                return HealthStatus.Warning;
            }

            if (s.XunitV3Status == XunitV3Status.No)
            {
                return HealthStatus.Warning;
            }

            if (!s.WorkflowsStatus.CheckoutIsLatest ||
                !s.WorkflowsStatus.SetupDotnetIsLatest ||
                !s.WorkflowsStatus.DotnetVersionIsLatest)
            {
                return HealthStatus.Warning;
            }

            if (!s.ReleasePleasePresent)
            {
                return HealthStatus.Warning;
            }
        }

        return HealthStatus.Ok;
    }
}