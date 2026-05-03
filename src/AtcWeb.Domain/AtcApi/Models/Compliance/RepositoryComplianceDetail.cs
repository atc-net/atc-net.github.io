namespace AtcWeb.Domain.AtcApi.Models.Compliance;

public sealed class RepositoryComplianceDetail
{
    public List<string> SrcFrameworks { get; init; } = [];

    public List<string> TestFrameworks { get; init; } = [];

    public List<string> SampleFrameworks { get; init; } = [];

    public List<AnalyzerPackageRef> AnalyzerPackages { get; init; } = [];

    public List<string> SuppressedRulesRoot { get; init; } = [];

    public List<string> SuppressedRulesSrc { get; init; } = [];

    public List<string> SuppressedRulesTest { get; init; } = [];
}