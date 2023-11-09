using Newtonsoft.Json;
using Occtoo.Formatter.Newstore.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Occtoo.Formatter.Newstore.Services
{
    public interface INewStoreService
    {
        JobCreate CreateImportJob(JobImport import);
        string StartImportJob(string uri, string importId);
        Task<bool> MonitorJob(string importId, int timeoutInMinutes);
    }

    public class NewStoreService : INewStoreService
    {
        private readonly HttpClient _httpClient;
        private readonly NewstoreToken _token;
        private readonly string _newStoreApiUrl = $"https://{Environment.GetEnvironmentVariable("newStoreTenant")}.p.newstore.net/";

        public NewStoreService(IHttpClientFactory httpClientFactory)
        {
            _token = GenerateToken(Environment.GetEnvironmentVariable("newStoreUser"), 
                Environment.GetEnvironmentVariable("newStorePassword"));
            if (_token == null)
                throw new Exception($"Unable to generate token towards NewStore");

            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
        }

        public JobCreate CreateImportJob(JobImport import)
        {
            var content = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(import)));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = _httpClient.PostAsync($"{_newStoreApiUrl}v0/d/import", content).Result;
            response.EnsureSuccessStatusCode();

            return response.Content.ReadAsAsync<JobCreate>().Result;
        }

        public string StartImportJob(string uri, string importId)
        {
            object uriJson = new
            {
                transformed_uri = uri
            };

            var content = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(uriJson)));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = _httpClient.PostAsync($"{_newStoreApiUrl}v0/d/import/{importId}/start", content).Result;
            response.EnsureSuccessStatusCode();


            return response.Content.ReadAsStringAsync().Result;
        }

        public async Task<bool> MonitorJob(string importId, int timeoutInMinutes)
        {
            var cancellationTokenSource = new CancellationTokenSource(new TimeSpan(0, timeoutInMinutes, 0));
            var token = cancellationTokenSource.Token;
            var success = false;
            try
            {
                await Task.Factory.StartNew(() => PollApi(importId), token).ContinueWith(x => { success = x.Result; }, token);

            }
            catch (OperationCanceledException)
            {
                success = false;
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
            return success;
        }

        private NewstoreToken GenerateToken(string username, string password)
        {
            var dict = new Dictionary<string, string>
            {
                {"username", username},
                {"password", password},
                {"grant_type", "password"}
            };

            var tokenResponse = _httpClient.PostAsync($"{_newStoreApiUrl}v0/token?", new FormUrlEncodedContent(dict)).Result;
            tokenResponse.EnsureSuccessStatusCode();

            var token = tokenResponse.Content.ReadAsAsync<NewstoreToken>().Result;
            return token;
        }

        private bool PollApi(string importId)
        {
            var response = _httpClient.GetAsync($"{_newStoreApiUrl}v0/d/import/{importId}").Result;
            var state = response.Content.ReadAsAsync<JobImportResponse>().Result?.state;

            while (state != "finished")
            {
                response = _httpClient.GetAsync($"{_newStoreApiUrl}v0/d/import/{importId}").Result;
                state = response.Content.ReadAsAsync<JobImportResponse>().Result?.state;
                if (state == "finished")
                {
                    return true;
                }

                Thread.Sleep(3000);
            }
            return false;
        }
    }

    public class JobCreate
    {
        public string id { get; set; }
    }

    public class JobImportResponse
    {
        public string import_id { get; set; }
        public string[] entities { get; set; }
        public string source { get; set; }
        public string provider { get; set; }
        public string source_uri { get; set; }
        public string transformed_uri { get; set; }
        public long revision { get; set; }
        public string name { get; set; }
        public string received_at { get; set; }
        public string completed_at { get; set; }
        public string state { get; set; }
        public string reason { get; set; }
        public bool full { get; set; }
    }
}
