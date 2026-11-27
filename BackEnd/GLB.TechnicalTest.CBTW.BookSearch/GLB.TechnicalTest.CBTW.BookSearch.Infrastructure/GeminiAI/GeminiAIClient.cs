using GLB.TechnicalTest.CBTW.BookSearch.Domain.Common.Exceptions.Application;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.GeminiAI
{
    internal sealed class GeminiAIClient(
        Client genAIClient,
        IOptions<GeminiAIOptions> geminiAIConfiguration,
        ILogger<GeminiAIClient> logger
        ) : IGeminiAIClient
    {
        // Enums travel as camelCase strings ("books", "author") both in the schema and in the response
        private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        private static readonly JsonSchemaExporterOptions SchemaExporterOptions = new()
        {
            TreatNullObliviousAsNonNullable = true
        };

        // The schema of a response type never changes, so it is only generated once
        private static readonly ConcurrentDictionary<System.Type, JsonNode> ResponseSchemas = new();

        public async Task<T?> IssueGenerationRequestAsync<T>(string systemPrompt, string userText, CancellationToken cancellationToken) where T : class
        {
            var generationConfig = new GenerateContentConfig
            {
                SystemInstruction = new Content { Parts = [ new Part { Text = systemPrompt } ] },
                ResponseMimeType = "application/json",
                ResponseJsonSchema = GetResponseSchema(typeof(T)),
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

        private static JsonNode GetResponseSchema(System.Type responseType) =>
            ResponseSchemas.GetOrAdd(responseType, type => JsonSerializerOptions.GetJsonSchemaAsNode(type, SchemaExporterOptions));
    }
}
