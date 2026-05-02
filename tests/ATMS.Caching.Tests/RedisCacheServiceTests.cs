using ATMS.Caching.Services;
using ATMS.Caching.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.Redis;

namespace ATMS.Caching.Tests;

public class RedisCacheServiceTests : IAsyncLifetime
{
    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7.2-alpine")
        .Build();

    private ICacheService _cache = null!;

    public async Task InitializeAsync()
    {
        await _redis.StartAsync();

        var services = new ServiceCollection();
        services.AddStackExchangeRedisCache(o =>
            o.Configuration = _redis.GetConnectionString());

        var provider = services.BuildServiceProvider();
        var distributedCache = provider.GetRequiredService<IDistributedCache>();

        _cache = new RedisCacheService(distributedCache, NullLogger<RedisCacheService>.Instance);
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldCallFactory_OnlyOnce_WhenCalledTwice()
    {
        var callCount = 0;
        const string key = "test:key:1";

        await _cache.GetOrSetAsync(key, Factory, TimeSpan.FromMinutes(1));
        await _cache.GetOrSetAsync(key, Factory, TimeSpan.FromMinutes(1));

        Assert.Equal(1, callCount);
        return;

        Task<string> Factory()
        {
            callCount++;
            return Task.FromResult("value");
        }
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldCallFactory_AfterRemove()
    {
        var callCount = 0;
        const string key = "test:key:2";

        await _cache.GetOrSetAsync(key, Factory, TimeSpan.FromMinutes(1));
        await _cache.RemoveAsync(key);
        await _cache.GetOrSetAsync(key, Factory, TimeSpan.FromMinutes(1));

        Assert.Equal(2, callCount);
        return;

        Task<string> Factory()
        {
            callCount++;
            return Task.FromResult("value");
        }
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldReturnNull_WhenRedisIsDown()
    {
        await _redis.StopAsync();

        var result = await _cache.GetOrSetAsync(
            key: "test:key:3",
            factory: () => Task.FromResult("value"),
            ttl: TimeSpan.FromMinutes(1));
        
        Assert.Equal("value", result);
    }

    public async Task DisposeAsync() => await _redis.DisposeAsync();
}