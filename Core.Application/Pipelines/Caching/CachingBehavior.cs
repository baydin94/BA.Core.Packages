using Core.Abstractions.Applications.Caching;
using Core.Abstractions.Applications.Transaction;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core.Application.Pipelines.Caching;

/// <summary>Serves opted-in, tenant-scoped queries from cache with a fixed lifetime.</summary>
public sealed class CachingBehavior<TRequest, TResponse>(
    ICacheService cache, IOptions<CacheSettings> options,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request is ITransactionRequest || System.Transactions.Transaction.Current is not null ||
            request is not ICacheableRequest { BypassCache: false, CacheAddress: { } address } query)
            return await next(cancellationToken);

        var ttl = query.AbsoluteExpiration ?? options.Value.AbsoluteExpiration;
        if (ttl < TimeSpan.FromMilliseconds(1))
            throw new ArgumentOutOfRangeException(nameof(ttl), "Cache lifetime must be at least one millisecond.");

        CacheReadResult<TResponse> read;
        try
        {
            read = await cache.GetAsync<TResponse>(address, cancellationToken);
        }
        catch (CacheUnavailableException)
        {
            logger.LogWarning("Cache read unavailable for {RequestType}.", typeof(TRequest).Name);
            return await next(cancellationToken);
        }

        if (read.IsHit)
            return read.Value!;

        var response = await next(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            if (read.Version is { } version && cache is IConditionalCacheWriter writer)
                await writer.SetIfCurrentAsync(address, response, ttl, version, cancellationToken);
            else
                await cache.SetAsync(address, response, ttl, cancellationToken);
        }
        catch (CacheUnavailableException)
        {
            logger.LogWarning("Cache write unavailable for {RequestType}.", typeof(TRequest).Name);
        }

        return response;
    }
}
