using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Rankers.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;
using System.Text.RegularExpressions;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Rankers
{
    // Deterministic ranking: every candidate is scored by how close its title is to the wanted title and by
    // where the wanted author appears in its author list (Open Library lists the primary author first).
    public static class MatchRanker
    {
        private const double SimilarTitleThreshold = 0.5;
        private const double ContainedTitleSimilarity = 0.6;

        private static readonly Regex NonLetterOrDigitCharacters = new(@"[^\p{L}\p{N}]+", RegexOptions.Compiled);

        public static Ranking Rank(BookSearchCriteria criteria, IReadOnlyList<BookSummary> candidates)
        {
            var wantedTitle = Normalize(criteria.WantedTitle);
            var wantedAuthor = Normalize(criteria.Author);

            var scoredBooks = candidates
                .Select(book => ScoreBook(book, wantedTitle, wantedAuthor))
                .OrderByDescending(scoredBook => scoredBook.TitleSimilarity)
                .ThenByDescending(scoredBook => scoredBook.AuthorMatch)
                .ThenByDescending(scoredBook => scoredBook.Book.EditionCount ?? 0)
                .ToList();

            var (tier, bestMatch) = wantedTitle.Length == 0
                ? FindAuthorOnlyMatch(scoredBooks)
                : FindTitleMatch(scoredBooks);

            var rankedBooks = bestMatch is null
                ? scoredBooks
                : scoredBooks.Where(scoredBook => scoredBook.Book.Id != bestMatch.Book.Id).Prepend(bestMatch);

            var books = rankedBooks
                .Select(scoredBook => scoredBook.Book with
                {
                    IsPrimary = scoredBook.Book.Id == bestMatch?.Book.Id,
                    Explanation = Explain(scoredBook, isTitleWanted: wantedTitle.Length > 0)
                })
                .ToList();

            return new Ranking(tier, Summarize(tier, bestMatch?.Book, criteria.Author), books);
        }

        private static (MatchTier Tier, ScoredBook? BestMatch) FindTitleMatch(List<ScoredBook> scoredBooks)
        {
            var exactTitleAndPrimaryAuthor = scoredBooks.FirstOrDefault(scoredBook =>
                scoredBook.HasExactTitle && scoredBook.AuthorMatch is AuthorMatch.NotRequested or AuthorMatch.PrimaryAuthor);
            if (exactTitleAndPrimaryAuthor is not null)
                return (MatchTier.ExactTitleAndPrimaryAuthor, exactTitleAndPrimaryAuthor);

            var exactTitleAndContributor = scoredBooks.FirstOrDefault(scoredBook =>
                scoredBook.HasExactTitle && scoredBook.AuthorMatch == AuthorMatch.Contributor);
            if (exactTitleAndContributor is not null)
                return (MatchTier.ExactTitleAndContributor, exactTitleAndContributor);

            // A matching author weighs more than a slightly closer title
            var nearMatch = scoredBooks
                .Where(scoredBook => scoredBook.HasSimilarTitle && scoredBook.AuthorMatch != AuthorMatch.None)
                .OrderByDescending(scoredBook => scoredBook.AuthorMatch)
                .ThenByDescending(scoredBook => scoredBook.TitleSimilarity)
                .FirstOrDefault();

            return nearMatch is null
                ? (MatchTier.NoClearWinner, null)
                : (MatchTier.NearMatch, nearMatch);
        }

        // No title was asked for: the author's work with the most editions is the best guess of what the user wants
        private static (MatchTier Tier, ScoredBook? BestMatch) FindAuthorOnlyMatch(List<ScoredBook> scoredBooks)
        {
            var mostPublishedWork = scoredBooks.FirstOrDefault(scoredBook => scoredBook.AuthorMatch == AuthorMatch.PrimaryAuthor);

            return mostPublishedWork is null
                ? (MatchTier.NoClearWinner, null)
                : (MatchTier.AuthorOnly, mostPublishedWork);
        }

        private static ScoredBook ScoreBook(BookSummary book, string wantedTitle, string wantedAuthor)
        {
            var titleSimilarity = TitleSimilarity(Normalize(book.Title), wantedTitle);

            return new ScoredBook(book, titleSimilarity, MatchAuthor(book, wantedAuthor));
        }

        private static AuthorMatch MatchAuthor(BookSummary book, string wantedAuthor)
        {
            if (wantedAuthor.Length == 0)
                return AuthorMatch.NotRequested;

            var authorNames = book.Authors.Select(author => Normalize(author.Name)).ToList();

            if (authorNames.Count > 0 && ContainsAllWords(authorNames[0], wantedAuthor))
                return AuthorMatch.PrimaryAuthor;

            return authorNames.Skip(1).Any(authorName => ContainsAllWords(authorName, wantedAuthor))
                ? AuthorMatch.Contributor
                : AuthorMatch.None;
        }

        private static double TitleSimilarity(string bookTitle, string wantedTitle)
        {
            if (bookTitle.Length == 0 || wantedTitle.Length == 0)
                return 0;

            if (bookTitle == wantedTitle)
                return 1;

            var bookTitleWords = bookTitle.Split(' ').ToHashSet();
            var wantedTitleWords = wantedTitle.Split(' ').ToHashSet();

            var sharedWordCount = bookTitleWords.Intersect(wantedTitleWords).Count();
            if (sharedWordCount == 0)
                return 0;

            // Jaccard index: shared words over every distinct word of both titles
            var similarity = (double)sharedWordCount / bookTitleWords.Union(wantedTitleWords).Count();

            // A title fully contained in the other ("hobbit" in "hobbit or there and back again") is at least similar
            var isOneTitleInsideTheOther = bookTitleWords.IsSubsetOf(wantedTitleWords) || wantedTitleWords.IsSubsetOf(bookTitleWords);

            return isOneTitleInsideTheOther
                ? Math.Max(similarity, ContainedTitleSimilarity)
                : similarity;
        }

        private static bool ContainsAllWords(string text, string wantedWords)
        {
            var textWords = text.Split(' ');

            return wantedWords.Split(' ').All(textWords.Contains);
        }

        private static string Normalize(string? text)
        {
            var normalized = NonLetterOrDigitCharacters.Replace((text ?? "").ToLowerInvariant(), " ").Trim();

            // Open Library is inconsistent with leading articles ("The Hobbit" and "Hobbit" are both used)
            return normalized.StartsWith("the ")
                ? normalized[4..]
                : normalized;
        }

        private static string Explain(ScoredBook scoredBook, bool isTitleWanted)
        {
            var titleExplanation =
                !isTitleWanted ? $"{scoredBook.Book.EditionCount ?? 0} editions" :
                scoredBook.HasExactTitle ? "Exact title" :
                scoredBook.HasSimilarTitle ? $"Similar title ({scoredBook.TitleSimilarity:0.00})" :
                "Title does not match";

            var authorExplanation = scoredBook.AuthorMatch switch
            {
                AuthorMatch.PrimaryAuthor => $"; primary author {scoredBook.Book.Authors[0].Name} matches.",
                AuthorMatch.Contributor => "; requested author is a contributor.",
                AuthorMatch.None => "; author does not match.",
                _ => "."
            };

            return titleExplanation + authorExplanation;
        }

        private static string Summarize(MatchTier tier, BookSummary? bestMatch, string? wantedAuthor) => tier switch
        {
            MatchTier.ExactTitleAndPrimaryAuthor => $"Exact title match: '{bestMatch!.Title}'.",
            MatchTier.ExactTitleAndContributor => $"Exact title match: '{bestMatch!.Title}'; '{wantedAuthor}' is a contributor, the primary author is {bestMatch.Authors[0].Name}.",
            MatchTier.NearMatch => $"No exact title; closest is '{bestMatch!.Title}'.",
            MatchTier.AuthorOnly => $"Works by '{wantedAuthor}'; the one with the most editions is '{bestMatch!.Title}'.",
            _ => "No candidate matched the requested title."
        };

        // Ordered from weakest to strongest so the enum value can be used to sort
        private enum AuthorMatch
        {
            None = 0,
            NotRequested = 1,
            Contributor = 2,
            PrimaryAuthor = 3
        }

        private sealed record ScoredBook(BookSummary Book, double TitleSimilarity, AuthorMatch AuthorMatch)
        {
            public bool HasExactTitle => TitleSimilarity >= 1;
            public bool HasSimilarTitle => TitleSimilarity >= SimilarTitleThreshold;
        }
    }
}
