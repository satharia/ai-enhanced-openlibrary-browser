using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces
{
    public interface IBookCatalog
    {
        Task<PagedResult<BookSummary>> SearchWorksAsync(BookSearchCriteria criteria, int page, int PageSize, CancellationToken cancellationToken);
    }
}
