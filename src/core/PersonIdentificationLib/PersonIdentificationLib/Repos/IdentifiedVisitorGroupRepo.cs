using CoreLib.Repos;
using Microsoft.Azure.Documents;
using PersonIdentificationLib.Abstractions;
using PersonIdentificationLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonIdentificationLib.Repos
{
    public class IdentifiedVisitorGroupRepo : CosmosDbRepository<IdentifiedVisitorGroup>, IIdentifiedVisitorGroupRepository
    {
        //public IdentifiedVisitorRepo(ICosmosDbClientFactory cosmosDbClientFactory) : base(cosmosDbClientFactory) { }

        public IdentifiedVisitorGroupRepo(ICosmosDbClientFactory cosmosDbClientFactory) : base(cosmosDbClientFactory) 
        { }

        public override string CollectionName { get; } = AppConstants.DbColIdentifiedVisitorGroup;

        public override string GenerateId(IdentifiedVisitorGroup entity) => $"{entity.Id ?? Guid.NewGuid().ToString() + "-" + entity.PartitionKey}";

        // Initially I opted to use month-year as the partition key. You can partition the data in different way.
        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey($"{entityId.Substring(entityId.LastIndexOf('-') + 1)}");
    }
}
