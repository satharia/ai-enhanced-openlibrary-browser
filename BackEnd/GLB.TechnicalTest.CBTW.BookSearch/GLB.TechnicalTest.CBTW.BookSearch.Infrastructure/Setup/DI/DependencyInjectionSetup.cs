using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Interfaces;
using GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.GeminiAI;
using GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.Setup.DI
{
    public static class DependencyInjectionSetup
    {
        public static IServiceCollection AddInfrastructureSetup(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddGeminiAI(configuration);
            services.AddOpenLibrary(configuration);

            return services;
        }

        private static IServiceCollection AddGeminiAI(this IServiceCollection services, IConfiguration configuration)
        {
            // In development environment, apiKey is read from userSecrets set in the api with the key "Gemini:ApiKey"
            // In production (AWS ECS), , apiKey is read from environment variable with name "Gemini__ApiKey", do not place directly, pull it from secrets manager
            services.AddOptions<GeminiAIOptions>()
                .BindConfiguration(GeminiAIOptions.Section);

            var apiKey = configuration[$"{GeminiAIOptions.Section}:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new SystemException("Missing GeminiAI ApiKey from secrets configuration");
            }

            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<GeminiAIOptions>>().Value;
                return new Client(
                    apiKey: options.ApiKey,
                    httpOptions: new HttpOptions
                    {
                        Timeout = options.TimeoutInSeconds * 1000
                    });
            });

            services.AddScoped<IGeminiAIClient, GeminiAIClient>();
            services.AddScoped<ISearchIntentExtractor, GeminiAISearchIntentExtractor>();

            return services;
        }

        private static IServiceCollection AddOpenLibrary(this IServiceCollection services, IConfiguration configuration)
        {
            // In local development, AppName and ContactEmail are provided as userSecrets with the keys OpenLibrary:AppName and OpenLibrary:ContactEmail
            // In AWS ECS, they are provided as environment variables with the keys OpenLibrary__AppName and OpenLibrary__ContactEmail
            services.AddOptions<OpenLibraryOptions>()
                .BindConfiguration(OpenLibraryOptions.Section);

            services.AddSingleton<OpenLibraryRateLimiter>();
            services.AddTransient<RateLimitingHandler>();

            services.AddHttpClient<OpenLibraryClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<OpenLibraryOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
                .AddHttpMessageHandler<RateLimitingHandler>()
                .AddStandardResilienceHandler();

            services.AddScoped<IBookCatalog, OpenLibraryBookCatalog>();

            return services;
        }
    }
}
