namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary
{
    public sealed class OpenLibraryOptions
    {
        public const string Section = "OpenLibrary";

        public string BaseUrl { get; init; } = "https://openlibrary.org/";
        public string AppName { get; init; } = "";
        public string ContactEmail { get; init; } = "";
        public int RequestsPerSecond { get; init; } = 3;
        public string UserAgent => $"{AppName} ({ContactEmail})";
    }
}
