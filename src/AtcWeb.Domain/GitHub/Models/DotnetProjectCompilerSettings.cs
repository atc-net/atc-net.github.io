using AtcWeb.Domain.Data;

namespace AtcWeb.Domain.GitHub.Models
{
    public class DotnetProjectCompilerSettings
    {
        public string TargetFramework { get; set; } = string.Empty;

        public string LangVersion { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public bool GenerateDocumentationFile { get; set; }

        public bool IsPackage { get; set; }

        public bool IsTargetFrameworkInLongTimeSupport => RepositoryMetadata.IsTargetFrameworkInLongTimeSupport(TargetFramework);

        public bool IsLangVersionInAcceptedVersion => RepositoryMetadata.IsLangVersionInAcceptedVersion(LangVersion);
    }
}