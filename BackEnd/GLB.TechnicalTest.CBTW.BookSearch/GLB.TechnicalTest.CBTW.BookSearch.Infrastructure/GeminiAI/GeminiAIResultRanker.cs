using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Rankers;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Rankers.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;
using GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.GeminiAI.Models;
using System.Text.Json;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.GeminiAI
{
    internal sealed class GeminiAIResultRanker(IGeminiAIClient geminiAIClient) : IResultRanker
    {
        private const int MaxCandidates = 20;
        private const int ShortlistSize = 5;

        private const string SystemPrompt = """
            I need you to rank Open Library search results against what a user asked for. A deterministic matcher found no exact
            title match. Each candidate's "authors" array is in catalog order: the first entry is the primary author, later entries may be contributors.
            If "leadingKey" is set, that candidate is the closest title found;
            confirm it as primaryKey or replace it, and state the assumption you made. If no single candidate is clearly the work meant, set primaryKey to null.
            Return primaryKey, a one-sentence summary, and ranked: up to 5 candidates best first, each with a one-sentence
            explanation naming the evidence (title words, author, year, edition count). Use only keys from the list.
        """;

        public async Task<Ranking> RankAsync(BookSearchCriteria criteria, IReadOnlyList<BookSummary> candidates, CancellationToken cancellationToken)
        {
            var deterministicRanking = MatchRanker.Rank(criteria, candidates);

            // These tiers are decided by plain string comparisons on data already in hand, no need to ask GeminiAI
            var isDecidedWithoutGemini = deterministicRanking.Tier
                is MatchTier.ExactTitleAndPrimaryAuthor
                or MatchTier.ExactTitleAndContributor
                or MatchTier.AuthorOnly;

            if (isDecidedWithoutGemini || candidates.Count == 0)
                return deterministicRanking;

            var candidatesToRank = deterministicRanking.Books.Take(MaxCandidates).ToList();
            var candidatesById = candidatesToRank.ToDictionary(book => book.Id);

            var userText = BuildUserText(criteria, deterministicRanking, candidatesToRank);
            var geminiRanking = await geminiAIClient.IssueGenerationRequestAsync<RankingDto>(SystemPrompt, userText, cancellationToken);
            if (geminiRanking is null)
                return deterministicRanking;

            // GeminiAI can only pick from the keys it was given, anything else is ignored
            var primaryId = geminiRanking.PrimaryKey is not null && candidatesById.ContainsKey(geminiRanking.PrimaryKey)
                ? geminiRanking.PrimaryKey
                : null;

            var tier = primaryId is null
                ? MatchTier.NoClearWinner
                : deterministicRanking.Tier;

            var rankedBooks = geminiRanking.Ranked
                .Where(rankedBook => candidatesById.ContainsKey(rankedBook.Key))
                .DistinctBy(rankedBook => rankedBook.Key)
                .Select(rankedBook => candidatesById[rankedBook.Key] with
                {
                    IsPrimary = rankedBook.Key == primaryId,
                    Explanation = rankedBook.Explanation
                })
                .ToList();

            // Books GeminiAI left out keep the deterministic order after the ones it ranked
            var rankedIds = rankedBooks.Select(book => book.Id).ToHashSet();
            rankedBooks.AddRange(deterministicRanking.Books
                .Where(book => !rankedIds.Contains(book.Id))
                .Select(book => book with { IsPrimary = false }));

            var books = tier == MatchTier.NoClearWinner
                ? rankedBooks.Take(ShortlistSize).ToList()
                : rankedBooks;

            return new Ranking(tier, geminiRanking.Summary, books);
        }

        private static string BuildUserText(BookSearchCriteria criteria, Ranking deterministicRanking, List<BookSummary> candidatesToRank)
        {
            var request = new
            {
                request = new
                {
                    criteria.Title,
                    criteria.Author,
                    criteria.Subject,
                    criteria.Language,
                    criteria.PublishedFrom,
                    criteria.PublishedTo,
                    criteria.FreeText
                },
                leadingKey = deterministicRanking.Tier == MatchTier.NearMatch ? deterministicRanking.Books[0].Id : null,
                candidates = candidatesToRank.Select(book => new
                {
                    book.Id,
                    book.Title,
                    authors = book.Authors.Select(author => author.Name),
                    book.FirstPublishYear,
                    book.EditionCount
                })
            };

            return JsonSerializer.Serialize(request, JsonSerializerOptions.Web);
        }
    }
}
