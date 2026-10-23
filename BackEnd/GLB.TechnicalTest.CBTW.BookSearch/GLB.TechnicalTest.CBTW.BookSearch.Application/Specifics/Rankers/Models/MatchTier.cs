namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Rankers.Models
{
    public enum MatchTier
    {
        ExactTitleAndPrimaryAuthor = 1,
        ExactTitleAndContributor = 2,
        NearMatch = 3,
        AuthorOnly = 4,
        NoClearWinner = 5
    }
}
