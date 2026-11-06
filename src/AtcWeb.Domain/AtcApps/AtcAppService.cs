namespace AtcWeb.Domain.AtcApps;

/// <summary>
/// Page-facing facade over <see cref="AtcApiAppClient"/>: returns data, or empty/null when unavailable.
/// </summary>
public class AtcAppService
{
    private readonly AtcApiAppClient client;

    public AtcAppService(AtcApiAppClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        this.client = client;
    }

    public async Task<List<AtcApp>> GetAtcAppsAsync(
        CancellationToken cancellationToken = default)
        => (await client.GetAtcApps(cancellationToken)).Data;

    public async Task<AtcApp?> GetAtcAppAsync(
        string name,
        CancellationToken cancellationToken = default)
        => (await client.GetAtcAppByName(name, cancellationToken)).Data;

    public async Task<List<AtcAppRelease>> GetReleasesAsync(
        string name,
        CancellationToken cancellationToken = default)
        => (await client.GetReleases(name, cancellationToken)).Data;

    public async Task<string> GetReadmeAsync(
        string name,
        CancellationToken cancellationToken = default)
        => (await client.GetReadme(name, cancellationToken)).Data;

    public async Task<List<AtcAppComponentReference>> GetBuiltWithAsync(
        string name,
        CancellationToken cancellationToken = default)
        => (await client.GetBuiltWith(name, cancellationToken)).Data;

    public async Task<WikiMetadata> GetWikiAsync(
        string name,
        CancellationToken cancellationToken = default)
        => (await client.GetWiki(name, cancellationToken)).Data;
}