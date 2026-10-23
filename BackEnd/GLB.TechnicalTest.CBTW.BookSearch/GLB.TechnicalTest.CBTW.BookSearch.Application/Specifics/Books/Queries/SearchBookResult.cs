using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Queries
{
    public sealed record SearchBookResult(
        BookSearchCriteria Criteria,
        bool UsedFallback,
        PagedResult<BookSummary> Books
        );
}
