using GLB.TechnicalTest.CBTW.BookSearch.Domain.Common.Exceptions.Application;
using GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary.Contracts;
using Polly.Timeout;
using System.Net.Http.Json;
using System.Text.Json;

namespace GLB.TechnicalTest.CBTW.BookSearch.Infrastructure.OpenLibrary
{
    internal sealed class OpenLibraryClient(HttpClient httpClient)
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

        public async Task<SearchResponse?> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
        {
            var result = await GetResponseOrNullOnNotFoundAsync<SearchResponse>(request.BuildRequestPathWithParameters(), cancellationToken);

            return result;
        }

        private async Task<T?> GetResponseOrNullOnNotFoundAsync<T>(string path, CancellationToken cancellationToken) where T : class
        {
            try
            {
                using var response = await httpClient.GetAsync(path, cancellationToken);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode)
                    throw new ExternalServiceException("OpenLibrary", $"GET request to {path} returned {(int)response.StatusCode}");

                return await response.Content.ReadFromJsonAsync<T>(jsonSerializerOptions, cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException or TimeoutRejectedException or JsonException)
            {
                throw new ExternalServiceException("OpenLibrary", $"GET request to {path} failed with: {ex.Message}", ex);
            }
        }
    }
}
