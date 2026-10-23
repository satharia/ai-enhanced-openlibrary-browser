using System.Text.Json.Serialization;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary.Contracts
{
    internal sealed record AuthorDoc(
        string Key,
        string Name,

        [property: JsonPropertyName("top_work")]
        string? TopWork,

        [property: JsonPropertyName("work_count")]
        int workCount
        );
}
