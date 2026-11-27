namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models
{
    public sealed record EditionSummary(
        string Id,
        string? Title,
        IReadOnlyList<string> Publishers,
        string? PublishDate,
        int? NumberOfPages,
        string? Isbn13,
        IReadOnlyList<string> Languages,
        string? CoverUrl
        );
}
