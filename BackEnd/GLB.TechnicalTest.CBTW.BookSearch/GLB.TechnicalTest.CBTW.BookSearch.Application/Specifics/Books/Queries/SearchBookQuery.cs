using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Caching;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Queries
{
    public sealed record SearchBookQuery(string FreeText, int Page = 1, int PageSize = 20) : IRequest<SearchBookResult>, ICachedQuery
    {
        public string CacheKey => $"q:books:search:{FreeText}:{Page}:{PageSize}";
        public TimeSpan TTL => TimeSpan.FromHours(1);
    }
}
