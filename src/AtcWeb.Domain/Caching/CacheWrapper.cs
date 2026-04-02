namespace AtcWeb.Domain.Caching;

internal sealed class CacheWrapper<T>
{
    public T Data { get; set; } = default!;

    public DateTimeOffset ExpiresAt { get; set; }
}