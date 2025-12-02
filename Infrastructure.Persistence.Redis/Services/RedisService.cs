using Infrastructure.Persistence.Redis.Connection;
using Infrastructure.Persistence.Redis.Extensions;
using Infrastructure.Persistence.Redis.Utilities;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Persistence.Redis.Services;

public class RedisService : IRedisService
{
    private readonly RedisConnectionFactory _redisConnectionFactory;

    public RedisService(RedisConnectionFactory redisConnectionFactory)
    {
        _redisConnectionFactory = redisConnectionFactory;
    }

    public async Task<T?> GetAsync<T>(string tenantId, string group, string key)
    {
        string redisKey = RedisKeyBuilder.Build(tenantId, group, key);
        RedisValue value = await GetDatabase().StringGetAsync(redisKey);
        return value.FromJson<T>();
    }

    public IDatabase GetDatabase()
    {
        return _redisConnectionFactory.GetDatabase();
    }

    public async Task<bool> KeyDeleteAsync(RedisKey key)
    {
        return await GetDatabase().KeyDeleteAsync(key);
    }

    public async Task<bool> KeyExistsAsync(RedisKey key)
    {
        return await GetDatabase().KeyExistsAsync(key);
    }

    public async Task RemoveGroupAsync(string tenantId, string group)
    {
        string pattern = RedisKeyBuilder.BuildGroupPattern(tenantId, group);
        IServer server = _redisConnectionFactory.GetServer();
        IDatabase database = GetDatabase();

        await foreach (var key in server.KeysAsync(database: database.Database, pattern: pattern))
            await database.KeyDeleteAsync(key);
    }

    public async Task SetAsync(string tenantId, string group, string key, RedisValue value, Expiration expiration = default, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        string redisKey = RedisKeyBuilder.Build(tenantId, group, key);
        string json = JsonSerializer.Serialize(value);
        await StringSetAsync(redisKey, json, expiration, when, flags);
    }

    public async Task<RedisValue> StringGetAsync(RedisKey key)
    {
        return await GetDatabase().StringGetAsync(key);
    }

    public async Task<bool> StringSetAsync(RedisKey key, RedisValue value, Expiration expiration = default, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        return await GetDatabase().StringSetAsync(key, value, expiration, when, flags);
    }
}
