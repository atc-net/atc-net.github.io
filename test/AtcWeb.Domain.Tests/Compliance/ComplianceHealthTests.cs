namespace AtcWeb.Domain.Tests.Compliance;

public sealed class ComplianceHealthTests
{
    [Fact]
    public void Compute_ReturnsOk_WhenAllGreen()
    {
        ComplianceHealth.Compute(AllGreenSummary()).Should().Be(HealthStatus.Ok);
    }

    [Fact]
    public void Compute_ReturnsError_WhenLicenseNotMit()
    {
        var s = AllGreenSummary();
        s.Signals.LicenseIsMit = false;
        ComplianceHealth.Compute(s).Should().Be(HealthStatus.Error);
    }

    [Fact]
    public void Compute_ReturnsError_WhenJavaSetupPresent()
    {
        var s = AllGreenSummary();
        s.Signals.WorkflowsStatus.HasJavaSetup = true;
        ComplianceHealth.Compute(s).Should().Be(HealthStatus.Error);
    }

    [Fact]
    public void Compute_ReturnsWarning_WhenReadmeMissingButNoErrors()
    {
        var s = AllGreenSummary();
        s.Signals.HasGoodReadme = false;
        ComplianceHealth.Compute(s).Should().Be(HealthStatus.Warning);
    }

    private static RepositoryComplianceSummary AllGreenSummary() => new()
    {
        Name = "atc-test",
        Language = "C#",
        LicenseKey = "mit",
        Signals = new RepositoryComplianceSignals
        {
            HasGoodReadme = true,
            LicenseIsMit = true,
            HomepageIsAtcWeb = true,
            UpdaterPresent = true,
            UpdaterTargetIsLatest = true,
            GlobalLangVersionIsLatest = true,
            GlobalTargetFrameworkIsLatest = true,
            XunitV3Status = XunitV3Status.Yes,
            ReleasePleasePresent = true,
            EditorConfigStatus = new EditorConfigStatus
            {
                RootPresent = true,
                RootIsLatest = true,
                SrcPresent = true,
                SrcIsLatest = true,
                TestPresent = true,
                TestIsLatest = true,
            },
            WorkflowsStatus = new WorkflowsStatus
            {
                CheckoutIsLatest = true,
                SetupDotnetIsLatest = true,
                DotnetVersionIsLatest = true,
                HasJavaSetup = false,
            },
        },
    };
}