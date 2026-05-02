using System.Collections.Concurrent;
using System.Text.Json;
using ATMS.Caching.Models;
using ATMS.Caching.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace ATMS.Caching.Services;

public sealed class RedisCacheService(
    IDistributedCache cache,
    ILogger<RedisCacheService> logger) : ICacheService
{
    // Start background refresh if TTL has less than 30 seconds left
    private static readonly TimeSpan EarlyRefreshThreshold = TimeSpan.FromSeconds(30);

    // One semaphore per key — only one thread goes to DB on cache miss
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Semaphores = new();

    // Tracks keys that are already being refreshed in background
    private static readonly ConcurrentDictionary<string, byte> RefreshingKeys = new();


    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        // Try to get from cache first
        // Fast track — we don't touch the semaphore
        var cached = await GetFromCacheAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            // TTL is low — refresh in background so next requests get fresh data
            // TryAdd returns true only for the first thread, others skip
            if (cached.ExpiresAt - DateTime.UtcNow < EarlyRefreshThreshold
                && RefreshingKeys.TryAdd(key, 1))
            {
                _ = Task.Run(async () =>
                {
                    await RefreshInBackgroundAsync(key, factory, ttl);
                    RefreshingKeys.TryRemove(key, out _);
                }, cancellationToken);
            }

            return cached.Value;
        }

        // The slow path is cache miss, we take a semaphore
        // Cache miss — only one thread goes to DB, others wait
        var semaphore = Semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            // Check again — while we were waiting, another thread may have filled the cache
            cached = await GetFromCacheAsync<T>(key, cancellationToken);
            if (cached is not null)
            {
                return cached.Value;
            }

            // Only one thread reaches here — load from DB and save to cache
            logger.LogDebug("Cache miss: {Key}", key);
            var result = await factory();
            await SetInCacheAsync(key, result, ttl, cancellationToken);
            return result;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(key, cancellationToken);
        logger.LogDebug("Cache invalidated: {Key}", key);
    }


    private async Task<CacheEntry<T>?> GetFromCacheAsync<T>(string key, CancellationToken cancellationToken)
    {
        try
        {
            var bytes = await cache.GetAsync(key, cancellationToken);
            return bytes is null ? null : JsonSerializer.Deserialize<CacheEntry<T>>(bytes);
        }
        catch (Exception ex)
        {
            // Redis crashed — log in and continue without cache, do not drop the application
            logger.LogError(ex, "Redis read failed. Key: {Key}", key);
            return null;
        }
    }

    private async Task SetInCacheAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        try
        {
            var entry = new CacheEntry<T>
            {
                Value = value,
                ExpiresAt = DateTime.UtcNow.Add(ttl)
            };
            var bytes = JsonSerializer.SerializeToUtf8Bytes(entry);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };
            await cache.SetAsync(key, bytes, options, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Redis write failed. Key: {Key}", key);
        }
    }

    private async Task RefreshInBackgroundAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl)
    {
        try
        {
            var result = await factory();
            await SetInCacheAsync(key, result, ttl, CancellationToken.None);
            logger.LogDebug("Background cache refresh done: {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Background cache refresh failed: {Key}", key);
        }
    }
}