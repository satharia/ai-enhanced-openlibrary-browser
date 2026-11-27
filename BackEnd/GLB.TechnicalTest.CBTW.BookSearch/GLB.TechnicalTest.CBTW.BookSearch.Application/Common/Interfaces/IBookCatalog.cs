using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Authors.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces
{
    public interface IBookCatalog
    {
        Task<PagedResult<BookSummary>> SearchWorksAsync(BookSearchCriteria criteria, int page, int pageSize, CancellationToken cancellationToken);

        Task<AuthorDetails?> GetAuthorAsync(string authorId, CancellationToken cancellationToken);

        Task<PagedResult<BookSummary>> GetAuthorWorksAsync(string authorId, int page, int pageSize, CancellationToken cancellationToken);

        Task<WorkDetails?> GetWorkAsync(string workId, CancellationToken cancellationToken);

        Task<PagedResult<EditionSummary>> GetWorkEditionsAsync(string workId, int page, int pageSize, CancellationToken cancellationToken);
    }
}
