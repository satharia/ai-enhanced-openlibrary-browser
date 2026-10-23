using System.Text.Json.Serialization;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary.Contracts
{
    internal sealed record SearchDoc(
        string Key,
        string Title,

        [property: JsonPropertyName("author_name")]
        string[]? AuthorName,

        [property: JsonPropertyName("author_key")]
        string[]? AuthorKey,

        [property: JsonPropertyName("first_publish_year")]
        int? FirstPublishYear,

        [property: JsonPropertyName("edition_count")]
        int? EditionCount,

        [property: JsonPropertyName("cover_i")]
        long? CoverI,

        [property: JsonPropertyName("cover_edition_key")]
        string? CoverEditionKey
        );
}
