using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Authors.Queries
{
    public sealed class GetAuthorWorksQueryHandler(IBookCatalog bookCatalog)
        : IRequestHandler<GetAuthorWorksQuery, AuthorWorksResult?>
    {
        public async Task<AuthorWorksResult?> Handle(GetAuthorWorksQuery request, CancellationToken cancellationToken)
        {
            var author = await bookCatalog.GetAuthorAsync(request.AuthorId, cancellationToken);
            if (author is null)
                return null;

            // Works come sorted by edition count, popular books tend to be published again and again
            var works = await bookCatalog.GetAuthorWorksAsync(request.AuthorId, request.Page, request.PageSize, cancellationToken);

            return new AuthorWorksResult(author, works);
        }
    }
}
