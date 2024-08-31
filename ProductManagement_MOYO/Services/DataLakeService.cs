using Azure.Storage.Blobs;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ProductManagement_MOYO.Services
{
    public class DataLakeService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public DataLakeService(IConfiguration configuration)
        {
            var accountName = configuration["Azure:StorageAccount:AccountName"];
            var tenantId = configuration["Azure:AzureAd:TenantId"];
            var clientId = configuration["Azure:AzureAd:ClientId"];
            var clientSecret = configuration["Azure:AzureAd:ClientSecret"];

            // Use ClientSecretCredential for authentication with Azure AD
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var blobUri = new Uri($"https://{accountName}.blob.core.windows.net");
            _blobServiceClient = new BlobServiceClient(blobUri, credential);

            // Retrieve the container name from configuration
            _containerName = configuration["Azure:StorageAccount:ContainerName"];
        }

        public async Task UploadFileAsync(string blobName, Stream content)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                // Reset stream position if possible
                if (content.CanSeek)
                {
                    content.Position = 0;
                }

                await blobClient.UploadAsync(content, true);
            }
            catch (InvalidOperationException ex)
            {
                // Handle stream-related exceptions
                Console.WriteLine("Stream operation failed: " + ex.Message);
                throw;  // Re-throw or handle accordingly
            }
            catch (Exception ex)
            {
                // Handle other potential exceptions
                Console.WriteLine("An error occurred: " + ex.Message);
                throw;
            }
        }


        public async Task<Stream> DownloadFileAsync(string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            try
            {
                var response = await blobClient.DownloadAsync();
                return response.Value.Content;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                // Blob not found
                return null;
            }
            catch (Exception)
            {
                // Handle other exceptions or rethrow if necessary
                throw;
            }
        }


        public async Task DeleteFileAsync(string filePath)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filePath);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<bool> FileExistsAsync(string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            try
            {
                var response = await blobClient.ExistsAsync();
                return response.Value;
            }
            catch (Exception ex)
            {
                // Handle exceptions as necessary
                return false;
            }
        }

    }
}
