using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Queries
{
    public sealed class SearchBookQueryHandler : IRequestHandler<SearchBookQuery, SearchBookResult>
    {
        public async Task<SearchBookResult> Handle(SearchBookQuery reqeust, CancellationToken cancellationToken)
        {
            return new SearchBookResult(null, false, null);
        }
    }
}
