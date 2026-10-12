namespace AtcWeb.Domain.GitHub;

public static class RepositoryNugetPackageHelper
{
    public const string OwnerSearchQuery = "owner:atc-net";

    private const string GitHubOrganization = "atc-net";

    /// <summary>
    /// Finds the NuGet packages published from a repository, matched by the package project URL.
    /// The primary package comes first, followed by libraries and then tools, most downloaded first.
    /// </summary>
    public static List<RepositoryNugetPackage> GetPackagesForRepository(
        string repositoryName,
        string repositoryDotName,
        IEnumerable<NugetPackageSearchItem> searchItems,
        ICollection<string> toolPackageIds)
    {
        ArgumentNullException.ThrowIfNull(searchItems);
        ArgumentNullException.ThrowIfNull(toolPackageIds);

        var packages = searchItems
            .Where(x => repositoryName.Equals(GetRepositoryNameFromProjectUrl(x.ProjectUrl), StringComparison.OrdinalIgnoreCase))
            .Select(x => new RepositoryNugetPackage(
                x.Id,
                x.Version,
                x.Description,
                x.TotalDownloads,
                toolPackageIds.Contains(x.Id)))
            .OrderBy(x => x.IsTool)
            .ThenByDescending(x => x.TotalDownloads)
            .ThenBy(x => x.PackageId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var primary = GetPrimaryPackage(repositoryDotName, packages);
        if (primary is not null)
        {
            packages.Remove(primary);
            packages.Insert(0, primary);
        }

        return packages;
    }

    /// <summary>
    /// Total NuGet downloads per repository name, summed across all of its packages.
    /// </summary>
    public static Dictionary<string, long> GetTotalDownloadsByRepository(
        IEnumerable<NugetPackageSearchItem> searchItems)
    {
        ArgumentNullException.ThrowIfNull(searchItems);

        return searchItems
            .Select(x => (RepositoryName: GetRepositoryNameFromProjectUrl(x.ProjectUrl), x.TotalDownloads))
            .Where(x => x.RepositoryName is not null)
            .GroupBy(x => x.RepositoryName!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalDownloads), StringComparer.OrdinalIgnoreCase);
    }

    [SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "NuGet returns the project URL as a free-form string that may be missing or invalid.")]
    [SuppressMessage("Design", "CA1055:URI-like return values should not be strings", Justification = "Returns a repository name, not a URL.")]
    public static string? GetRepositoryNameFromProjectUrl(string? projectUrl)
    {
        if (!Uri.TryCreate(projectUrl, UriKind.Absolute, out var uri) ||
            !(uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) ||
              uri.Host.Equals("www.github.com", StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2 ||
            !segments[0].Equals(GitHubOrganization, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var name = segments[1];
        return name.EndsWith(".git", StringComparison.OrdinalIgnoreCase)
            ? name[..^4]
            : name;
    }

    private static RepositoryNugetPackage? GetPrimaryPackage(
        string repositoryDotName,
        List<RepositoryNugetPackage> packages)
    {
        if (packages.Count == 0)
        {
            return null;
        }

        var nameMatches = packages
            .Where(x => x.PackageId.Equals(repositoryDotName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return nameMatches.Find(x => !x.IsTool)
               ?? nameMatches.FirstOrDefault()
               ?? packages.Find(x => !x.IsTool)
               ?? packages[0];
    }
}