using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Authors.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Queries;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Rankers.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;
using Microsoft.Extensions.Logging.Abstractions;
using static GLB.TechnicalTest.CBTW.BookSearch.Application.Tests.TestData.TestBooks;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Tests.Books
{
    public class SearchBookQueryHandlerTests
    {
        private static readonly BookSummary Hobbit = Book("OL27482W", "The Hobbit", 482, "J.R.R. Tolkien");

        [Fact]
        public async Task Handle_IntentExtractionFails_SearchesWithTheFreeText()
        {
            var bookCatalog = new FakeBookCatalog(_ => [Hobbit]);
            var handler = CreateHandler(_ => throw new HttpRequestException("Gemini is down"), bookCatalog);

            var result = await handler.Handle(new SearchBookQuery("the hobbit"), CancellationToken.None);

            Assert.True(result.UsedFallback);
            Assert.Equal("the hobbit", bookCatalog.SearchedCriteria.Single().FreeText);
        }

        [Fact]
        public async Task Handle_InvalidExtractedCriteria_SearchesWithTheFreeText()
        {
            var invalidYearRange = Criteria(title: "the hobbit") with { PublishedFrom = 2000, PublishedTo = 1990 };
            var bookCatalog = new FakeBookCatalog(_ => [Hobbit]);
            var handler = CreateHandler(_ => invalidYearRange, bookCatalog);

            var result = await handler.Handle(new SearchBookQuery("the hobbit after 2000 before 1990"), CancellationToken.None);

            Assert.True(result.UsedFallback);
            Assert.Null(bookCatalog.SearchedCriteria.Single().Title);
        }

        [Fact]
        public async Task Handle_StructuredSearchFindsNothing_RetriesAndRanksWithTheFreeText()
        {
            var bookCatalog = new FakeBookCatalog(criteria => criteria.HasStructuredFields ? [] : [Hobbit]);
            var handler = CreateHandler(_ => Criteria(title: "the hobit", author: "tolkien"), bookCatalog);

            var result = await handler.Handle(new SearchBookQuery("the hobbit tolkien"), CancellationToken.None);

            Assert.Equal(2, bookCatalog.SearchedCriteria.Count);
            Assert.True(result.UsedFallback);
            Assert.Equal("the hobbit tolkien", result.Criteria.FreeText);
            Assert.Null(result.Criteria.Title);
            Assert.Single(result.Books.Items);
        }

        [Fact]
        public async Task Handle_RankerFails_UsesTheDeterministicRanking()
        {
            var bookCatalog = new FakeBookCatalog(_ => [Book("OL1W", "Hobbit Companion", 30, "David Day"), Hobbit]);
            var handler = CreateHandler(_ => Criteria(title: "the hobbit", author: "tolkien"), bookCatalog, rankerFails: true);

            var result = await handler.Handle(new SearchBookQuery("the hobbit by tolkien"), CancellationToken.None);

            Assert.False(result.UsedFallback);
            Assert.Equal(MatchTier.ExactTitleAndPrimaryAuthor, result.Tier);
            Assert.Equal(Hobbit.Id, result.Books.Items[0].Id);
        }

        private static SearchBookQueryHandler CreateHandler(
            Func<string, BookSearchCriteria?> extractCriteria, FakeBookCatalog bookCatalog, bool rankerFails = false)
        {
            return new SearchBookQueryHandler(
                new FakeSearchIntentExtractor(extractCriteria),
                bookCatalog,
                new FakeResultRanker(rankerFails),
                NullLogger<SearchBookQueryHandler>.Instance);
        }

        private sealed class FakeSearchIntentExtractor(Func<string, BookSearchCriteria?> extractCriteria) : ISearchIntentExtractor
        {
            public Task<BookSearchCriteria?> ExtractAsync(string freeText, CancellationToken cancellationToken) =>
                Task.FromResult(extractCriteria(freeText));
        }

        private sealed class FakeResultRanker(bool rankerFails) : IResultRanker
        {
            public Task<Ranking> RankAsync(BookSearchCriteria criteria, IReadOnlyList<BookSummary> candidates, CancellationToken cancellationToken)
            {
                if (rankerFails)
                    throw new HttpRequestException("Gemini is down");

                return Task.FromResult(new Ranking(MatchTier.NoClearWinner, "Ranked by the fake", candidates));
            }
        }

        // Only the search is used by this handler; it records every criteria it is asked to search with
        private sealed class FakeBookCatalog(Func<BookSearchCriteria, List<BookSummary>> findBooks) : IBookCatalog
        {
            public List<BookSearchCriteria> SearchedCriteria { get; } = [];

            public Task<PagedResult<BookSummary>> SearchWorksAsync(BookSearchCriteria criteria, int page, int pageSize, CancellationToken cancellationToken)
            {
                SearchedCriteria.Add(criteria);
                var books = findBooks(criteria);

                return Task.FromResult(new PagedResult<BookSummary>(books, books.Count, page, pageSize));
            }

            public Task<AuthorDetails?> GetAuthorAsync(string authorId, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<PagedResult<BookSummary>> GetAuthorWorksAsync(string authorId, int page, int pageSize, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<WorkDetails?> GetWorkAsync(string workId, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<PagedResult<EditionSummary>> GetWorkEditionsAsync(string workId, int page, int pageSize, CancellationToken cancellationToken) => throw new NotSupportedException();
        }
    }
}
