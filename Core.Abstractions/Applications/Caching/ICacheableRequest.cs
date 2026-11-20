namespace Core.Abstractions.Applications.Caching;

public interface ICacheableRequest
{
    CacheAddress? CacheAddress => null;
    TimeSpan? AbsoluteExpiration => null;
    bool BypassCache => false;

    // Retained for existing request implementations. Scoped caching requires CacheAddress.
    string CacheKey => CacheAddress?.Key ?? string.Empty;
    TimeSpan? SlidingExpiration => null;
}
