using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Rankers.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;
using System.Text.RegularExpressions;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models
{
    public static class MatchRanker
    {
        public static Ranking Rank(BookSearchCriteria criteria, IReadOnlyList<BookSummary> pool)
        {
            var title = Normalize(criteria.Title ?? criteria.FreeText);
            var author = Normalize(criteria.Author);

            var scored = pool.Select(book =>
            {
                var names = book.Authors.Select(author => Normalize(author.Name)).ToList();
                var authorMatch = author.Length == 0 ? 1                           // not requested
                    : names.Count > 0 && Has(names[0], author) ? 3                 // primary author: strongest
                    : names.Skip(1).Any(n => Has(n, author)) ? 2                   // contributor: weaker
                    : 0;
                return (book, sim: Similarity(Normalize(book.Title), title), authorMatch);
            })
                .OrderByDescending(x => x.sim).ThenByDescending(x => x.authorMatch).ThenByDescending(x => x.book.EditionCount ?? 0)
                .ToList();

            var (tier, pick) =
                scored.FirstOrDefault(x => x.sim >= 1 && x.authorMatch is 1 or 3) is { book: not null } t1 ? (MatchTier.ExactTitleAndPrimaryAuthor, t1) :
                scored.FirstOrDefault(x => x.sim >= 1 && x.authorMatch == 2) is { book: not null } t2 ? (MatchTier.ExactTitleAndContributor, t2) :
                scored.Where(x => x.sim >= 0.5 && x.authorMatch != 0).OrderByDescending(x => x.authorMatch).ThenByDescending(x => x.sim).FirstOrDefault() is { book: not null } t3 ? (MatchTier.NearMatch, t3) :
                (MatchTier.NoClearWinner, default);

            var ordered = pick.book is null ? scored : scored.Where(x => x.book != pick.book).Prepend(pick);

            var summary = tier switch
            {
                MatchTier.ExactTitleAndPrimaryAuthor => $"Exact title match: '{pick.book!.Title}'.",
                MatchTier.ExactTitleAndContributor => $"Exact title match: '{pick.book!.Title}'; '{criteria.Author}' is a contributor, the primary author is {pick.book.Authors[0].Name}.",
                MatchTier.NearMatch => $"No exact title; closest is '{pick.book!.Title}'.",
                _ => "No candidate matched the requested title.",
            };

            var items = ordered.Select(x => x.book with
            {
                IsPrimary = x.book == pick.book,
                Explanation =
                    (x.sim >= 1 ? "Exact title" : x.sim >= 0.5 ? $"Similar title ({x.sim:0.00})" : "Title does not match") +
                    (x.authorMatch switch
                    {
                        2 => "; requested author is a contributor.",
                        3 => $"; primary author {x.book.Authors[0].Name} matches.",
                        0 => "; author does not match.",
                        _ => ".",
                    }),
                }).ToList();

            return new Ranking(tier, summary, items);
        }

        private static string Normalize(string? input)
        {
            var normalized = Regex.Replace((input ?? "").ToLowerInvariant(), @"[^\p{L}\p{N}]+", " ").Trim();
            return normalized.StartsWith("the ") ? normalized[4..] : normalized;
        }

        private static bool Has(string candidate, string wanted) => wanted.Split(' ').All(candidate.Split(' ').Contains);

        private static double Similarity(string a, string b)
        {
            if (a.Length == 0 || b.Length == 0) return 0;
            if (a == b) return 1;
            var ta = a.Split(' ').ToHashSet();
            var tb = b.Split(' ').ToHashSet();
            var inter = ta.Intersect(tb).Count();
            if (inter == 0) return 0;
            var jaccard = (double)inter / ta.Union(tb).Count();
            return ta.IsSubsetOf(tb) || tb.IsSubsetOf(ta) ? Math.Max(jaccard, 0.6) : jaccard;
        }
    }
}
