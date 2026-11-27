using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.GeminiAI.Models
{
    internal sealed record ExtractedCriteria
    (
        SearchIntent Intent,
        string? Title,
        string? Author,
        string? Subject,
        string? Publisher,
        string? Language,
        int? PublishedFrom,
        int? PublishedTo,
        string? FreeText
    );
}
