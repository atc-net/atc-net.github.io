namespace AtcWeb.Domain.AtcApi.Models;

public class GitHubLicenseMetadata
{
    public string Key { get; set; }

    public override string ToString() => $"{nameof(Key)}: {Key}";
}