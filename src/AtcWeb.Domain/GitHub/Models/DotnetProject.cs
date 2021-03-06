namespace AtcWeb.Domain.GitHub.Models;

public class DotnetProject
{
    public string RawCsproj { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string RawReadme { get; set; } = string.Empty;

    public bool HasReadme => !string.IsNullOrEmpty(RawReadme);

    public DotnetProjectCompilerSettings CompilerSettings { get; set; } = new DotnetProjectCompilerSettings();

    public DotnetProjectAnalyzerSettings AnalyzerSettings { get; set; } = new DotnetProjectAnalyzerSettings();

    public List<DotnetNugetPackage> PackageReferences { get; set; } = new List<DotnetNugetPackage>();
}