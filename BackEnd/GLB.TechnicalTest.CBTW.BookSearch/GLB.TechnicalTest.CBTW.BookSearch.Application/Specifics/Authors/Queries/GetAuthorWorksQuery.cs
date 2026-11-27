using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Caching;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Authors.Queries
{
    // Null result means Open Library has no author with that id
    public sealed record GetAuthorWorksQuery(string AuthorId, int Page = 1, int PageSize = 20) : IRequest<AuthorWorksResult?>, ICachedQuery
    {
        public string CacheKey => $"q:authors:works:{AuthorId}:{Page}:{PageSize}";
        public TimeSpan TTL => TimeSpan.FromDays(1);
    }
}
