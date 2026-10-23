using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Common.Exceptions.Domain;
using Microsoft.Extensions.Logging;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Queries
{
    public sealed class SearchBookQueryHandler(
        ISearchIntentExtractor searchIntentExtractor,
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

            // ToDo: Use criteria to query books from OpenLibrary output port

            return new SearchBookResult(null, false, null);
        }
    }
}
