namespace GLB.TechnicalTest.CBTW.BookSearch.Domain.Books
{
    public sealed record BookSearchCriteria(
        string? Title,
        string? Author,
        string? Subject,
        string? Publisher,
        string? Language,
        int? PublishedFrom,
        int? PublishedTo,
        string? FreeText)
    {
        public SearchIntent Intent { get; init; } = SearchIntent.Books;
    }
}
