using System;

namespace AtcWeb.Domain.GitHub.Models
{
    public class DotnetNugetPackageVersion
    {
        public DotnetNugetPackageVersion(string packageId, Version version)
        {
            this.PackageId = packageId;
            this.Version = version;
        }

        public string PackageId { get; }

        public Version Version { get; }

        public override string ToString()
            => $"{nameof(PackageId)}: {PackageId}, {nameof(Version)}: {Version}";
    }
}