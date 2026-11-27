using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Rankers;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Rankers.Models;
using static GLB.TechnicalTest.CBTW.BookSearch.Application.Tests.TestData.TestBooks;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Tests.Rankers
{
    public class MatchRankerTests
    {
        [Fact]
        public void Rank_ExactTitleByPrimaryAuthor_IsTheBestMatchEvenWhenListedLast()
        {
            var candidates = new[]
            {
                Book("OL1W", "The Hobbit Companion", 30, "David Day"),
                Book("OL2W", "J.R.R. Tolkien's The Hobbit", 12, "Patsey Gray"),
                Book("OL3W", "The Hobbit", 482, "J.R.R. Tolkien")
            };

            var ranking = MatchRanker.Rank(Criteria(title: "the hobbit", author: "tolkien"), candidates);

            Assert.Equal(MatchTier.ExactTitleAndPrimaryAuthor, ranking.Tier);
            Assert.Equal("OL3W", ranking.Books[0].Id);
            Assert.True(ranking.Books[0].IsPrimary);
        }

        [Fact]
        public void Rank_WantedAuthorIsOnlyAContributor_ReportsThePrimaryAuthor()
        {
            // The graphic novel lists Tolkien third, after the adapter and the illustrator
            var candidates = new[]
            {
                Book("OL1W", "The Hobbit", 25, "Charles Dixon", "Sean Deming", "J.R.R. Tolkien"),
                Book("OL2W", "The Hobbit Companion", 30, "David Day")
            };

            var ranking = MatchRanker.Rank(Criteria(title: "the hobbit", author: "tolkien"), candidates);

            Assert.Equal(MatchTier.ExactTitleAndContributor, ranking.Tier);
            Assert.Equal("OL1W", ranking.Books[0].Id);
            Assert.Contains("the primary author is Charles Dixon", ranking.Summary);
        }

        [Fact]
        public void Rank_SameTitleByPrimaryAuthorAndAsContributor_PrefersThePrimaryAuthorOverMoreEditions()
        {
            var candidates = new[]
            {
                Book("OL1W", "The Hobbit", 900, "Charles Dixon", "J.R.R. Tolkien"),
                Book("OL2W", "The Hobbit", 482, "J.R.R. Tolkien")
            };

            var ranking = MatchRanker.Rank(Criteria(title: "the hobbit", author: "tolkien"), candidates);

            Assert.Equal(MatchTier.ExactTitleAndPrimaryAuthor, ranking.Tier);
            Assert.Equal("OL2W", ranking.Books[0].Id);
        }

        [Fact]
        public void Rank_NoAuthorRequested_BreaksTitleTiesByEditionCount()
        {
            var candidates = new[]
            {
                Book("OL1W", "Dune", 4, "Somebody Else"),
                Book("OL2W", "Dune", 280, "Frank Herbert")
            };

            var ranking = MatchRanker.Rank(Criteria(title: "dune"), candidates);

            Assert.Equal(MatchTier.ExactTitleAndPrimaryAuthor, ranking.Tier);
            Assert.Equal("OL2W", ranking.Books[0].Id);
        }

        [Theory]
        [InlineData("hobbit")]
        [InlineData("THE HOBBIT")]
        [InlineData("the hobbit!!")]
        [InlineData("  the   hobbit ")]
        public void Rank_TitleWithDifferentCasePunctuationOrArticle_IsStillExact(string wantedTitle)
        {
            var candidates = new[] { Book("OL1W", "The Hobbit", 482, "J.R.R. Tolkien") };

            var ranking = MatchRanker.Rank(Criteria(title: wantedTitle), candidates);

            Assert.Equal(MatchTier.ExactTitleAndPrimaryAuthor, ranking.Tier);
        }

        [Fact]
        public void Rank_NoTitleExtracted_UsesTheFreeTextAsTitle()
        {
            var candidates = new[] { Book("OL1W", "The Hobbit", 482, "J.R.R. Tolkien") };

            var ranking = MatchRanker.Rank(Criteria(freeText: "the hobbit"), candidates);

            Assert.Equal(MatchTier.ExactTitleAndPrimaryAuthor, ranking.Tier);
        }

        [Fact]
        public void Rank_ExactTitleButAuthorNotListed_HasNoClearWinner()
        {
            var candidates = new[] { Book("OL1W", "The Hobbit", 482, "J.R.R. Tolkien") };

            var ranking = MatchRanker.Rank(Criteria(title: "the hobbit", author: "rowling"), candidates);

            Assert.Equal(MatchTier.NoClearWinner, ranking.Tier);
            Assert.DoesNotContain(ranking.Books, book => book.IsPrimary);
            Assert.EndsWith("author does not match.", ranking.Books[0].Explanation);
        }

        [Fact]
        public void Rank_TitleContainedInALongerTitle_IsANearMatch()
        {
            var candidates = new[]
            {
                Book("OL1W", "The Lord of the Rings: 50th Anniversary Edition", 20, "J.R.R. Tolkien"),
                Book("OL2W", "The Silmarillion", 200, "J.R.R. Tolkien")
            };

            var ranking = MatchRanker.Rank(Criteria(title: "lord of the rings", author: "tolkien"), candidates);

            Assert.Equal(MatchTier.NearMatch, ranking.Tier);
            Assert.Equal("OL1W", ranking.Books[0].Id);
            Assert.StartsWith("Similar title", ranking.Books[0].Explanation);
        }

        [Fact]
        public void Rank_NearMatches_PreferTheWantedPrimaryAuthorOverACloserTitle()
        {
            var candidates = new[]
            {
                Book("OL1W", "The Lord of the Rings Sketchbook", 5, "Alan Lee", "J.R.R. Tolkien"),
                Book("OL2W", "The Lord of the Rings: Deluxe Illustrated Edition in One Volume", 3, "J.R.R. Tolkien")
            };

            var ranking = MatchRanker.Rank(Criteria(title: "lord of the rings", author: "tolkien"), candidates);

            Assert.Equal(MatchTier.NearMatch, ranking.Tier);
            Assert.Equal("OL2W", ranking.Books[0].Id);
        }

        [Fact]
        public void Rank_OnlyAnAuthorRequested_PicksTheirWorkWithTheMostEditions()
        {
            var candidates = new[]
            {
                Book("OL1W", "The Silmarillion", 200, "J.R.R. Tolkien", "Christopher Tolkien"),
                Book("OL2W", "Tolkien: A Biography", 900, "Humphrey Carpenter", "J.R.R. Tolkien"),
                Book("OL3W", "The Hobbit", 482, "J.R.R. Tolkien")
            };

            var ranking = MatchRanker.Rank(Criteria(author: "tolkien"), candidates);

            Assert.Equal(MatchTier.AuthorOnly, ranking.Tier);
            Assert.Equal("OL3W", ranking.Books[0].Id);
            Assert.Equal("482 editions; primary author J.R.R. Tolkien matches.", ranking.Books[0].Explanation);
        }

        [Fact]
        public void Rank_PartialAuthorWord_DoesNotMatch()
        {
            var candidates = new[] { Book("OL1W", "The Hobbit", 482, "J.R.R. Tolkien") };

            var ranking = MatchRanker.Rank(Criteria(title: "the hobbit", author: "tolk"), candidates);

            Assert.Equal(MatchTier.NoClearWinner, ranking.Tier);
        }

        [Fact]
        public void Rank_BookWithoutAuthors_IsScoredWithoutFailing()
        {
            var candidates = new[]
            {
                Book("OL1W", "The Hobbit", 3),
                Book("OL2W", "The Hobbit", 482, "J.R.R. Tolkien")
            };

            var ranking = MatchRanker.Rank(Criteria(title: "the hobbit", author: "tolkien"), candidates);

            Assert.Equal("OL2W", ranking.Books[0].Id);
            Assert.Equal("Exact title; author does not match.", ranking.Books[1].Explanation);
        }

        [Fact]
        public void Rank_KeepsEveryCandidateWithOnlyOnePrimary()
        {
            var candidates = new[]
            {
                Book("OL1W", "The Hobbit", 482, "J.R.R. Tolkien"),
                Book("OL2W", "The Hobbit", 25, "Charles Dixon", "J.R.R. Tolkien"),
                Book("OL3W", "Farmer Giles of Ham", 90, "J.R.R. Tolkien")
            };

            var ranking = MatchRanker.Rank(Criteria(title: "the hobbit", author: "tolkien"), candidates);

            Assert.Equal(3, ranking.Books.Count);
            Assert.Single(ranking.Books, book => book.IsPrimary);
        }

        [Fact]
        public void Rank_NoCandidates_ReturnsAnEmptyRanking()
        {
            var ranking = MatchRanker.Rank(Criteria(title: "the hobbit"), []);

            Assert.Equal(MatchTier.NoClearWinner, ranking.Tier);
            Assert.Empty(ranking.Books);
        }
    }
}
