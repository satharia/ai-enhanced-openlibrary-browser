using GLB.TechnicalTest.CBTW.BookSearch.Domain.Common.Utils.Http;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary.Contracts
{
    internal sealed record SearchRequest
    {
        // OpenLibrary asks it's users to specify the fields to get on every request
        public const string SearchFields = "key,title,author_name,author_key,first_publish_year,edition_count,cover_i,cover_edition_key";

        public string? Q { get; init; }
        public string? Title { get; init; }
        public string? Author { get; init; }
        public string? Subject { get; init; }
        public string? Publisher { get; init; }
        public string? Isbn { get; init; }

        public string? AuthorKey { get; init; }

        public string? Sort { get; init; }

        public int Page { get; init; } = 1;
        public int Limit { get; init; } = 20;
        public string Fields { get; init; } = SearchFields;

        public string BuildRequestPathWithParameters() =>
            QueryParameterUtils.BuildPathWithQueryParameterString(
                "search.json",
                [
                    new("q", Q),
                    new("title", Title),
                    new("author", Author),
                    new("subject", Subject),
                    new("publisher", Publisher),
                    new("isbn", Isbn),
                    new("author_key", AuthorKey),
                    new("sort", Sort),
                    new("page", Page.ToString()),
                    new("limit", Limit.ToString()),
                    new("fields", Fields)
                ]);
    }
}
