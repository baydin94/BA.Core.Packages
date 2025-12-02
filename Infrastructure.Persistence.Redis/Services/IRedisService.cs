using StackExchange.Redis;

namespace Infrastructure.Persistence.Redis.Services;

public interface IRedisService 
{
    //Low-level access (non-tenant)
    IDatabase GetDatabase();
    Task<bool> KeyDeleteAsync(RedisKey key);
    Task<bool> KeyExistsAsync(RedisKey key);
    Task<bool> StringSetAsync(RedisKey key, RedisValue value, Expiration expiration = default, When when = When.Always, CommandFlags flags = CommandFlags.None);
    Task<RedisValue> StringGetAsync(RedisKey key);

    //Multi-tenant / prefix-based
    Task SetAsync(string tenantId, string group, string key, RedisValue value, Expiration expiration = default, When when = When.Always, CommandFlags flags = CommandFlags.None);
    Task<T?> GetAsync<T>(string tenantId, string group, string key);
    Task RemoveGroupAsync(string tenantId, string group);
}
