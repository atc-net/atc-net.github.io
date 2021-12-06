using System.Collections.Generic;
using Atc.Data.Models;

namespace AtcWeb.Domain.GitHub.Models
{
    public class DotnetProject
    {
        public string RawCsproj { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string TargetFramework { get; set; } = string.Empty;

        public string LangVersion { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public bool IsPackage { get; set; }

        public List<DotnetNugetPackageVersionExtended> PackageReferences { get; set; } = new List<DotnetNugetPackageVersionExtended>();
    }
}