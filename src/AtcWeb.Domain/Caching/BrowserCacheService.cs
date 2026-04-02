namespace AtcWeb.Domain.Caching;

/// <summary>
/// Provides a localStorage-based cache layer that persists across browser refreshes.
/// Uses a stale-while-revalidate pattern: cached data is returned immediately while
/// fresh data can be fetched in the background.
/// </summary>
public sealed class BrowserCacheService
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(30);
    private readonly IJSRuntime jsRuntime;

    public BrowserCacheService(IJSRuntime jsRuntime)
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);

        this.jsRuntime = jsRuntime;
    }

    public async Task<T?> GetAsync<T>(string key)
        where T : class
    {
        try
        {
            var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", $"cache_{key}");
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            var wrapper = JsonSerializer.Deserialize<CacheWrapper<T>>(json, JsonSerializerOptionsFactory.Create());
            if (wrapper is null || wrapper.ExpiresAt < DateTimeOffset.UtcNow)
            {
                await RemoveAsync(key);
                return null;
            }

            return wrapper.Data;
        }
        catch
        {
            return null;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? ttl = null)
        where T : class
    {
        try
        {
            var wrapper = new CacheWrapper<T>
            {
                Data = value,
                ExpiresAt = DateTimeOffset.UtcNow.Add(ttl ?? DefaultTtl),
            };

            var json = JsonSerializer.Serialize(wrapper, JsonSerializerOptionsFactory.Create());
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", $"cache_{key}", json);
        }
        catch
        {
            // localStorage may be full or unavailable — silently ignore
        }
    }

    private async Task RemoveAsync(string key)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", $"cache_{key}");
        }
        catch
        {
            // Ignore removal failures
        }
    }

    private sealed class CacheWrapper<T>
    {
        public T Data { get; set; } = default!;

        public DateTimeOffset ExpiresAt { get; set; }
    }
}