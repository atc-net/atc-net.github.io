namespace AtcWeb.State;

public static class SiteWorldHelper
{
    public static SiteWorld FromRelativePath(string relativePath)
    {
        ArgumentNullException.ThrowIfNull(relativePath);

        var path = relativePath.Split('?')[0].Split('#')[0];
        var firstSegment = path.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

        return firstSegment.ToLowerInvariant() switch
        {
            "apps" => SiteWorld.AtcApps,
            "libraries" or "components" or "repository" => SiteWorld.Libraries,
            _ => SiteWorld.None,
        };
    }
}