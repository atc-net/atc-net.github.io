namespace AtcWeb.Helpers;

/// <summary>
/// Describes what a project or an app ships, e.g. "3 libraries · 1 CLI tool" or "4 downloads".
/// </summary>
public static class ShipsFormatter
{
    public static string? ForRepository(
        int libraries,
        int cliTools)
    {
        var parts = new List<string>(2);
        if (libraries > 0)
        {
            parts.Add(libraries == 1 ? "1 library" : $"{libraries} libraries");
        }

        if (cliTools > 0)
        {
            parts.Add(cliTools == 1 ? "1 CLI tool" : $"{cliTools} CLI tools");
        }

        return parts.Count == 0
            ? null
            : string.Join(" · ", parts);
    }

    public static string? ForAtcApp(int downloads)
        => downloads switch
        {
            <= 0 => null,
            1 => "1 download",
            _ => $"{downloads} downloads",
        };
}