using Infrastructure.Persistence.Redis.Connection;
using Infrastructure.Persistence.Redis.Extensions;
using Infrastructure.Persistence.Redis.Utilities;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Persistence.Redis.Services;

public class RedisService : IRedisService
{
    private readonly RedisConnectionFactory _redisConnectionFactory;

    /// <inheritdoc />
    public RedisService(RedisConnectionFactory redisConnectionFactory)
    {
        _redisConnectionFactory = redisConnectionFactory;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string tenantId, string group, string key)
    {
        string redisKey = RedisKeyBuilder.Build(tenantId, group, key);
        RedisValue value = await GetDatabase().StringGetAsync(redisKey);
        return value.FromJson<T>();
    }
    /// <inheritdoc />
    public IDatabase GetDatabase()
    {
        return _redisConnectionFactory.GetDatabase();
    }
    /// <inheritdoc />
    public async Task<bool> KeyDeleteAsync(RedisKey key)
    {
        return await GetDatabase().KeyDeleteAsync(key);
    }
    /// <inheritdoc />
    public async Task<bool> KeyExistsAsync(RedisKey key)
    {
        return await GetDatabase().KeyExistsAsync(key);
    }
    /// <inheritdoc />
    public async Task RemoveGroupAsync(string tenantId, string group)
    {
        string pattern = RedisKeyBuilder.BuildGroupPattern(tenantId, group);
        IServer server = _redisConnectionFactory.GetServer();
        IDatabase database = GetDatabase();

        await foreach (var key in server.KeysAsync(database: database.Database, pattern: pattern))
            await database.KeyDeleteAsync(key);
    }
    /// <inheritdoc />
    public Task<bool> SetAsync<T>(string tenantId, string group, string key, T value, Expiration expiration = default, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        string redisKey = RedisKeyBuilder.Build(tenantId, group, key);
        string json = JsonSerializer.Serialize(value);
        return StringSetAsync(redisKey, json, expiration, when, flags);
    }
    /// <inheritdoc />
    public async Task<RedisValue> StringGetAsync(RedisKey key)
    {
        return await GetDatabase().StringGetAsync(key);
    }
    /// <inheritdoc />
    public async Task<bool> StringSetAsync(RedisKey key, RedisValue value, Expiration expiration = default, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        return await GetDatabase().StringSetAsync(key, value, expiration, when, flags);
    }
}
