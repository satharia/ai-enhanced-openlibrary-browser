namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models
{
    public sealed record WorkDetails(
        string Id,
        string Title,
        string? Description,
        IReadOnlyList<string> Subjects,
        string? CoverUrl
        );
}
