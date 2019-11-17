using CoreLib.Abstractions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Repos
{
    public class AzureBlobStorageRepository : IStorageRepository
    {
        CloudStorageAccount storageAccount;
        CloudBlobClient cloudBlob;
        CloudBlobContainer blobContainer;

        public AzureBlobStorageRepository(string storageName, string storageKey, string storageContainerName)
        {
            storageAccount = new CloudStorageAccount(
                                    new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                                    storageName,
                                    storageKey), true);
            
            //Preparing the storage container for blobs
            cloudBlob = storageAccount.CreateCloudBlobClient();
            blobContainer = cloudBlob.GetContainerReference(storageContainerName);
            blobContainer.CreateIfNotExistsAsync().Wait();
        }

        public AzureBlobStorageRepository(string connectionString, string storageContainerName)
        {
            var connection = CloudStorageAccount.Parse(connectionString);

            storageAccount = new CloudStorageAccount(
                                    connection.Credentials, true);

            //Preparing the storage container for blobs
            cloudBlob = storageAccount.CreateCloudBlobClient();
            blobContainer = cloudBlob.GetContainerReference(storageContainerName);
            blobContainer.CreateIfNotExistsAsync().Wait();
        }

        /// <summary>
        /// Upload file to pre-configured storage account and container.
        /// </summary>
        /// <param name="name">Unique file name with extension</param>
        /// <param name="fileData">File data</param>
        /// <returns>FQDN for the newly created file</returns>
        public async Task<string> CreateFileAsync(string name, Stream fileData)
        {
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(name);

            // Create or overwrite the file name blob with the contents of the provided stream
            await blockBlob.UploadFromStreamAsync(fileData);

            return blockBlob.Uri.AbsoluteUri;
        }

        /// <summary>
        /// Upload file to pre-configured storage account and container.
        /// </summary>
        /// <param name="name">Unique file name with extension</param>
        /// <param name="fileData">File data</param>
        /// <returns>FQDN for the newly created file</returns>
        public async Task<string> CreateFileAsync(string name, byte[] fileData)
        {
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(name);

            // Create or overwrite the file name blob with the contents of the provided stream
            await blockBlob.UploadFromByteArrayAsync(fileData, 0, fileData.Length);

            return blockBlob.Uri.AbsoluteUri;
        }

        /// <summary>
        /// Upload file to pre-configured storage account and container.
        /// </summary>
        /// <param name="name">Unique file name with extension</param>
        /// <param name="fileData">File data</param>
        /// <returns>FQDN for the newly created file</returns>
        public async Task<string> CreateFileAsync(string containerName, string fileName, Stream fileData)
        {
            var workspaceContainer = cloudBlob.GetContainerReference(containerName);
            await workspaceContainer.CreateIfNotExistsAsync();

            CloudBlockBlob blockBlob = workspaceContainer.GetBlockBlobReference(fileName);

            // Create or overwrite the file name blob with the contents of the provided stream
            await blockBlob.UploadFromStreamAsync(fileData);
            return blockBlob.Uri.AbsoluteUri;
        }

        /// <summary>
        /// Get file for pre-configured storage account and container.
        /// </summary>
        /// <param name="fileName">Unique file name with extension</param>
        /// <returns>Target file bytes</returns>
        public async Task<byte[]> GetFileAsync(string fileName)
        {
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileName);
            using (var fileStream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(fileStream);
                return fileStream.ToArray();
            }
        }

        /// <summary>
        /// Generate temporary access url (using SAS) to a particular file valid for 1 hour
        /// </summary>
        /// <param name="fileName">File name with the extension</param>
        /// <returns>FQDN with SAS to the target file</returns>
        public string GetFileDownloadUrl(string fileName)
        {
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileName);

            SharedAccessBlobPolicy adHocReadOnlyPolicy = new SharedAccessBlobPolicy()
            {
                // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Permissions = SharedAccessBlobPermissions.Read
            };

            var sas = blockBlob.GetSharedAccessSignature(adHocReadOnlyPolicy);

            return blockBlob.Uri + sas;
        }

        public async Task<byte[]> GetFileAsync(string containerName, string fileName)
        {
            var workspaceContainer = cloudBlob.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = workspaceContainer.GetBlockBlobReference(fileName);
            using (var fileStream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(fileStream);
                return fileStream.ToArray();
            }
        }
    }
}
