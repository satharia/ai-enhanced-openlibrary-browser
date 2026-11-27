using GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary.Contracts;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary
{
    internal static class CoverUrlsExtractor
    {
        private const string Host = "https://covers.openlibrary.org";

        public static string? ById(long? coverId, char size = 'M') =>
            coverId is > 0 ? $"{Host}/b/id/{coverId}-{size}.jpg" : null;

        // Open Library uses -1 as a placeholder for removed covers, so the first valid id is taken
        public static string? FirstPositive(long[]? coverIds, char size = 'M') =>
            ById(coverIds?.FirstOrDefault(id => id > 0), size);

        public static string? ForSearchDoc(SearchDoc doc, char size = 'M') =>
            ById(doc.CoverI, size) ?? (doc.CoverEditionKey is { Length: > 0 } olid
                ? $"{Host}/b/olid/{olid}-{size}.jpg?default=false"
                : null);

        public static string? ForAuthorPhoto(long[]? photoIds, char size = 'M')
        {
            var photoId = photoIds?.FirstOrDefault(id => id > 0);

            return photoId is > 0
                ? $"{Host}/a/id/{photoId}-{size}.jpg"
                : null;
        }
    }
}
