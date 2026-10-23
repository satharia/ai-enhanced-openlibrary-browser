using System.Text.Json.Nodes;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.GeminiAI
{
    internal interface IGeminiAIClient
    {
        Task<T?> IssueGenerationRequestAsync<T>(string systemPrompt, string userText, CancellationToken cancellationToken) where T : class;
    }
}
