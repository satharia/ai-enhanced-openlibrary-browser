using System.Text.Json.Serialization;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary.Contracts
{
    internal sealed record WorkRecord(
        string Key,
        string Title,

        [property: JsonConverter(typeof(TextOrObjectConverter))]
        string? Description,

        List<string>? Subjects,
        long[]? Covers,
        List<WorkAuthorRef>? Authors
        );
}
