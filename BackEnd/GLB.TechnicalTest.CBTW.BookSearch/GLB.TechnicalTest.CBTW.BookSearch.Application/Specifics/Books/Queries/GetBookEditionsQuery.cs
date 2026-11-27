using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Caching;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Queries
{
    // Null result means Open Library has no work with that id
    public sealed record GetBookEditionsQuery(string WorkId, int Page = 1, int PageSize = 20) : IRequest<BookEditionsResult?>, ICachedQuery
    {
        public string CacheKey => $"q:books:editions:{WorkId}:{Page}:{PageSize}";
        public TimeSpan TTL => TimeSpan.FromDays(1);
    }
}
