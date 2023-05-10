using Azure.Storage.Blobs;
using LSC.AZ204.WebAPI.Common;
using LSC.AZ204.WebAPI.Core;
using LSC.AZ204.WebAPI.Data;
using LSC.AZ204.WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;

namespace LSC.AZ204.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly AZ204DemoDbContext dbContext;

        public StorageController(IConfiguration configuration, AZ204DemoDbContext dbContext)
        {
            Configuration = configuration;
            this.dbContext = dbContext;
        }

        public IConfiguration Configuration { get; }

        [HttpPost("uploadContacts", Name = ControllerRoute.UploadContacts)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadContacts(
         IFormFile file,
         CancellationToken cancellationToken)
        {
            var isSuccess = await UploadBlobToContainer(file);
            return Ok(isSuccess);
        }


        [HttpGet("DownloadBlobs", Name = ControllerRoute.DownloadBlobs)]
        [ProducesResponseType(typeof(List<BlobDownload>),StatusCodes.Status200OK)]        
        public async Task<IActionResult> DownloadBlobs(string containerName= "demo-protected")
        {
            
            var blobUrlsWithSas = await GetBlobs(string.IsNullOrEmpty(containerName) ? "samples-workitems" : containerName);
            return Ok(blobUrlsWithSas);
        }

        private async Task<bool> UploadBlobToContainer(IFormFile file)
        {
            var containerName = "demo-protected"; //"demo-protected" or "demo-public"
            var isUploadSuccess = false;
            var uploadEntityToAdd = new CustomerContactUploads()
            {
                CreatedBy = "system",
                CreatedDate = DateTime.Now,
                IsProcessed = false
            };

            try
            {
                // Read the uploaded file into a MemoryStream
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                // Set the position of the stream to 0
                memoryStream.Position = 0;

                // Use the filename to create a unique blob name
                string blobName = $"{DateTime.Now.Ticks}_{file.FileName}";
                uploadEntityToAdd.FilePath = $"https://azureaz204storagedemo.blob.core.windows.net/{containerName}/{blobName}";

                // Upload the file to Azure Blob Storage
                var blobClient = new BlobClient(Configuration.GetConnectionString("AzureStorage"), containerName, blobName);
                await blobClient.UploadAsync(memoryStream);
                isUploadSuccess = true;

                await dbContext.CustomerContactUploads.AddAsync(uploadEntityToAdd);
                await dbContext.SaveChangesAsync();
                
            }
            catch (Exception ex)
            {
                uploadEntityToAdd.ErrorMessage = ex.Message;
                if (!isUploadSuccess)
                {
                    await dbContext.CustomerContactUploads.AddAsync(uploadEntityToAdd);
                    await dbContext.SaveChangesAsync();
                }
            }
            return true;
        }

      
        private async Task<List<BlobDownload>> GetBlobs(string containerName)
        {
            
            // Retrieve the storage account and container references
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Configuration.GetConnectionString("AzureStorage"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Retrieve a list of all blobs in the container
            List<IListBlobItem> blobItems = new List<IListBlobItem>();
            BlobContinuationToken continuationToken = null;
            do
            {
                var response = await container.ListBlobsSegmentedAsync(null, continuationToken);
                continuationToken = response.ContinuationToken;
                blobItems.AddRange(response.Results);
            }
            while (continuationToken != null);

            // Generate a SAS token for each blob and construct the URLs with the SAS tokens
            List<BlobDownload> blobUrlsWithSas = new List<BlobDownload>();
            foreach (IListBlobItem blobItem in blobItems)
            {
                if (blobItem.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)blobItem;
                    string sasToken = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy
                    {
                        Permissions = SharedAccessBlobPermissions.Read,
                        SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1) // set the expiry time for the SAS token
                    });
                    string blobUrlWithSas = string.Format("{0}{1}", blob.Uri, sasToken);
                    blobUrlsWithSas.Add(new BlobDownload() { Name = blob.Uri.ToString(), DownloadLink = blobUrlWithSas });
                }
            }

            // Return the list of URLs with the SAS tokens to the client-side code
            return blobUrlsWithSas;

        }

    }
}
