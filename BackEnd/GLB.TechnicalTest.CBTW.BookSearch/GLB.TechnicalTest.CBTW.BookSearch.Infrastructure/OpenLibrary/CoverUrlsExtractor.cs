using GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary.Contracts;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary
{
    internal static class CoverUrlsExtractor
    {
        private const string Host = "https://covers.openlibrary.org";

        public static string? ById(long? coverId, char size = 'M') =>
            coverId is > 0 ? $"{Host}/b/id/{coverId}-{size}.jpg" : null;

        public static string? FirstPositive(long[]? coverIds, char size = 'M') =>
            ById(coverIds?.FirstOrDefault(id => id > 0), size);

        public static string? ForSearchDoc(SearchDoc doc, char size = 'M') =>
            ById(doc.CoverI, size) ?? (doc.CoverEditionKey is { Length: > 0 } olid 
                ? $"{Host}/b/olid/{olid}-{size}.jpg?default=false" 
                : null);
    }
}
