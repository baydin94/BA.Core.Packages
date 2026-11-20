using System.Text.Json;
using Core.Abstractions.Applications.Caching;
using Infrastructure.Persistence.Redis.Configuration;
using Infrastructure.Persistence.Redis.Services;
using Infrastructure.Persistence.Redis.Utilities;
using StackExchange.Redis;

namespace Infrastructure.Persistence.Redis.Adapters;

/// <summary>Stores JSON entries with absolute expiration and tenant-scoped group invalidation.</summary>
public class RedisCacheAdapter : ICacheService, IConditionalCacheWriter
{
    // Both keys share one Redis Cluster hash slot. Reads, conditional fills and
    // invalidation therefore linearize on the primary without enumerating keys.
    private const string ReadScript = """
        local generation = redis.call('GET', KEYS[1])
        if not generation then
            generation = ARGV[1]
            redis.call('SET', KEYS[1], generation, 'PX', 3600000)
        end
        if redis.call('HGET', KEYS[2], 'generation') == generation then
            return {generation, redis.call('HGET', KEYS[2], 'value')}
        end
        return {generation, false}
        """;

    private const string WriteScript = """
        local generation = redis.call('GET', KEYS[1])
        if ARGV[1] ~= '' and ARGV[1] ~= generation then
            return 0
        end
        if not generation then
            generation = ARGV[2]
            redis.call('SET', KEYS[1], generation, 'PX', ARGV[4])
        elseif redis.call('PTTL', KEYS[1]) < tonumber(ARGV[4]) then
            redis.call('PEXPIRE', KEYS[1], ARGV[4])
        end
        redis.call('HSET', KEYS[2], 'generation', generation)
        redis.call('HSET', KEYS[2], 'value', ARGV[3])
        redis.call('PEXPIRE', KEYS[2], ARGV[4])
        return 1
        """;

    private readonly IRedisService _service;
    private readonly string _instanceName;

    public RedisCacheAdapter(IRedisService redisService) : this(redisService, new RedisSettings()) { }

    public RedisCacheAdapter(IRedisService redisService, RedisSettings settings)
    {
        _service = redisService;
        _instanceName = settings.InstanceName;
    }

    public Task<CacheReadResult<T>> GetAsync<T>(string tenantId, string group, string key,
        CancellationToken cancellationToken = default)
        => GetAsync<T>(new CacheAddress(tenantId, group, key), cancellationToken);

    public async Task<CacheReadResult<T>> GetAsync<T>(CacheAddress address,
        CancellationToken cancellationToken = default)
    {
        var keys = GetKeys(address);
        var result = await ExecuteAsync(db => db.ScriptEvaluateAsync(
            ReadScript, keys, [NewGeneration()]), cancellationToken).ConfigureAwait(false);
        var parts = (RedisResult[])result!;
        if (parts[1].IsNull)
            return CacheReadResult<T>.MissAt(parts[0].ToString());

        var value = JsonSerializer.Deserialize<T>((byte[])parts[1]!);
        return CacheReadResult<T>.Hit(value);
    }

    public Task SetAsync<T>(CacheAddress address, T value, TimeSpan ttl,
        CancellationToken cancellationToken = default)
        => WriteAsync(address, value, ttl, string.Empty, cancellationToken);

    public Task SetIfCurrentAsync<T>(CacheAddress address, T value, TimeSpan ttl,
        string version, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        return WriteAsync(address, value, ttl, version, cancellationToken);
    }

    private async Task WriteAsync<T>(CacheAddress address, T value, TimeSpan ttl,
        string version, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfLessThan(ttl, TimeSpan.FromMilliseconds(1));
        var keys = GetKeys(address);
        long milliseconds = ttl.Ticks / TimeSpan.TicksPerMillisecond;
        var payload = JsonSerializer.SerializeToUtf8Bytes(value);
        await ExecuteAsync(db => db.ScriptEvaluateAsync(WriteScript, keys,
            [version, NewGeneration(), payload, milliseconds]), cancellationToken).ConfigureAwait(false);
    }

    public Task RemoveAsync(CacheAddress address, CancellationToken cancellationToken = default)
    {
        var keys = GetKeys(address);
        return ExecuteAsync(db => db.KeyDeleteAsync(keys[1]), cancellationToken);
    }

    public Task RemoveGroupAsync(string tenantId, string group, CancellationToken cancellationToken = default)
    {
        var prefix = RedisKeyBuilder.BuildCacheGroup(_instanceName, tenantId, group);
        return ExecuteAsync(db => db.KeyDeleteAsync(prefix + ":generation"), cancellationToken);
    }

    public async Task<T?> GetTAsync<T>(string key, CancellationToken cancellationToken = default)
        => (await GetAsync<T>(ParseAddress(key), cancellationToken).ConfigureAwait(false)).Value;

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
        => SetAsync(ParseAddress(key), value, ttl, cancellationToken);

    public Task RemoveAsync<T>(string key, CancellationToken cancellationToken = default)
        => RemoveAsync(ParseAddress(key), cancellationToken);

    private static CacheAddress ParseAddress(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var segments = key.Split(':', 3);
        if (segments.Length != 3)
            throw new ArgumentException("Expected a tenant:group:key address.", nameof(key));
        return new CacheAddress(segments[0], segments[1], segments[2]);
    }

    private RedisKey[] GetKeys(CacheAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        var prefix = RedisKeyBuilder.BuildCacheGroup(_instanceName, address.TenantId, address.Group);
        return [prefix + ":generation", prefix + ":value:" + RedisKeyBuilder.Encode(address.Key)];
    }

    private static string NewGeneration() => Guid.NewGuid().ToString("N");

    private async Task<TResult> ExecuteAsync<TResult>(
        Func<IDatabase, Task<TResult>> operation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return await operation(_service.GetDatabase()).WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception error) when (error is RedisConnectionException or RedisTimeoutException)
        {
            throw new CacheUnavailableException("Redis cache is unavailable.", error);
        }
    }
}
