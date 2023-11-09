using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Occtoo.Formatter.Newstore.Services
{
    public interface IBlobService
    {
        Task<BlobClient> UploadAsZipToBlobStorage(List<ZipHelper.ZipContentsEntry> content, string filename, string containerName);
        Uri GetServiceSasUriForBlob(string containerName, string filename, string storedPolicyName = null);
    }

    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<BlobClient> UploadAsZipToBlobStorage(List<ZipHelper.ZipContentsEntry> content, string filename, string containerName)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);

            await using var archive = ZipHelper.CreateZip(content);
            var blob = container.GetBlobClient(filename);
            await blob.UploadAsync(archive,
                new BlobHttpHeaders
                {
                    ContentType = "application/zip"
                });

            return blob;
        }

        public Uri GetServiceSasUriForBlob(string containerName, string filename, string storedPolicyName = null)
        {

            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = container.GetBlobClient(filename);

            // Check whether this BlobClient object has been authorized with Shared Key.
            if (!blobClient.CanGenerateSasUri)
                throw new Exception("Unable to Generate SasUri! BlobClient must be authorized with Shared Key credentials to create a service SAS.");

            // Create a SAS token
            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                BlobName = blobClient.Name,
                Resource = "b"
            };

            if (storedPolicyName == null)
            {
                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(22);
                sasBuilder.SetPermissions(BlobSasPermissions.Read);
            }
            else
            {
                sasBuilder.Identifier = storedPolicyName;
            }

            return blobClient.GenerateSasUri(sasBuilder);
        }
    }
}
