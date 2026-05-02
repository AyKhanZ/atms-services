namespace ATMS.Caching.Models;

public sealed class CacheEntry<T>
{
    public T Value { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
}