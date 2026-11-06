namespace AtcWeb.Domain.AtcApi.Models;

public class AtcAppAsset
{
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Friendly label derived from the file name, e.g. "Camera Wall".
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Lower-case extension without the dot, e.g. "msi".
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// True for helper files such as checksums and config files.
    /// </summary>
    public bool IsSecondary { get; set; }

    public long Size { get; set; }

    public int DownloadCount { get; set; }

    public string DownloadUrl { get; set; } = string.Empty;
}