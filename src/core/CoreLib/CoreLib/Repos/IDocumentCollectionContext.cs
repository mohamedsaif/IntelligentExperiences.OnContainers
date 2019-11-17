using CoreLib.Models;
using Microsoft.Azure.Documents;

namespace CoreLib.Repos
{
    internal interface IDocumentCollectionContext<T> where T : BaseModel
    {
        string CollectionName { get; }

        string GenerateId(T entity);

        PartitionKey ResolvePartitionKey(string entityId);
    }
}