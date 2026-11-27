namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Authors.Models
{
    public sealed record AuthorDetails(
        string Id,
        string Name,
        string? Bio,
        string? BirthDate,
        string? DeathDate,
        string? PhotoUrl
        );
}
