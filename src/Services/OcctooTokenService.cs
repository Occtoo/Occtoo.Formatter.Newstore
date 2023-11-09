using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Occtoo.Formatter.Newstore.Services
{
    public interface ITokenService
    {
        Task<string> GetApiToken(string providerName, string clientId, string clientSecret);
    }
    public class OcctooTokenService : ITokenService
    {
        private readonly string _tokenUrl = Environment.GetEnvironmentVariable("occtooTokenUrl");
        private readonly string _partitionKey = "token";
        private readonly TableClient _tableClient;
        private readonly HttpClient _httpClient;

        public OcctooTokenService(IHttpClientFactory httpClientFactory, TableServiceClient tableServiceClient)
        {
            _httpClient = httpClientFactory.CreateClient("Default");
            _tableClient = tableServiceClient.GetTableClient("token");
            _tableClient.CreateIfNotExists();
        }

        public async Task<string> GetApiToken(string nameInTable, string clientId, string clientSecret)
        {
            var tokenResponse = await CheckForCachedTokenAsync(nameInTable);
            if (tokenResponse != null && tokenResponse.HasValue)
            {
                var validTime = DateTime.UtcNow - tokenResponse.Value.Created;
                if (validTime.TotalMinutes < 50)
                {
                    return tokenResponse.Value.AccessToken;
                }
            }

            var client = new
            {
                clientId,
                clientSecret
            };
            var tokenInformation = await GetTokenInternal(client);
            await AddTokenToCache(tokenInformation, nameInTable);
            return tokenInformation.AccessToken;
        }

        private async Task<TokenTableEntry> GetTokenInternal(object clientInformation)
        {
            var tokenResponse = await _httpClient.PostAsJsonAsync(_tokenUrl, clientInformation);
            tokenResponse.EnsureSuccessStatusCode();
            var tokenData = JsonConvert.DeserializeObject<TokenInfo>(await tokenResponse.Content.ReadAsStringAsync());

            return new TokenTableEntry
            {
                AccessToken = tokenData.accessToken
            };
        }

        private async Task AddTokenToCache(TokenTableEntry tokenTableEntry, string destinationId)
        {
            await _tableClient.DeleteEntityAsync(_partitionKey, destinationId);

            tokenTableEntry.Created = DateTime.UtcNow;
            tokenTableEntry.RowKey = destinationId;
            tokenTableEntry.PartitionKey = _partitionKey;

            await _tableClient.AddEntityAsync(tokenTableEntry);
        }

        private async Task<Azure.NullableResponse<TokenTableEntry>> CheckForCachedTokenAsync(string destinationId)
        {
            try
            { 
                return await _tableClient.GetEntityIfExistsAsync<TokenTableEntry>(_partitionKey, destinationId);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    public class TokenTableEntry : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string AccessToken { get; set; }
        public DateTime Created { get; set; }
    }

    public class TokenInfo
    {
        public string accessToken { get; set; }
        public int expiresIn { get; set; }
    }
}
