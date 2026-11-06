namespace AtcWeb.Domain.AtcApi;

public static class AtcApiUrlBuilder
{
    private static readonly HashSet<string> AllowedExternalProxyOwners = new(StringComparer.OrdinalIgnoreCase)
    {
        "atc-net",
        "atc-net-apps",
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

    /// <summary>
    /// Turns an app screenshot reference into an image URL. Repository-relative paths go through
    /// the atc-api image proxy; absolute URLs are used as-is.
    /// </summary>
    /// <param name="app">The app the image belongs to.</param>
    /// <param name="imageReference">An absolute URL or a repository-relative path.</param>
    /// <returns>The image URL.</returns>
    [SuppressMessage("Design", "CA1055:URI-like return values should not be strings", Justification = "Consumed directly as an HTML attribute value.")]
    public static string ResolveAtcAppImageUrl(
        AtcApp app,
        string imageReference)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(imageReference);

        return imageReference.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               imageReference.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? imageReference
            : BuildExternalRawFileUrl(app.Owner, app.Name, app.DefaultBranch, imageReference);
    }

    public static bool IsExternalProxyOwner(string owner)
        => AllowedExternalProxyOwners.Contains(owner);
}