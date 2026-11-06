namespace AtcWeb.Helpers;

/// <summary>
/// The numbers shown on the home page and in the world banners, so every page counts the same way.
/// </summary>
public static class SiteStatsHelper
{
    /// <summary>
    /// Splits the atc-net NuGet packages into libraries and CLI tools.
    /// </summary>
    /// <param name="packageIds">The ids of all atc-net packages on NuGet.</param>
    /// <param name="cliToolIds">The ids of the packages that are CLI tools.</param>
    /// <returns>The number of libraries and the number of CLI tools.</returns>
    public static (int Libraries, int CliTools) CountPackages(
        IEnumerable<string> packageIds,
        IEnumerable<string> cliToolIds)
    {
        ArgumentNullException.ThrowIfNull(packageIds);
        ArgumentNullException.ThrowIfNull(cliToolIds);

        var toolIds = cliToolIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var ids = packageIds.ToList();
        var cliTools = ids.Count(toolIds.Contains);
        return (ids.Count - cliTools, cliTools);
    }

    /// <summary>
    /// Splits the apps into those with a release and those still coming soon.
    /// </summary>
    /// <param name="apps">The app cards.</param>
    /// <returns>The number of apps ready to download and the number coming soon.</returns>
    public static (int Ready, int ComingSoon) CountAtcApps(
        IEnumerable<AtcAppCardModel> apps)
    {
        ArgumentNullException.ThrowIfNull(apps);

        var list = apps.ToList();
        var comingSoon = list.Count(x => x.IsComingSoon);
        return (list.Count - comingSoon, comingSoon);
    }
}