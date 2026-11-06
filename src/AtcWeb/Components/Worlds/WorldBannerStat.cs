namespace AtcWeb.Components.Worlds;

public static class WorldBannerStat
{
    /// <summary>
    /// A banner number, or "--" while it is still loading.
    /// </summary>
    /// <param name="value">The number.</param>
    /// <returns>The formatted number.</returns>
    public static string Format(int value)
        => value > 0
            ? value.ToString(CultureInfo.InvariantCulture)
            : "--";
}