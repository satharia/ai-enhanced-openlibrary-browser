namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary.Contracts
{
    internal sealed record AuthorSearchResponse(
        int NumFound,
        List<AuthorDoc> Docs
        );
}
