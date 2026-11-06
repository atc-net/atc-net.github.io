namespace AtcWeb.Models;

/// <summary>
/// Everything a component card on /components needs.
/// </summary>
/// <param name="InstallCommand">The install command for the primary package.</param>
/// <param name="IsCliTool">Whether the primary package is a CLI tool.</param>
/// <param name="LibraryInstallCommand">The install command for the main library package, if the repository has one.</param>
/// <param name="ToolInstallCommand">The install command for the CLI tool, if the repository has one (it may also have libraries).</param>
public sealed record ComponentCardModel(
    string Name,
    string? Description,
    string? Language,
    string Category,
    long Downloads,
    DateTimeOffset? UpdatedAt,
    string? InstallCommand,
    bool IsCliTool,
    string? LibraryInstallCommand,
    string? ToolInstallCommand)
{
    public bool HasPackage
        => InstallCommand is not null;

    public bool HasLibrary
        => LibraryInstallCommand is not null;

    public bool HasCliTool
        => ToolInstallCommand is not null;

    /// <param name="repository">The component repository.</param>
    /// <param name="packages">Its NuGet packages, primary package first (see RepositoryNugetPackageHelper).</param>
    /// <returns>The card model.</returns>
    public static ComponentCardModel From(
        AtcRepository repository,
        IReadOnlyList<RepositoryNugetPackage> packages)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(packages);

        var primary = packages.Count > 0 ? packages[0] : null;
        var library = packages.FirstOrDefault(x => !x.IsTool);
        var tool = packages.FirstOrDefault(x => x.IsTool);

        return new ComponentCardModel(
            repository.Name,
            string.IsNullOrWhiteSpace(repository.Description) ? null : repository.Description,
            repository.BaseData.Language,
            RepositoryCategoryHelper.GetCategory(repository.Name),
            packages.Sum(x => x.TotalDownloads),
            repository.BaseData.PushedAt,
            GetInstallCommand(primary),
            IsCliTool: primary?.IsTool == true,
            GetInstallCommand(library),
            GetInstallCommand(tool));
    }

    private static string? GetInstallCommand(RepositoryNugetPackage? package)
        => package switch
        {
            null => null,
            { IsTool: true } => $"dotnet tool install -g {package.PackageId}",
            _ => $"dotnet add package {package.PackageId}",
        };
}