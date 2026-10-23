using GLB.TechnicalTest.CBTW.BookSearch.Domain.Common.Exceptions.Application;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.GeminiAI
{
    internal sealed class GeminiAIClient(
        Client genAIClient,
        IOptions<GeminiAIOptions> geminiAIConfiguration,
        ILogger<GeminiAIClient> logger
        ) : IGeminiAIClient
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

        private static readonly JsonSchemaExporterOptions SchemaOptions = new()
        {
            TreatNullObliviousAsNonNullable = true
        };
        private static readonly ConcurrentDictionary<System.Type, JsonNode> SchemaCache = new();
        internal static JsonNode SchemaFor(System.Type type) =>
            SchemaCache.GetOrAdd(type, static T => JsonSerializerOptions.GetJsonSchemaAsNode(T, SchemaOptions));

        public async Task<T?> IssueGenerationRequestAsync<T>(string systemPrompt, string userText, CancellationToken cancellationToken) where T : class
        {
            var generationConfig = new GenerateContentConfig
            {
                SystemInstruction = new Content { Parts = [ new Part { Text = systemPrompt } ] },
                ResponseMimeType = "application/json",
                ResponseJsonSchema = SchemaOptions,
                Temperature = 0
            };

            try
            {
                var response = await genAIClient.Models.GenerateContentAsync(
                    geminiAIConfiguration.Value.Model, userText, generationConfig, cancellationToken
                    );

                var responseText = response.Text;
                if (string.IsNullOrWhiteSpace(responseText))
                {
                    logger.LogWarning("Gemini returned a response with no text; Reason: {Reason}", response.Candidates?.FirstOrDefault()?.FinishReason);
                    return null;
                }

                return JsonSerializer.Deserialize<T>(responseText, JsonSerializerOptions);
            }
            catch (Exception ex) 
                when (ex is ApiException or HttpRequestException or JsonException)
            {
                throw new ExternalServiceException("GeminiAI", ex.Message, ex);
            }
        }
    }
}
