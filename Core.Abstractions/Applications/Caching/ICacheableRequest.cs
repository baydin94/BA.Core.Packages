namespace Core.Abstractions.Applications.Caching;

public interface ICacheableRequest
{
    string CacheKey { get; }
    bool BypassCache { get; } //Developer
    TimeSpan? SlidingExpiration { get; }
}
