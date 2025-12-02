using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Persistence.Redis.Extensions;

public static class RedisValueExtensions
{
    public static T? FromJson<T>(this RedisValue value)
    {
        if (!value.HasValue)
            return default;
        return JsonSerializer.Deserialize<T>(value.ToString());
    }
}
