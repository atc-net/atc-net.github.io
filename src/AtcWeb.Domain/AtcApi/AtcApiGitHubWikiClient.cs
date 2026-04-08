namespace AtcWeb.Domain.AtcApi;

public class AtcApiGitHubWikiClient
{
    private const string BaseAddress = $"{AtcApiConstants.BaseAddress}/github/wiki";
    private readonly HttpClient httpClient;
    private readonly IMemoryCache memoryCache;
    private static readonly SemaphoreSlim SemaphoreWiki = new(1, 1);

    public AtcApiGitHubWikiClient(
        HttpClient httpClient,
        IMemoryCache memoryCache)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(memoryCache);

        this.httpClient = httpClient;
        this.memoryCache = memoryCache;
    }

    public async Task<(bool IsSuccessful, WikiMetadata Data)> GetWiki(
        string repositoryName,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheConstants.CacheKeyWiki}_{repositoryName}";
        if (memoryCache.TryGetValue(cacheKey, out WikiMetadata? data) && data is not null)
        {
            return (IsSuccessful: true, data);
        }

        await SemaphoreWiki.WaitAsync(cancellationToken);

        try
        {
            var url = $"{BaseAddress}/{repositoryName}";

            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, new WikiMetadata());
            }

            var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<WikiMetadata>(content, JsonSerializerOptionsFactory.Create());
            if (result is null)
            {
                return (IsSuccessful: false, new WikiMetadata());
            }

            memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (IsSuccessful: true, result);
        }
        catch
        {
            return (IsSuccessful: false, new WikiMetadata());
        }
        finally
        {
            SemaphoreWiki.Release();
        }
    }
}