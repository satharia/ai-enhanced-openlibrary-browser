using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Tests.TestData
{
    // Builders that mirror the shape of Open Library's search results: authors in catalog order, first is the primary author
    internal static class TestBooks
    {
        public static BookSummary Book(string id, string title, int editionCount, params string[] authorNames)
        {
            var authors = authorNames.Select(authorName => new AuthorRef(null, authorName)).ToList();

            return new BookSummary(id, title, authors, FirstPublishYear: null, editionCount, CoverUrl: null);
        }

        public static BookSearchCriteria Criteria(string? title = null, string? author = null, string? freeText = null)
        {
            return new BookSearchCriteria(title, author, null, null, null, null, null, freeText);
        }
    }
}
