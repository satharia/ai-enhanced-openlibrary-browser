using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Common.Exceptions.Domain;
using Microsoft.Extensions.Logging;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Queries
{
    public sealed class SearchBookQueryHandler(
        ISearchIntentExtractor searchIntentExtractor,
        IBookCatalog bookCatalog,
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

            return new SearchBookResult(criteria!, false, books);
        }
    }
}
