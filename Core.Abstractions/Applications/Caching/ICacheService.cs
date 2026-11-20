namespace Core.Abstractions.Applications.Caching;

public interface ICacheService
{
    Task<CacheReadResult<T>> GetAsync<T>(string tenantId, string group, string key, CancellationToken cancellationToken = default);
    Task<T?> GetTAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task RemoveAsync<T>(string key, CancellationToken cancellationToken = default);
    Task RemoveGroupAsync(string tenantId, string group, CancellationToken cancellationToken = default);
}
