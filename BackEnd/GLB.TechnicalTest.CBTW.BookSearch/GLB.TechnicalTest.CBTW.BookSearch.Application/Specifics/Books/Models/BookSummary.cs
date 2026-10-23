namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models
{
    public sealed record BookSummary(
        string Id,
        string Title,
        IReadOnlyList<AuthorRef> Authors,
        int? FirstPublishYear,
        int? EditionCount,
        string? CoverUrl,
        string? EditionId = null
        )
    {
        public bool IsPrimary { get; init; }
        public string? Explanation { get; init; }
    }
}
