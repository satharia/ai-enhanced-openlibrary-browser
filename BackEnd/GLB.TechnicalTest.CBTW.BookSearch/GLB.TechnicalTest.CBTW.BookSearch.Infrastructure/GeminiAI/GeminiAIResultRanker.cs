using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Rankers.Models;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;
using GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.GeminiAI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.GeminiAI
{
    internal sealed class GeminiAIResultRanker(IGeminiAIClient gemini) : IResultRanker
    {
        private const int MaxCandidates = 20;
        private const int Shortlist = 5;

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
            var det = MatchRanker.Rank(criteria, candidates);

            // Tiers 1 and 2 are string equality on data already in hand: no GeminiAI call.
            if (det.Tier is MatchTier.ExactTitleAndPrimaryAuthor or MatchTier.ExactTitleAndContributor || candidates.Count == 0)
                return det;

            var input = det.Books.Take(MaxCandidates).ToList();
            var byKey = input.ToDictionary(b => b.Id);

            var userText = JsonSerializer.Serialize(new
            {
                request = new { criteria.Title, criteria.Author, criteria.Subject, criteria.Language, criteria.PublishedFrom, criteria.PublishedTo, criteria.FreeText },
                leadingKey = det.Tier == MatchTier.NearMatch ? det.Books[0].Id : null,
                candidates = input.Select(b => new { b.Id, b.Title, authors = b.Authors.Select(a => a.Name), b.FirstPublishYear, b.EditionCount }),
            }, JsonSerializerOptions.Web);

            var dto = await gemini.IssueGenerationRequestAsync<RankingDto>(SystemPrompt, userText, cancellationToken);
            if (dto is null) return det;

            var primary = dto.PrimaryKey is not null && byKey.ContainsKey(dto.PrimaryKey) ? dto.PrimaryKey : null;
            var tier = primary is null ? MatchTier.NoClearWinner : det.Tier;

            var ranked = dto.Ranked
                .Where(r => byKey.ContainsKey(r.Key))
                .DistinctBy(r => r.Key)
                .Select(r => byKey[r.Key] with { IsPrimary = r.Key == primary, Explanation = r.Explanation })
                .ToList();

            var mentioned = ranked.Select(b => b.Id).ToHashSet();
            ranked.AddRange(det.Books.Where(b => !mentioned.Contains(b.Id)).Select(b => b with { IsPrimary = false }));

            return new Ranking(tier, dto.Summary, tier == MatchTier.NoClearWinner ? ranked.Take(Shortlist).ToList() : ranked);
        }
    }
}
