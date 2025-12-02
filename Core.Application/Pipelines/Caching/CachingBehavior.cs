//using Core.Abstractions.Applications.Caching;
//using MediatR;
//using Microsoft.Extensions.Caching.Distributed;
//using Microsoft.Extensions.Options;
//using System.Text;
//using System.Text.Json;

//namespace Core.Application.Pipelines.Caching;

//public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
//    where TRequest : IRequest<TResponse>, ICacheableRequest
//{
//    private readonly CacheSettings _cacheSettings;
//    private readonly IDistributedCache _distributedCache;

//    public CachingBehavior(IDistributedCache distributedCache, IOptions<CacheSettings> options)
//    {
//        _distributedCache = distributedCache;
//        _cacheSettings = options.Value ?? throw new InvalidOperationException();
//    }

//    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
//    {
//        if (request.BypassCache)
//            return await next();

//        byte[]? cachedResponse = await _distributedCache.GetAsync(request.CacheKey, cancellationToken);
//        TResponse? response;
//        if (cachedResponse is not null)
//        {
//             response = JsonSerializer.Deserialize<TResponse>(Encoding.UTF8.GetString(cachedResponse));
//            //log
//        }
//        else
//        {
//            response = await getResponseAndAddToCache(request, next, cancellationToken);
//        }
//    }

//    private async Task<TResponse?> getResponseAndAddToCache(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
//    {
//        TResponse? response = await next();

//        TimeSpan slidingExpiration = request.SlidingExpiration ?? TimeSpan.FromDays(_cacheSettings.SlidingExpiration);
//        DistributedCacheEntryOptions cacheEntryOptions = new()
//        {
//            SlidingExpiration = slidingExpiration
//        };
//    }
//}
