namespace AtcWeb.Domain.GitHub.Models
{
    public class DotnetProjectAnalyzerSettings
    {
        public string AnalysisMode { get; set; } = string.Empty;

        public string AnalysisLevel { get; set; } = string.Empty;

        public bool EnableNetAnalyzers { get; set; }

        public bool EnforceCodeStyleInBuild { get; set; }
    }
}