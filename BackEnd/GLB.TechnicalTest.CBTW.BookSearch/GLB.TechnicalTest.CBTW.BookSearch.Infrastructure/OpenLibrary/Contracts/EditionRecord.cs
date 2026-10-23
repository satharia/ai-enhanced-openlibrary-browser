using System.Text.Json.Serialization;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary.Contracts
{
    internal sealed record EditionRecord(
        string Key,
        string? Title,
        List<string>? Publishers,

        [property: JsonPropertyName("publish_date")]
        string? PublishDate,

        [property: JsonPropertyName("number_of_pages")]
        int? NumberOfPages,

        [property: JsonPropertyName("isbn_13")]
        string[]? Isbn13,

        List<KeyRef>? Languages,
        long[]? Covers
        );
}
