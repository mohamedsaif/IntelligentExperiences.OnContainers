using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreLib.Repos
{
    public class CosmosDbClient : ICosmosDbClient
    {
        private readonly string _databaseName;
        private readonly string _collectionName;
        private readonly IDocumentClient _documentClient;

        public CosmosDbClient(string databaseName, string collectionName, IDocumentClient documentClient)
        {
            _databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
            _collectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
            _documentClient = documentClient ?? throw new ArgumentNullException(nameof(documentClient));
        }

        public async Task<Document> ReadDocumentAsync(string documentId, RequestOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _documentClient.ReadDocumentAsync(
                UriFactory.CreateDocumentUri(_databaseName, _collectionName, documentId), options, cancellationToken);
        }

        public async Task<Document> CreateDocumentAsync(object document, RequestOptions options = null,
            bool disableAutomaticIdGeneration = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _documentClient.CreateDocumentAsync(
                UriFactory.CreateDocumentCollectionUri(_databaseName, _collectionName), document, options,
                disableAutomaticIdGeneration, cancellationToken);
        }

        public async Task<Document> ReplaceDocumentAsync(string documentId, object document,
            RequestOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _documentClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri(_databaseName, _collectionName, documentId), document, options,
                cancellationToken);
        }

        public async Task<Document> DeleteDocumentAsync(string documentId, RequestOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _documentClient.DeleteDocumentAsync(
                UriFactory.CreateDocumentUri(_databaseName, _collectionName, documentId), options, cancellationToken);
        }

        public async Task<List<Document>> ReadAllDocumentsInCollection(int maxItemCount = 10,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new List<Document>();
            
            string continuationToken = null;

            var options = new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken, EnableCrossPartitionQuery = true };

            do
            {
                var feed = await _documentClient.ReadDocumentFeedAsync(
                    UriFactory.CreateDocumentCollectionUri(_databaseName, _collectionName),
                    options);
                continuationToken = feed.ResponseContinuation;
                result.AddRange(from Document document in feed
                                select document);
            } while (continuationToken != null);

            return result;
        }

        /// <summary>
        /// Allows filtered query of cosmos db collection
        /// </summary>
        /// <param name="from">Use the target entity name like Employees or Products for example</param>
        /// <param name="whereFilter">SQL compliant where conditions like Employee.id=@EmployeeId AND Employee.Department.Name=@DepartmentName</param>
        /// <param name="filterParams">A collection that provides values for declared parameters in the whereFilter. Like @EmployeeId and @DepartmentName</param>
        /// <returns>List of filtered documents</returns>
        public async Task<List<dynamic>> QueryDocumentsAsync(string from, string whereFilter, SqlParameterCollection filterParams)
        {
            var options = new FeedOptions { EnableCrossPartitionQuery = true };
            string queryText = $"SELECT * FROM {from} WHERE {whereFilter}";

            var query = _documentClient.CreateDocumentQuery(
                UriFactory.CreateDocumentCollectionUri(_databaseName, _collectionName),
                new SqlQuerySpec()
                {
                    QueryText = queryText,
                    Parameters = filterParams,
                }, options);
            
            var result = query.ToList();
            
            return result;
        }
    }
}