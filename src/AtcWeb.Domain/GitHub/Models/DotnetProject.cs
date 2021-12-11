using System.Collections.Generic;
using AtcWeb.Domain.Data;

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

        public bool IsTargetFrameworkInLongTimeSupport => RepositoryMetadata.IsTargetFrameworkInLongTimeSupport(TargetFramework);

        public bool IsLangVersionInAcceptedVersion => RepositoryMetadata.IsLangVersionInAcceptedVersion(LangVersion);

        public List<DotnetNugetPackageVersionExtended> PackageReferences { get; set; } = new List<DotnetNugetPackageVersionExtended>();
    }
}