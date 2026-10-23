using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Rankers.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces
{
    public interface IResultRanker
    {
        Task<Ranking> RankAsync(BookSearchCriteria criteria, IReadOnlyList<BookSummary> candidates, CancellationToken cancellationToken);
    }
}
