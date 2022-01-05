using System.Collections.Generic;

namespace AtcWeb.Domain.GitHub.Models
{
    public class DotnetProject
    {
        public string RawCsproj { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public DotnetProjectCompilerSettings CompilerSettings { get; set; } = new DotnetProjectCompilerSettings();

        public DotnetProjectAnalyzerSettings AnalyzerSettings { get; set; } = new DotnetProjectAnalyzerSettings();

        public bool IsPackage { get; set; }

        public List<DotnetNugetPackageVersionExtended> PackageReferences { get; set; } = new List<DotnetNugetPackageVersionExtended>();
    }
}