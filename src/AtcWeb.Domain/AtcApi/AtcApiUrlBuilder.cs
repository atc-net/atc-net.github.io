namespace AtcWeb.Domain.AtcApi;

public static class AtcApiUrlBuilder
{
    private static readonly HashSet<string> AllowedExternalProxyOwners = new(StringComparer.OrdinalIgnoreCase)
    {
        "atc-net",
        "microsoft",
        "dotnet",
        "Azure",
        "github",
    };

    [SuppressMessage("Design", "CA1055:URI-like return values should not be strings", Justification = "Consumed directly as an HTML attribute value.")]
    public static string BuildRawFileUrl(
        string repositoryName,
        string branch,
        string filePath)
        => $"{AtcApiConstants.BaseAddress}/github/repository/{Uri.EscapeDataString(repositoryName)}/raw?branch={Uri.EscapeDataString(branch)}&filePath={Uri.EscapeDataString(filePath)}";

    [SuppressMessage("Design", "CA1055:URI-like return values should not be strings", Justification = "Consumed directly as an HTML attribute value.")]
    public static string BuildExternalRawFileUrl(
        string owner,
        string repositoryName,
        string branch,
        string filePath)
        => $"{AtcApiConstants.BaseAddress}/github/raw/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repositoryName)}?branch={Uri.EscapeDataString(branch)}&filePath={Uri.EscapeDataString(filePath)}";

    [SuppressMessage("Design", "CA1055:URI-like return values should not be strings", Justification = "Consumed directly as an HTML attribute value.")]
    public static string BuildAvatarUrl(
        long userId,
        int? size = null)
        => size.HasValue
            ? $"{AtcApiConstants.BaseAddress}/github/users/{userId}/avatar?size={size.Value}"
            : $"{AtcApiConstants.BaseAddress}/github/users/{userId}/avatar";

    public static bool IsExternalProxyOwner(string owner)
        => AllowedExternalProxyOwners.Contains(owner);
}