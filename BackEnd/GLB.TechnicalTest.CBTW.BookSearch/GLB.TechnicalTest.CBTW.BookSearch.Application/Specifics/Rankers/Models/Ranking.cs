using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Rankers.Models
{
    public sealed record Ranking(MatchTier Tier, string Summary, IReadOnlyList<BookSummary> Books);
}
