namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.GeminiAI.Models
{
    internal sealed record RankingDto(string? PrimaryKey, string Summary, List<RankedDto> Ranked);
}
