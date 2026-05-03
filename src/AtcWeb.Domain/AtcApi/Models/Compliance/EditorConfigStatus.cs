namespace AtcWeb.Domain.AtcApi.Models.Compliance;

public sealed class EditorConfigStatus
{
    public bool RootPresent { get; set; }

    public bool RootIsLatest { get; set; }

    public string? RootVersion { get; set; }

    public bool SrcPresent { get; set; }

    public bool SrcIsLatest { get; set; }

    public string? SrcVersion { get; set; }

    public bool TestPresent { get; set; }

    public bool TestIsLatest { get; set; }

    public string? TestVersion { get; set; }
}