using CoreLib.Abstractions;
using CoreLib.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Repos
{
    public abstract class CosmosDbRepository<T> : IRepository<T>, IDocumentCollectionContext<T> where T : BaseModel
    {
        private readonly ICosmosDbClientFactory _cosmosDbClientFactory;

        protected CosmosDbRepository(ICosmosDbClientFactory cosmosDbClientFactory)
        {
            _cosmosDbClientFactory = cosmosDbClientFactory;
        }

        public async Task<T> GetByIdAsync(string id)
        {
            try
            {
                var cosmosDbClient = _cosmosDbClientFactory.GetClient(CollectionName);
                var document = await cosmosDbClient.ReadDocumentAsync(id, new RequestOptions
                {
                    PartitionKey = ResolvePartitionKey(id)
                });

                return JsonConvert.DeserializeObject<T>(document.ToString());
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException("Entity not found");
                }

                throw;
            }
        }

        public async Task<T> AddAsync(T entity)
        {
            try
            {
                entity.Id = GenerateId(entity);
                var cosmosDbClient = _cosmosDbClientFactory.GetClient(CollectionName);
                var document = await cosmosDbClient.CreateDocumentAsync(entity);
                return JsonConvert.DeserializeObject<T>(document.ToString());
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new InvalidOperationException("Document already exists");
                }
                else if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    //This happen when you try to access the db container while it is not provisioned yet
                    await _cosmosDbClientFactory.EnsureDbSetupAsync();

                    //Collections are created, Trying again :)
                    //this is supper dangerous. I should use Polly
                    return await AddAsync(entity);

                }

                throw;
            }
        }

        public async Task UpdateAsync(T entity)
        {
            try
            {
                var cosmosDbClient = _cosmosDbClientFactory.GetClient(CollectionName);
                await cosmosDbClient.ReplaceDocumentAsync(entity.Id, entity);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException("Entity not found");
                }

                throw;
            }
        }

        public async Task DeleteAsync(T entity)
        {
            try
            {
                var cosmosDbClient = _cosmosDbClientFactory.GetClient(CollectionName);
                await cosmosDbClient.DeleteDocumentAsync(entity.Id, new RequestOptions
                {
                    PartitionKey = ResolvePartitionKey(entity.Id)
                });
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException("Entity not found");
                }

                throw;
            }
        }

        public abstract string CollectionName { get; }
        public virtual string GenerateId(T entity) => Guid.NewGuid().ToString();
        public virtual PartitionKey ResolvePartitionKey(string entityId) => null;

        public async Task<List<T>> GetAllAsync()
        {
            try
            {
                var cosmosDbClient = _cosmosDbClientFactory.GetClient(CollectionName);
                var documents = await cosmosDbClient.ReadAllDocumentsInCollection();

                return JsonConvert.DeserializeObject<List<T>>(documents.ToString());
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException("Entity not found");
                }

                throw;
            }
        }
    }
}
