using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Occtoo.Formatter.Newstore.Models;
using Occtoo.Formatter.Newstore.Services;
using System;
using System.Threading.Tasks;

namespace Occtoo.Formatter.Newstore
{
    public class NewStoreOutboxDequeue
    {
        private readonly INewStoreService _newStoreService;
        private readonly IBlobService _blobService;

        public NewStoreOutboxDequeue(INewStoreService newStoreService, IBlobService blobService)
        {
            _newStoreService = newStoreService;
            _blobService = blobService;
        }

        [FunctionName(nameof(NewStoreOutboxDequeue))]
        public async Task Run([QueueTrigger("%newStoreQueue%", Connection = "AzureWebJobsStorage")] string myQueueItem, ILogger log)
        {
            var newstoreOutboxDto = JsonConvert.DeserializeObject<NewstoreOutboxDto>(myQueueItem);
            if (newstoreOutboxDto == null)
                throw new Exception("Unable to parse json");

            if (string.IsNullOrWhiteSpace(newstoreOutboxDto.FileName))
                throw new Exception($"NewstoreOutboxDto didn't have any FileName!");

            if (string.IsNullOrWhiteSpace(newstoreOutboxDto.Locale))
                throw new Exception($"NewstoreOutboxDto didn't have any Locale!");

            var url = _blobService.GetServiceSasUriForBlob(NewStoreExport.NewstoreContainer, newstoreOutboxDto.FileName);

            if (newstoreOutboxDto.FileName.Contains(NewStoreExport.ProductEntities))
            {
                await PostImportJobAsync(NewStoreExport.ProductEntities, url.ToString(), newstoreOutboxDto.Locale, new string[] { NewStoreExport.ProductEntities });
            }

            if (newstoreOutboxDto.FileName.Contains(NewStoreExport.CategoriesEntities))
            {
                await PostImportJobAsync(NewStoreExport.CategoriesEntities, url.ToString(), newstoreOutboxDto.Locale, new string[] { NewStoreExport.CategoriesEntities });
            }
        }

        private async Task PostImportJobAsync(string type, string url, string locale, string[] entities)
        {
            var importName = GetUniqeImportName(type, locale);
            var importJson = CreateImportJson(importName, url, locale, entities);
            var jobId = _newStoreService.CreateImportJob(importJson);
            _newStoreService.StartImportJob(url.ToString(), jobId.id);
            await _newStoreService.MonitorJob(jobId.id, 1);
        }

        private static string GetUniqeImportName(string type, string locale)
        {
            return $"{type}_{locale}_{DateTime.UtcNow.ToString("O")}";
        }

        private static JobImport CreateImportJson(string importName, string sourceUri, string locale, string[] entities)
        {
            return new JobImport
            {
                Provider = "Axel Arigato",
                Name = importName,
                SourceUri = sourceUri,
                Entities = entities,
                Full = false,
                Shop = "storefront-catalog-en",
                Locale = locale
            };
        }
    }
}
