using CoreLib.Abstractions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Repos
{
    public class AzureQueueStorageRepository : IQueueRepository<CloudQueueMessage>
    {
        CloudStorageAccount storageAccount;
        CloudQueueClient queueClient;
        CloudQueue queue;

        public AzureQueueStorageRepository(string storageName, string storageKey, string queueName)
        {
            storageAccount = new CloudStorageAccount(
                                    new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                                    storageName,
                                    storageKey), true);

            //Preparing the storage queues 
            // Create the CloudQueueClient object for the storage account.
            queueClient = storageAccount.CreateCloudQueueClient();

            // Get a reference to the CloudQueue
            queue = queueClient.GetQueueReference(queueName);

            // Create the CloudQueue if it does not exist.
            queue.CreateIfNotExistsAsync().Wait();
        }

        public AzureQueueStorageRepository(string connectionString, string queueName)
        {
            var connection = CloudStorageAccount.Parse(connectionString);

            storageAccount = new CloudStorageAccount(
                                    connection.Credentials, true);

            //Preparing the storage queues 
            // Create the CloudQueueClient object for the storage account.
            queueClient = storageAccount.CreateCloudQueueClient();

            // Get a reference to the CloudQueue
            queue = queueClient.GetQueueReference(queueName);

            // Create the CloudQueue if it does not exist.
            queue.CreateIfNotExistsAsync().Wait();
        }

        public async Task<bool> DeleteMessage(CloudQueueMessage message)
        {
            // Then delete the message from the relevant queue
            await queue.DeleteMessageAsync(message);
            return true;
        }

        public async Task<CloudQueueMessage> GetMessage()
        {
            // Get the next message in the queue.
            CloudQueueMessage retrievedMessage = await queue.GetMessageAsync();
            return retrievedMessage;
        }

        public async Task<int> GetQueueLength()
        {
            // Fetch the queue attributes.
            await queue.FetchAttributesAsync();

            // Retrieve the cached approximate message count.
            int? cachedMessageCount = queue.ApproximateMessageCount;

            if (cachedMessageCount == null)
                return 0;
            return cachedMessageCount.Value;
        }

        public async Task<bool> QueueMessage(CloudQueueMessage message)
        {
            // Create a message and add it to the queue.
            await queue.AddMessageAsync(message);
            return true;
        }

        public CloudQueue GetQueue()
        {
            return queue;
        }
    }
}
