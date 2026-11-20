namespace Core.Abstractions.Applications.Caching;

/// <summary>Stores a computed result only while the group version observed on a miss is current.</summary>
public interface IConditionalCacheWriter
{
    Task SetIfCurrentAsync<T>(CacheAddress address, T value, TimeSpan ttl, string version,
        CancellationToken cancellationToken = default);
}
