namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary.Contracts
{
    internal sealed record EditionsResponse(
        int Size,
        List<EditionRecord> Entries
        );
}
