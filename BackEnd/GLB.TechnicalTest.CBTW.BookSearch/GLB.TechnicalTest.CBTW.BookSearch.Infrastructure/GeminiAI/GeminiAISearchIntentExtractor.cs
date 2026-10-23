using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces;
using GLB.TechnicalTest.CBTW.BookSearch.Domain.Books;
using GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.GeminiAI.Models;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.GeminiAI
{
    internal sealed class GeminiAISearchIntentExtractor(IGeminiAIClient geminiAIClient) : ISearchIntentExtractor
    {
        private const string SystemPrompt = """
            I want you to classify a user's free-text book search into structured fields for a digital library's book catalog search.
            Fill only the fields you are confident about. Put anything you cannot classify into the freeText field.
            Rules:
            - intent: "author" ONLY when the entire input is a person's name and nothing else (eg. "jk rowling", "jrr martin", "tolkien", "juarismi").
              Any title words, genre, subject, era or description should be considered "book" descriptors.
            - title: the book's title or parts of it, users can sometimes provide only the begining of the whole book's title. Keep every word that seems to belong
              to a title together ("harry potter stone" and "harry potter stone 3d book" are different works in OpenLibrary's catalog)
            - author: the author's name, only when it is complete enough to identify a person (a surname or a full name, eg. "rowling", "jk rowling"). A fragment such as "jk row" is NOT an author, leave it in the freeText field.
            - subject: a genre or topic such as "science fiction", "cooking", "roman history".
            - publisher: only if mentioned explicitly.
            - language: Three letter code, only when the user explicitly names a language or asks for books in a specific one. Never infer it from the language the user writes in. Leave it otherwise.
            - publishedFrom and publishedTo: set it as integer years, infer it from messages from the user like these (eg. "from the 90" means 1990 to 1999,
              "before 2000" means from null to 1999, and so on)
            - freeText: any text or words that could not be classified to be placed in a field from the jsonSchema I provide.
            - NEVER make up any names of titles or authors. Make inferences but dont modify the user provided names to complete them.
            - If the whole user message is vague, place it as is in the freeText field.
        """;

        public async Task<BookSearchCriteria?> ExtractAsync(string freeText, CancellationToken cancellationToken)
        {
            var dto = await geminiAIClient.IssueGenerationRequestAsync<ExtractedCriteria>(SystemPrompt, freeText, cancellationToken);
            if (dto is null) return null;

            return new BookSearchCriteria(
                dto.Title,
                dto.Author,
                dto.Subject,
                dto.Publisher,
                dto.Language,
                dto.PublishedFrom,
                dto.PublishedTo,
                dto.FreeText
                );
        }
    }
}
