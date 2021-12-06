using System;

namespace AtcWeb.Domain.GitHub.Models;

public class DotnetNugetPackageVersionExtended : DotnetNugetPackageVersion
{
    public DotnetNugetPackageVersionExtended(string packageId, Version currentVersion, Version newestVersion)
        : base(packageId, currentVersion)
    {
        this.NewestVersion = newestVersion;
    }

    public Version NewestVersion { get; set; }

    public bool IsNewest => Version >= NewestVersion;
}