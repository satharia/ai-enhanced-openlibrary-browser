using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;
using GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary.Contracts;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary
{
    internal sealed class OpenLibraryBookCatalog(OpenLibraryClient openLibraryClient) : IBookCatalog
    {
        public async Task<PagedResult<BookSummary>> SearchWorksAsync(BookSearchCriteria criteria, int page, int pageSize, CancellationToken cancellationToken)
        {
            var response = await openLibraryClient.SearchAsync(new SearchRequest
            {
                Title = criteria.Title,
                Author = criteria.Author,
                Subject = criteria.Subject,
                Publisher = criteria.Publisher,
                Q = BuildOpenLibraryQ(criteria),
                Page = page,
                Limit = pageSize
            },
            cancellationToken);

            return ToPagedResult(response, page, pageSize);
        }

        private static string? BuildOpenLibraryQ(BookSearchCriteria criteria)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(criteria.FreeText))
                parts.Add(criteria.FreeText);

            if (!string.IsNullOrWhiteSpace(criteria.Language))
                parts.Add($"language:{criteria.Language}");

            if (criteria.PublishedFrom is not null || criteria.PublishedTo is not null)
                parts.Add($"first_publish_year:[{criteria.PublishedFrom ?? 0} TO {criteria.PublishedTo ?? DateTime.UtcNow.Year + 1}]");

            return parts.Count == 0 
                ? null 
                : string.Join(" ", parts);
        }

        private static PagedResult<BookSummary> ToPagedResult(SearchResponse? response, int page, int pageSize)
        {
            return response is null
                ? PagedResult<BookSummary>.NewEmpty(page, pageSize)
                : new PagedResult<BookSummary>(ToSummaryList(response.Docs), response.NumFound, page, pageSize);
        }

        private static List<BookSummary> ToSummaryList(IEnumerable<SearchDoc> documents)
        {
            return documents.Select(ToSummary).OfType<BookSummary>().ToList();
        }

        private static BookSummary? ToSummary(SearchDoc document)
        {
            var authors = (document.AuthorName ?? []).Select((name, i) =>
            {
                var key = document.AuthorKey?.ElementAtOrDefault(i);
                return new AuthorRef(key is not null ? key : null, name);
            }).ToList();

            return new BookSummary(
                document.Key,
                document.Title,
                authors,
                document.FirstPublishYear,
                document.EditionCount,
                CoverUrlsExtractor.ForSearchDoc(document),
                document.CoverEditionKey
                );
        }
    }
}
