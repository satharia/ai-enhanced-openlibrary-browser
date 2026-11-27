using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Authors.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Authors.Queries
{
    public sealed record AuthorWorksResult(
        AuthorDetails Author,
        PagedResult<BookSummary> Works
        );
}
