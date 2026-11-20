namespace Core.Abstractions.Applications.Caching;

/// <summary>Identifies a cached projection within a tenant and an invalidation group.</summary>
public sealed record CacheAddress(string TenantId, string Group, string Key);
