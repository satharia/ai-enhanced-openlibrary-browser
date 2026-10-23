namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary.Contracts
{
    internal sealed record SearchResponse(int NumFound, List<SearchDoc> Docs);
}
