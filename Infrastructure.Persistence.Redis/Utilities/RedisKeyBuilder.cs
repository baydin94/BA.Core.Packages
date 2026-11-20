using System.Text;

namespace Infrastructure.Persistence.Redis.Utilities;

public static class RedisKeyBuilder
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static string Build(string tenantId, string group, string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return $"{LegacyGroup(tenantId, group)}:{key}";
    }

    public static string BuildGroupPattern(string tenantId, string group)
        => LegacyGroup(tenantId, group) + ":*";

    private static string LegacyGroup(string tenantId, string group)
    {
        ValidateLegacySegment(tenantId);
        ValidateLegacySegment(group);
        return $"{tenantId}:{group}";
    }

    private static void ValidateLegacySegment(string segment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(segment);
        if (segment.IndexOfAny([':', '*', '?', '[', ']', '{', '}', '\\']) >= 0)
            throw new ArgumentException("Invalid delimiter or Redis pattern character.", nameof(segment));
    }

    internal static string BuildCacheGroup(string instance, string tenantId, string group)
        => $"{{{Encode(instance)}:{Encode(tenantId)}:{Encode(group)}}}:cache:v1";

    internal static string Encode(string segment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(segment);
        return Convert.ToBase64String(StrictUtf8.GetBytes(segment));
    }
}
