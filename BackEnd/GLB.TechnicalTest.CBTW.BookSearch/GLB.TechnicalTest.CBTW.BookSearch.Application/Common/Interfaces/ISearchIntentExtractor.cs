using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces
{
    public interface ISearchIntentExtractor
    {
        Task<BookSearchCriteria?> ExtractAsync(string freeText, CancellationToken cancellationToken);
    }
}
