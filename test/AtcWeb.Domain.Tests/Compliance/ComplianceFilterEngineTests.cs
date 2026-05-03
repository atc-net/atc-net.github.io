namespace AtcWeb.Domain.Tests.Compliance;

public sealed class ComplianceFilterEngineTests
{
    [Fact]
    public void Apply_ReturnsAll_WhenStateIsEmpty()
    {
        var data = new[] { Make("atc-rest", "C#"), Make("atc-foo", "Python") };
        ComplianceFilterEngine.Apply(data, new ComplianceFilterState()).Should().HaveCount(2);
    }

    [Fact]
    public void Apply_FiltersByLanguage()
    {
        var data = new[] { Make("a", "C#"), Make("b", "Python") };
        var state = new ComplianceFilterState { Language = "C#" };
        ComplianceFilterEngine.Apply(data, state).Should().ContainSingle(s => s.Name == "a");
    }

    [Fact]
    public void Apply_FiltersBySearchText_OnNameAndDescription()
    {
        var data = new[]
        {
            Make("atc-rest-api-generator", "C#", description: "Generates REST APIs"),
            Make("atc-iot", "C#", description: "IoT helpers"),
        };
        var state = new ComplianceFilterState { SearchText = "rest" };
        ComplianceFilterEngine.Apply(data, state)
            .Should().ContainSingle(s => s.Name.StartsWith("atc-rest", StringComparison.Ordinal));
    }

    [Fact]
    public void Apply_FiltersByHealth()
    {
        var ok = Make("ok", "C#");
        ok.Signals.LicenseIsMit = true;
        var bad = Make("bad", "C#");
        bad.Signals.LicenseIsMit = false;
        var state = new ComplianceFilterState { Health = HealthStatus.Error };
        ComplianceFilterEngine.Apply(new[] { ok, bad }, state)
            .Should().ContainSingle(s => s.Name == "bad");
    }

    [Fact]
    public void Apply_FiltersByFailingSignal_TfmBehind()
    {
        var ok = Make("ok", "C#");
        ok.Signals.GlobalTargetFrameworkIsLatest = true;
        var stale = Make("stale", "C#");
        stale.Signals.GlobalTargetFrameworkIsLatest = false;
        var state = new ComplianceFilterState { FailingSignals = ["TfmBehind"] };
        ComplianceFilterEngine.Apply(new[] { ok, stale }, state)
            .Should().ContainSingle(s => s.Name == "stale");
    }

    private static RepositoryComplianceSummary Make(
        string name,
        string language,
        string? description = null)
        => new()
        {
            Name = name,
            Language = language,
            LicenseKey = "mit",
            Description = description,
            Signals = new RepositoryComplianceSignals
            {
                LicenseIsMit = true,
                EditorConfigStatus = new EditorConfigStatus(),
                WorkflowsStatus = new WorkflowsStatus(),
            },
        };
}