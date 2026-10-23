using GLB.TechnicalTest.CBTW.BookSearch.Domain.Common.Exceptions.Domain;

namespace GLB.TechnicalTest.CBTW.BookSearch.Domain.Books
{
    public sealed record BookSearchCriteria(
        string? Title,
        string? Author,
        string? Subject,
        string? Publisher,
        string? Language,
        int? PublishedFrom,
        int? PublishedTo,
        string? FreeText)
    {
        public SearchIntent Intent { get; init; } = SearchIntent.Books;

        public bool HasStructuredFields =>
            !string.IsNullOrWhiteSpace(Title) ||
            !string.IsNullOrWhiteSpace(Author) ||
            !string.IsNullOrWhiteSpace(Subject) ||
            !string.IsNullOrWhiteSpace(Publisher) ||
            PublishedFrom is not null ||
            PublishedTo is not null;

        public string? WantedTitle => Title ?? FreeText;

        public string? WantedAuthor => Author ?? FreeText;

        public static BookSearchCriteria FreeTextCriteria(string freeText) =>
            new(null, null, null, null, null, null, null, freeText);

        public BookSearchCriteria GetNormalizedCriteria()
        {
            if (PublishedFrom is not null && PublishedTo is not null && PublishedFrom > PublishedTo)
            {
                throw new DomainException("Invalid publish year range, FROM is greater than TO");
            }

            if (Intent == SearchIntent.Author && string.IsNullOrWhiteSpace(WantedAuthor))
            {
                throw new DomainException("Author intent must have an author name identified with it");
            }

            return this with
            {
                Title = Normalize(Title),
                Author = Normalize(Author),
                Subject = Normalize(Subject),
                Publisher = Normalize(Publisher),
                Language = Normalize(Language),
                FreeText = Normalize(FreeText)
            };
        }

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
    }
}
