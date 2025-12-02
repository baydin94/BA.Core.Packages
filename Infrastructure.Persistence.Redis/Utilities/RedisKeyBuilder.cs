namespace Infrastructure.Persistence.Redis.Utilities;

public static class RedisKeyBuilder
{
    public static string Build(string tenantId, string group, string key) => $"{tenantId}:{group}:{key}";
    public static string BuildGroupPattern(string tenantId, string group) => $"{tenantId}:{group}:*";
}
