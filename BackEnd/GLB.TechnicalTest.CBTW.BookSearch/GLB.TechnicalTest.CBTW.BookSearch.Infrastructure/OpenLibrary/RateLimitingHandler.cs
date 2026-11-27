using GLB.TechnicalTest.CBTW.BookSearch.Domain.Common.Exceptions.Application;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary
{
    internal sealed class RateLimitingHandler(OpenLibraryRateLimiter rateLimiter) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using var lease = await rateLimiter.Limiter.AcquireAsync(1, cancellationToken);
            if (!lease.IsAcquired)
                throw new ExternalServiceException("OpenLibrary", "Too many queued requests; try again shortly.");

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
