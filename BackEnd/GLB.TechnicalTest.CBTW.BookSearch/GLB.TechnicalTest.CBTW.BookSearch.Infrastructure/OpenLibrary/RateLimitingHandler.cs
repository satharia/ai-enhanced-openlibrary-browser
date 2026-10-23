using GLB.TechnicalTest.CBTW.BookSearch.Domain.Common.Exceptions.Application;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary
{
    internal sealed class RateLimitingHandler(OpenLibraryRateLimiter rateLimiter) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            using var lease = await rateLimiter.Limiter.AcquireAsync(1, ct);
            if (!lease.IsAcquired)
                throw new ExternalServiceException("Open Library", "Too many queued requests; try again shortly.");

            return await base.SendAsync(request, ct);
        }
    }
}
