using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary
{
    internal sealed class OpenLibraryRateLimiter : IDisposable
    {
        public RateLimiter Limiter { get; }

        public OpenLibraryRateLimiter(IOptions<OpenLibraryOptions> options)
        {
            var perSecond = options.Value.RequestsPerSecond;
            Limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = perSecond,
                TokensPerPeriod = perSecond,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                AutoReplenishment = true,
                QueueLimit = 200,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            });
        }

        public void Dispose() => Limiter.Dispose();
    }
}
