using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Rankers.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Common.Exceptions.Domain;
using Microsoft.Extensions.Logging;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Queries
{
    public sealed class SearchBookQueryHandler(
        ISearchIntentExtractor searchIntentExtractor,
        IBookCatalog bookCatalog,
        IResultRanker resultRanker,
        ILogger<SearchBookQueryHandler> logger)
        : IRequestHandler<SearchBookQuery, SearchBookResult>
    {
        public async Task<SearchBookResult> Handle(SearchBookQuery request, CancellationToken cancellationToken)
        {
            BookSearchCriteria? criteria = null;

            try
            {
                criteria = (await searchIntentExtractor.ExtractAsync(request.FreeText, cancellationToken))?.GetNormalizedCriteria();
            }
            catch (DomainException ex)
            {
                logger.LogWarning("Invalid criteria generated with reason: {Error}; querying with free text directly", ex.Message);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning("Intent extraction failed; querying with free text directly");
            }

            var usedFallback = criteria is null || !criteria.HasStructuredFields;
            if (usedFallback)
            {
                criteria = BookSearchCriteria.FreeTextCriteria(request.FreeText);
            }

            var books = await bookCatalog.SearchWorksAsync(criteria!, request.Page, request.PageSize, cancellationToken);
            if (books.Items.Count == 0 && !usedFallback)
            {
                logger.LogWarning("Structured search returned empty; retrying as free text");
                books = await bookCatalog.SearchWorksAsync(BookSearchCriteria.FreeTextCriteria(request.FreeText), request.Page, request.PageSize, cancellationToken);
                usedFallback = true;
            }

            Ranking ranking;
            try
            {
                ranking = await resultRanker.RankAsync(criteria!, books.Items, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Ranking failed; using the deterministic tiers");
                ranking = MatchRanker.Rank(criteria!, books.Items);
            }

            books = books with { Items = ranking.Books };

            return new SearchBookResult(criteria!, usedFallback, ranking.Tier, ranking.Summary, books);
        }
    }
}
