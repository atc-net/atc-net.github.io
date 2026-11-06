namespace AtcWeb.Domain.AtcApi.Models;

public class AtcAppComponentReference
{
    public string PackageId { get; set; } = string.Empty;

    /// <summary>
    /// The atc-net repository that publishes the package.
    /// </summary>
    public string RepositoryName { get; set; } = string.Empty;
}