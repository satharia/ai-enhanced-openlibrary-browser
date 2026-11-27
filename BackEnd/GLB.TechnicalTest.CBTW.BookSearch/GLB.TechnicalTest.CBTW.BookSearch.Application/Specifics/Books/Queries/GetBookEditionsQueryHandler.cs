using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Queries
{
    public sealed class GetBookEditionsQueryHandler(IBookCatalog bookCatalog)
        : IRequestHandler<GetBookEditionsQuery, BookEditionsResult?>
    {
        public async Task<BookEditionsResult?> Handle(GetBookEditionsQuery request, CancellationToken cancellationToken)
        {
            var work = await bookCatalog.GetWorkAsync(request.WorkId, cancellationToken);
            if (work is null)
                return null;

            var editions = await bookCatalog.GetWorkEditionsAsync(request.WorkId, request.Page, request.PageSize, cancellationToken);

            return new BookEditionsResult(work, editions);
        }
    }
}
