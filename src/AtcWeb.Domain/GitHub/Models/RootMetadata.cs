namespace AtcWeb.Domain.GitHub.Models;

public class RootMetadata
{
    public string RawReadme { get; set; } = string.Empty;

    public bool HasReadme => !string.IsNullOrEmpty(RawReadme);
}