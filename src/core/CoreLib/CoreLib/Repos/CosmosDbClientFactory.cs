using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Repos
{
    public class CosmosDbClientFactory : ICosmosDbClientFactory
    {
        const int DefaultRUs = 1000;
        private readonly string _databaseName;
        private readonly IDictionary<string, string> _collectionNames;
        private readonly IDocumentClient _documentClient;

        public CosmosDbClientFactory(string databaseName, IDictionary<string, string> collectionNames, IDocumentClient documentClient)
        {
            _databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
            _collectionNames = collectionNames ?? throw new ArgumentNullException(nameof(collectionNames));
            _documentClient = documentClient ?? throw new ArgumentNullException(nameof(documentClient));
        }

        public ICosmosDbClient GetClient(string collectionName)
        {
            if (!_collectionNames.ContainsKey(collectionName))
            {
                throw new ArgumentException($"Unable to find collection: {collectionName}");
            }

            return new CosmosDbClient(_databaseName, collectionName, _documentClient);
        }

        public async Task EnsureDbSetupAsync()
        {
            await CreateDatabaseIfNotExists();
            await CreateColltionsIfNotExists();
        }

        private async Task CreateDatabaseIfNotExists()
        {
            try
            {
                await _documentClient.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseName));
            }
            catch (DocumentClientException e)
            {
                //Database do not exists! Create it
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await _documentClient.CreateDatabaseAsync(new Database { Id = _databaseName });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateColltionsIfNotExists()
        {
            foreach (var collectionName in _collectionNames)
            {
                try
                {
                    await _documentClient.ReadDocumentCollectionAsync(
                        UriFactory.CreateDocumentCollectionUri(_databaseName, collectionName.Key));
                }
                catch (DocumentClientException e)
                {
                    if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var docCollection = new DocumentCollection { Id = collectionName.Key };
                        string partionKey = collectionName.Value;

                        if (string.IsNullOrEmpty(partionKey))
                            docCollection.PartitionKey.Paths.Add($"/{partionKey}");

                        await _documentClient.CreateDocumentCollectionAsync(
                            UriFactory.CreateDatabaseUri(_databaseName),
                            docCollection,
                            new RequestOptions { OfferThroughput = DefaultRUs });
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
