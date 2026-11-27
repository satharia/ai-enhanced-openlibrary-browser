using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Caching;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Authors.Queries;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Queries;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Rankers.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using static GLB.TechnicalTest.CBTW.BookSearch.Application.Tests.TestData.TestBooks;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Tests.Caching
{
    public class CachingBehaviorTests
    {
        private readonly MemoryDistributedCache cache = new(Options.Create(new MemoryDistributedCacheOptions()));

        [Fact]
        public async Task Handle_SameCachedQueryTwice_RunsTheHandlerOnceAndReturnsTheSameResult()
        {
            var behavior = CreateBehavior<SearchBookQuery, SearchBookResult>(cache);
            var query = new SearchBookQuery("The Hobbit");
            var handlerCalls = 0;
            var handlerResult = new SearchBookResult(
                Criteria(title: "the hobbit"),
                UsedFallback: false,
                MatchTier.ExactTitleAndPrimaryAuthor,
                "Exact title match: 'The Hobbit'.",
                new PagedResult<Specifics.Books.Models.BookSummary>([Book("OL27482W", "The Hobbit", 482, "J.R.R. Tolkien")], 1, 1, 20));

            Task<SearchBookResult> Handler()
            {
                handlerCalls++;
                return Task.FromResult(handlerResult);
            }

            await behavior.Handle(query, Handler, CancellationToken.None);
            var cachedResult = await behavior.Handle(query, Handler, CancellationToken.None);

            Assert.Equal(1, handlerCalls);
            Assert.Equal(handlerResult.Summary, cachedResult.Summary);
            Assert.Equal(handlerResult.Tier, cachedResult.Tier);
            Assert.Equal(handlerResult.Criteria, cachedResult.Criteria);
            Assert.Equal(handlerResult.Books.Items[0].Title, cachedResult.Books.Items[0].Title);
            Assert.Equal("J.R.R. Tolkien", cachedResult.Books.Items[0].Authors[0].Name);
        }

        [Fact]
        public void SearchBookQuery_CacheKey_IgnoresCaseAndSurroundingSpaces()
        {
            Assert.Equal(new SearchBookQuery("the hobbit").CacheKey, new SearchBookQuery(" The Hobbit ").CacheKey);
        }

        [Fact]
        public async Task Handle_NotFoundResult_IsCachedAsNull()
        {
            var behavior = CreateBehavior<GetAuthorWorksQuery, AuthorWorksResult?>(cache);
            var handlerCalls = 0;

            Task<AuthorWorksResult?> Handler()
            {
                handlerCalls++;
                return Task.FromResult<AuthorWorksResult?>(null);
            }

            await behavior.Handle(new GetAuthorWorksQuery("OL1A"), Handler, CancellationToken.None);
            var cachedResult = await behavior.Handle(new GetAuthorWorksQuery("OL1A"), Handler, CancellationToken.None);

            Assert.Null(cachedResult);
            Assert.Equal(1, handlerCalls);
        }

        [Fact]
        public async Task Handle_RequestThatIsNotACachedQuery_AlwaysRunsTheHandler()
        {
            var behavior = CreateBehavior<UncachedQuery, string>(cache);
            var handlerCalls = 0;

            Task<string> Handler()
            {
                handlerCalls++;
                return Task.FromResult("result");
            }

            await behavior.Handle(new UncachedQuery(), Handler, CancellationToken.None);
            await behavior.Handle(new UncachedQuery(), Handler, CancellationToken.None);

            Assert.Equal(2, handlerCalls);
        }

        [Fact]
        public async Task Handle_CacheIsDown_StillAnswersTheRequest()
        {
            var behavior = CreateBehavior<CachedQuery, string>(new UnavailableCache());

            var result = await behavior.Handle(new CachedQuery(), () => Task.FromResult("result"), CancellationToken.None);

            Assert.Equal("result", result);
        }

        private static CachingBehavior<TRequest, TResponse> CreateBehavior<TRequest, TResponse>(IDistributedCache cache)
            where TRequest : IRequest<TResponse>
        {
            return new CachingBehavior<TRequest, TResponse>(cache, NullLogger<CachingBehavior<TRequest, TResponse>>.Instance);
        }

        private sealed record UncachedQuery : IRequest<string>;

        private sealed record CachedQuery : IRequest<string>, ICachedQuery
        {
            public string CacheKey => "q:test";
            public TimeSpan TTL => TimeSpan.FromMinutes(1);
        }

        // Behaves like a Redis server that cannot be reached
        private sealed class UnavailableCache : IDistributedCache
        {
            public byte[]? Get(string key) => throw new InvalidOperationException("Cache is down");
            public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => throw new InvalidOperationException("Cache is down");
            public void Refresh(string key) => throw new InvalidOperationException("Cache is down");
            public Task RefreshAsync(string key, CancellationToken token = default) => throw new InvalidOperationException("Cache is down");
            public void Remove(string key) => throw new InvalidOperationException("Cache is down");
            public Task RemoveAsync(string key, CancellationToken token = default) => throw new InvalidOperationException("Cache is down");
            public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => throw new InvalidOperationException("Cache is down");
            public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => throw new InvalidOperationException("Cache is down");
        }
    }
}
