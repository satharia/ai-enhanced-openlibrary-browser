using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Authors.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;
using GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary.Contracts;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary
{
    internal sealed class OpenLibraryBookCatalog(OpenLibraryClient openLibraryClient) : IBookCatalog
    {
        // Open Library's search sorts by number of editions with this value
        private const string SortByEditionCount = "editions";

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

        public async Task<AuthorDetails?> GetAuthorAsync(string authorId, CancellationToken cancellationToken)
        {
            var author = await openLibraryClient.GetAuthorAsync(authorId, cancellationToken);

            return author is null
                ? null
                : new AuthorDetails(
                    ToId(author.Key),
                    author.Name,
                    author.Bio,
                    author.BirthDate,
                    author.DeathDate,
                    CoverUrlsExtractor.ForAuthorPhoto(author.Photos)
                    );
        }

        public async Task<PagedResult<BookSummary>> GetAuthorWorksAsync(string authorId, int page, int pageSize, CancellationToken cancellationToken)
        {
            var response = await openLibraryClient.SearchAsync(new SearchRequest
            {
                AuthorKey = authorId,
                Sort = SortByEditionCount,
                Page = page,
                Limit = pageSize
            },
            cancellationToken);

            return ToPagedResult(response, page, pageSize);
        }

        public async Task<WorkDetails?> GetWorkAsync(string workId, CancellationToken cancellationToken)
        {
            var work = await openLibraryClient.GetWorkAsync(workId, cancellationToken);

            return work is null
                ? null
                : new WorkDetails(
                    ToId(work.Key),
                    work.Title,
                    work.Description,
                    work.Subjects ?? [],
                    CoverUrlsExtractor.FirstPositive(work.Covers)
                    );
        }

        public async Task<PagedResult<EditionSummary>> GetWorkEditionsAsync(string workId, int page, int pageSize, CancellationToken cancellationToken)
        {
            var offset = (page - 1) * pageSize;
            var response = await openLibraryClient.GetWorkEditionsAsync(workId, pageSize, offset, cancellationToken);

            return response is null
                ? PagedResult<EditionSummary>.NewEmpty(page, pageSize)
                : new PagedResult<EditionSummary>(response.Entries.Select(ToEditionSummary).ToList(), response.Size, page, pageSize);
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
                : new PagedResult<BookSummary>(response.Docs.Select(ToBookSummary).ToList(), response.NumFound, page, pageSize);
        }

        private static BookSummary ToBookSummary(SearchDoc document)
        {
            // author_name and author_key are parallel arrays in Open Library's search response
            var authors = (document.AuthorName ?? [])
                .Select((authorName, index) => new AuthorRef(document.AuthorKey?.ElementAtOrDefault(index), authorName))
                .ToList();

            return new BookSummary(
                ToId(document.Key),
                document.Title,
                authors,
                document.FirstPublishYear,
                document.EditionCount,
                CoverUrlsExtractor.ForSearchDoc(document),
                document.CoverEditionKey
                );
        }

        private static EditionSummary ToEditionSummary(EditionRecord edition)
        {
            return new EditionSummary(
                ToId(edition.Key),
                edition.Title,
                edition.Publishers ?? [],
                edition.PublishDate,
                edition.NumberOfPages,
                edition.Isbn13?.FirstOrDefault(),
                (edition.Languages ?? []).Select(language => ToId(language.Key)).ToList(),
                CoverUrlsExtractor.FirstPositive(edition.Covers)
                );
        }

        // Open Library keys are paths ("/works/OL27448W", "/authors/OL26320A"), the last segment is the id
        private static string ToId(string key) => key[(key.LastIndexOf('/') + 1)..];
    }
}
