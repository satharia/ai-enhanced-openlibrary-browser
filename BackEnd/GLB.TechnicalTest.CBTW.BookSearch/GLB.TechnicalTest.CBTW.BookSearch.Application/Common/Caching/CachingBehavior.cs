using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Caching
{
    // Returns the cached response of any request implementing ICachedQuery, otherwise runs the handler and caches its response
    public sealed class CachingBehavior<TRequest, TResponse>(
        IDistributedCache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not ICachedQuery cachedQuery)
                return await next();

            // The cache only saves calls to the external services, a failing cache must never fail the request
            try
            {
                var cachedJson = await cache.GetStringAsync(cachedQuery.CacheKey, cancellationToken);
                if (cachedJson is not null)
                    return JsonSerializer.Deserialize<TResponse>(cachedJson, JsonSerializerOptions)!;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Could not read {CacheKey} from the cache; running the query", cachedQuery.CacheKey);
            }

            var response = await next();

            try
            {
                var cacheEntryOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = cachedQuery.TTL };
                var json = JsonSerializer.Serialize(response, JsonSerializerOptions);

                await cache.SetStringAsync(cachedQuery.CacheKey, json, cacheEntryOptions, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Could not write {CacheKey} to the cache", cachedQuery.CacheKey);
            }

            return response;
        }
    }
}
