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
    private static readonly ConcurrentDictionary<string, TaskCompletionSource<object?>> InFlightLoads = new();

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetFromCacheAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached.Value;
        }

        while (true)
        {
            var completion = new TaskCompletionSource<object?>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            var activeLoad = InFlightLoads.GetOrAdd(key, completion);
            if (!ReferenceEquals(activeLoad, completion))
            {
                try
                {
                    var sharedResult = await activeLoad.Task.WaitAsync(cancellationToken);
                    return (T?)sharedResult;
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Yield();
                    continue;
                }
            }

            try
            {
                logger.LogDebug("Cache miss: {Key}", key);
                var result = await factory();
                await SetInCacheAsync(key, result, ttl, cancellationToken);
                completion.TrySetResult(result);
                return result;
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
                _ = completion.Task.Exception;
                throw;
            }
            finally
            {
                InFlightLoads.TryRemove(
                    new KeyValuePair<string, TaskCompletionSource<object?>>(key, completion));
            }
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await cache.RemoveAsync(key, ct);
        logger.LogDebug("Cache invalidated: {Key}", key);
    }

    private async Task<CacheEntry<T>?> GetFromCacheAsync<T>(
        string key,
        CancellationToken cancellationToken)
    {
        try
        {
            var bytes = await cache.GetAsync(key, cancellationToken);
            return bytes is null ? null : JsonSerializer.Deserialize<CacheEntry<T>>(bytes);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Redis read failed. Key: {Key}", key);
            return null;
        }
    }

    private async Task SetInCacheAsync<T>(
        string key,
        T value,
        TimeSpan ttl,
        CancellationToken cancellationToken)
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
        catch (Exception exception)
        {
            logger.LogError(exception, "Redis write failed. Key: {Key}", key);
        }
    }
}
