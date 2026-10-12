namespace AtcWeb.Components.Repository;

public partial class RepositoryInstallation : ComponentBase
{
    /// <summary>
    /// Above this many packages the pills turn into a dropdown, so large repositories stay compact.
    /// </summary>
    private const int MaxPackagePills = 5;

    private string? selectedPackageId;

    [Parameter]
    public IReadOnlyList<RepositoryNugetPackage> Packages { get; set; } = [];

    private RepositoryNugetPackage? SelectedPackage
        => Packages.FirstOrDefault(x => x.PackageId.Equals(selectedPackageId, StringComparison.Ordinal))
           ?? (Packages.Count > 0 ? Packages[0] : null);

    private void SelectPackage(string packageId)
        => selectedPackageId = packageId;

    private static IEnumerable<(string Label, string Icon, string Code)> GetSnippets(
        RepositoryNugetPackage package)
    {
        if (package.IsTool)
        {
            yield return ("Global tool", Icons.Material.Filled.Terminal, $"dotnet tool install -g {package.PackageId} --version {package.Version}");
            yield return ("Local tool (tool manifest)", Icons.Material.Filled.InsertDriveFile, $"dotnet tool install --local {package.PackageId} --version {package.Version}");
            yield break;
        }

        yield return ("Terminal", Icons.Material.Filled.Terminal, $"dotnet add package {package.PackageId} --version {package.Version}");
        yield return ("Project Reference", Icons.Material.Filled.InsertDriveFile, $"""<PackageReference Include="{package.PackageId}" Version="{package.Version}" />""");
    }
}