using Core.Abstractions.Applications.Caching;
using Infrastructure.Persistence.Redis.Extensions;
using Infrastructure.Persistence.Redis.Services;
using Infrastructure.Persistence.Redis.Utilities;
using StackExchange.Redis;

namespace Infrastructure.Persistence.Redis.Adapters;

/// <summary>
/// Provides a Redis-backed implementation of the application cache service.
/// </summary>
public class RedisCacheAdapter : ICacheService
{
    private readonly IRedisService _redisService;

    public RedisCacheAdapter(IRedisService redisService)
    {
        _redisService = redisService;
    }

    public async Task<CacheReadResult<T>> GetAsync<T>(string tenantId, string group, string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string redisKey = RedisKeyBuilder.Build(tenantId, group, key);
        RedisValue value = await _redisService.StringGetAsync(redisKey);

        if (value.IsNull) return CacheReadResult<T>.Miss;

        return CacheReadResult<T>.Hit(value.FromJson<T>());

    }

    public Task<T?> GetTAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveGroupAsync(string tenantId, string group, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
