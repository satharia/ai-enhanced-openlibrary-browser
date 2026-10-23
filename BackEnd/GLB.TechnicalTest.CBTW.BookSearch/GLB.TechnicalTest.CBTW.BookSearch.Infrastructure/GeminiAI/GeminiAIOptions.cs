namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.GeminiAI
{
    internal sealed class GeminiAIOptions
    {
        public const string Section = "Gemini";
        public string Model { get; init; } = "gemini-3.5-flash-lite";
        public int TimeoutInSeconds { get; init; } = 20;
        public string? ApiKey { get; init; }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
    }
}
