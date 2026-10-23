namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models
{
    public sealed record RankedBook(BookSummary Book, bool IsPrimary, string Explanation);
}
