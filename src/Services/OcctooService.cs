using Occtoo.Formatter.Newstore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Occtoo.Formatter.Newstore.Services
{
    public interface IOcctooService
    {
        Task<List<OcctooProductVariant>> GetProductVariants(string languageCode = "en");
    }

    public class OcctooService : IOcctooService
    {
        private const int _batchSize = 500;
        private readonly string _destinationApi;
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;

        public OcctooService(IHttpClientFactory httpClientFactory, ITokenService tokenService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _tokenService = tokenService;
            _destinationApi = Environment.GetEnvironmentVariable("occtooDestinationUrl");
        }

        public async Task<List<OcctooProductVariant>> GetProductVariants(string languageCode = "en")
        {
            return await FetchAllData<OcctooProductVariant>("/productvariants", languageCode);
        }

        private async Task<List<T>> FetchAllData<T>(string endPoint, string languageCode = "en", string optionalQuery = null) where T : IOcctooBase
        {
            var lastProductId = "";
            var responses = new List<T>();
            List<T> fetchedData;
            do
            {
                fetchedData = await GetContent<T>($"{_destinationApi}{endPoint}", lastProductId, _batchSize, languageCode, optionalQuery);
                if (fetchedData == null || !fetchedData.Any())
                    break;

                responses.AddRange(fetchedData);
                lastProductId = fetchedData.Last().Id;
            } while (fetchedData.Any());

            return responses;
        }
        private async Task<List<T>> GetContent<T>(string fullDestinationApi, string lastIdPrevBatch, int batchSize, string language, string optionalQuery = null)
        {
            var queryString = $"?top={batchSize}&language={language}&sortAsc=id";
            if (!string.IsNullOrEmpty(lastIdPrevBatch))
                queryString = $"?top={batchSize}&language={language}&sortAsc=id&after={lastIdPrevBatch}";

            if (!string.IsNullOrWhiteSpace(optionalQuery))
                queryString = $"{queryString}&{optionalQuery}";

            var token = await _tokenService.GetApiToken(nameof(NewStoreExport), Environment.GetEnvironmentVariable("occtooTokenId"), Environment.GetEnvironmentVariable("occtooTokenSecret"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetFromJsonAsync<Response<T>>($"{fullDestinationApi}{queryString}");
            return response.Results;
        }
    }

    public class Response<T>
    {
        public List<T> Results { get; set; }
        public string Language { get; set; }
    }
}
