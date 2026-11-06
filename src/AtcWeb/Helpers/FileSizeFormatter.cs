namespace AtcWeb.Helpers;

public static class FileSizeFormatter
{
    private const double KiloByte = 1024;
    private const double MegaByte = KiloByte * 1024;
    private const double GigaByte = MegaByte * 1024;

    /// <summary>
    /// Readable file size rounded to whole units, e.g. "145 MB", "2 KB" or "512 B".
    /// </summary>
    /// <param name="bytes">The size in bytes.</param>
    /// <returns>The formatted size.</returns>
    public static string Format(long bytes)
        => bytes switch
        {
            >= (long)GigaByte => $"{bytes / GigaByte:0.#} GB",
            >= (long)MegaByte => $"{Round(bytes / MegaByte)} MB",
            >= (long)KiloByte => $"{Round(bytes / KiloByte)} KB",
            _ => $"{bytes} B",
        };

    private static double Round(double value)
        => Math.Round(value, MidpointRounding.AwayFromZero);
}