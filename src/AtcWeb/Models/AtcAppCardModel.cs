namespace AtcWeb.Models;

/// <summary>
/// Everything an app card needs, derived from a <see cref="AtcApp"/>.
/// </summary>
[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Consumed directly as HTML attribute values.")]
[SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Consumed directly as HTML attribute values.")]
public sealed record AtcAppCardModel(
    string Name,
    string DisplayName,
    string? Pitch,
    string HeroImageUrl,
    bool HasScreenshot,
    string? Version,
    DateTimeOffset? UpdatedAt,
    bool IsComingSoon,
    string DownloadHref,
    bool IsDirectDownload,
    int DownloadCount,
    IReadOnlyList<string> Topics)
{
    public static AtcAppCardModel From(AtcApp app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var release = app.LatestRelease;
        var primaryAssets = release?.Assets
            .Where(x => !x.IsSecondary)
            .ToList() ?? [];

        var isDirectDownload = primaryAssets.Count == 1;
        var downloadHref = isDirectDownload
            ? primaryAssets[0].DownloadUrl
            : $"/apps/{app.Name}#download";

        var heroImageUrl = app.Screenshots.Count > 0
            ? AtcApiUrlBuilder.ResolveAtcAppImageUrl(app, app.Screenshots[0])
            : app.SocialImageUrl;

        return new AtcAppCardModel(
            app.Name,
            app.DisplayName,
            app.Description,
            heroImageUrl,
            HasScreenshot: app.Screenshots.Count > 0,
            release?.TagName,
            release?.PublishedAt ?? app.PushedAt,
            IsComingSoon: release is null,
            downloadHref,
            isDirectDownload,
            primaryAssets.Count,
            app.Topics);
    }
}