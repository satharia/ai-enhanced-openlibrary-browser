using System.Text.Json.Serialization;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary.Contracts
{
    internal sealed record AuthorRecord(
        string Key,
        string Name,

        [property: JsonConverter(typeof(TextOrObjectConverter))]
        string? Bio,

        [property: JsonPropertyName("birth_date")]
        string? BirthDate,

        [property: JsonPropertyName("death_date")]
        string? DeathDate,

        long[]? Photos
        );
}
