namespace AtcWeb.Domain.AtcApi;

public class AtcApiNugetClient
{
    private const string BaseAddress = $"{AtcApiConstants.BaseAddress}/nuget-search";
    private readonly HttpClient httpClient;
    private readonly IMemoryCache memoryCache;
    private static readonly SemaphoreSlim SemaphoreTotalDownloads = new(1, 1);
    private static readonly SemaphoreSlim SemaphoreCliTools = new(1, 1);

    public AtcApiNugetClient(
        HttpClient httpClient,
        IMemoryCache memoryCache)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(memoryCache);

        this.httpClient = httpClient;
        this.memoryCache = memoryCache;
    }

    public async Task<(bool IsSuccessful, NugetTotalDownloadsResult? Result)> GetTotalDownloads(
        CancellationToken cancellationToken = default)
    {
        const string cacheKey = CacheConstants.CacheKeyNugetTotalDownloads;
        if (memoryCache.TryGetValue(cacheKey, out NugetTotalDownloadsResult? data))
        {
            return (IsSuccessful: true, data);
        }

        await SemaphoreTotalDownloads.WaitAsync(cancellationToken);

        try
        {
            const string url = $"{BaseAddress}/total-downloads";

            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, null);
            }

            var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<NugetTotalDownloadsResult>(content, JsonSerializerOptionsFactory.Create());
            if (result is null)
            {
                return (IsSuccessful: false, null);
            }

            memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, result);
        }
        catch
        {
            return (IsSuccessful: false, null);
        }
        finally
        {
            SemaphoreTotalDownloads.Release();
        }
    }

    public async Task<(bool IsSuccessful, NugetCliToolSearchResult Result)> GetCliTools(
        CancellationToken cancellationToken = default)
    {
        const string cacheKey = CacheConstants.CacheKeyNugetCliTools;
        if (memoryCache.TryGetValue(cacheKey, out NugetCliToolSearchResult? data) && data is not null)
        {
            return (IsSuccessful: true, data);
        }

        await SemaphoreCliTools.WaitAsync(cancellationToken);

        try
        {
            const string url = $"{BaseAddress}/cli-tools";

            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, new NugetCliToolSearchResult());
            }

            var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<NugetCliToolSearchResult>(content, JsonSerializerOptionsFactory.Create());
            if (result is null)
            {
                return (IsSuccessful: false, new NugetCliToolSearchResult());
            }

            memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, result);
        }
        catch
        {
            return (IsSuccessful: false, new NugetCliToolSearchResult());
        }
        finally
        {
            SemaphoreCliTools.Release();
        }
    }
}