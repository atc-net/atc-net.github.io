namespace AtcWeb.Domain.AtcApi;

/// <summary>
/// Client for the atc-api app endpoints (repositories of the atc-net-apps organization).
/// </summary>
public class AtcApiAppClient
{
    private static readonly SemaphoreSlim SemaphoreAtcApps = new(1, 1);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> DetailLocks = new(StringComparer.Ordinal);

    private readonly HttpClient httpClient;
    private readonly IMemoryCache memoryCache;
    private readonly BrowserCacheService browserCache;

    public AtcApiAppClient(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        BrowserCacheService browserCache)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(memoryCache);
        ArgumentNullException.ThrowIfNull(browserCache);

        this.httpClient = httpClient;
        this.memoryCache = memoryCache;
        this.browserCache = browserCache;
    }

    private static string BaseAddress
        => $"{AtcApiConstants.BaseAddress}/github/apps";

    public async Task<(bool IsSuccessful, List<AtcApp> Data)> GetAtcApps(
        CancellationToken cancellationToken = default)
    {
        const string cacheKey = CacheConstants.CacheKeyAtcApps;
        if (memoryCache.TryGetValue(cacheKey, out List<AtcApp>? data) && data is not null)
        {
            return (IsSuccessful: true, data);
        }

        var browserCached = await browserCache.GetAsync<List<AtcApp>>(cacheKey);
        if (browserCached is not null)
        {
            memoryCache.Set(cacheKey, browserCached, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, browserCached);
        }

        await SemaphoreAtcApps.WaitAsync(cancellationToken);
        try
        {
            var (isSuccessful, apps) = await GetJson<List<AtcApp>>(BaseAddress, cancellationToken);
            if (!isSuccessful || apps is null)
            {
                return (IsSuccessful: false, []);
            }

            memoryCache.Set(cacheKey, apps, CacheConstants.AbsoluteExpirationRelativeToNow);
            await browserCache.SetAsync(cacheKey, apps);
            return (IsSuccessful: true, apps);
        }
        finally
        {
            SemaphoreAtcApps.Release();
        }
    }

    public async Task<(bool IsSuccessful, AtcApp? Data)> GetAtcAppByName(
        string name,
        CancellationToken cancellationToken = default)
    {
        var (isSuccessful, apps) = await GetAtcApps(cancellationToken);
        var app = isSuccessful
            ? apps.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            : null;

        return (IsSuccessful: app is not null, app);
    }

    public async Task<(bool IsSuccessful, List<AtcAppRelease> Data)> GetReleases(
        string name,
        CancellationToken cancellationToken = default)
    {
        var result = await GetCachedDetail<List<AtcAppRelease>>(
            $"{CacheConstants.CacheKeyAtcAppReleases}_{name}",
            $"{BaseAddress}/{Uri.EscapeDataString(name)}/releases",
            cancellationToken);

        return (result is not null, result ?? []);
    }

    public async Task<(bool IsSuccessful, string Data)> GetReadme(
        string name,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheConstants.CacheKeyAtcAppReadme}_{name}";
        if (memoryCache.TryGetValue(cacheKey, out string? cached) && cached is not null)
        {
            return (IsSuccessful: true, cached);
        }

        try
        {
            var response = await httpClient.GetAsync(new Uri($"{BaseAddress}/{Uri.EscapeDataString(name)}/readme"), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, string.Empty);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            memoryCache.Set(cacheKey, content, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, content);
        }
        catch (HttpRequestException)
        {
            return (IsSuccessful: false, string.Empty);
        }
    }

    public async Task<(bool IsSuccessful, List<AtcAppComponentReference> Data)> GetBuiltWith(
        string name,
        CancellationToken cancellationToken = default)
    {
        var result = await GetCachedDetail<List<AtcAppComponentReference>>(
            $"{CacheConstants.CacheKeyAtcAppBuiltWith}_{name}",
            $"{BaseAddress}/{Uri.EscapeDataString(name)}/built-with",
            cancellationToken);

        return (result is not null, result ?? []);
    }

    public async Task<(bool IsSuccessful, WikiMetadata Data)> GetWiki(
        string name,
        CancellationToken cancellationToken = default)
    {
        var result = await GetCachedDetail<WikiMetadata>(
            $"{CacheConstants.CacheKeyAtcAppWiki}_{name}",
            $"{BaseAddress}/{Uri.EscapeDataString(name)}/wiki",
            cancellationToken);

        return (result is not null, result ?? new WikiMetadata());
    }

    private async Task<T?> GetCachedDetail<T>(
        string cacheKey,
        string url,
        CancellationToken cancellationToken)
        where T : class
    {
        if (memoryCache.TryGetValue(cacheKey, out T? cached) && cached is not null)
        {
            return cached;
        }

        // One lock per resource, so a slow call (e.g. "built with") never blocks the others.
        var detailLock = DetailLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        await detailLock.WaitAsync(cancellationToken);
        try
        {
            if (memoryCache.TryGetValue(cacheKey, out cached) && cached is not null)
            {
                return cached;
            }

            var (isSuccessful, data) = await GetJson<T>(url, cancellationToken);
            if (!isSuccessful || data is null)
            {
                return null;
            }

            memoryCache.Set(cacheKey, data, CacheConstants.AbsoluteExpirationRelativeToNow);
            return data;
        }
        finally
        {
            detailLock.Release();
        }
    }

    private async Task<(bool IsSuccessful, T? Data)> GetJson<T>(
        string url,
        CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            var response = await httpClient.GetAsync(new Uri(url), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, null);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonSerializer.Deserialize<T>(content, JsonSerializerOptionsFactory.Create());
            return (data is not null, data);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            return (IsSuccessful: false, null);
        }
    }
}