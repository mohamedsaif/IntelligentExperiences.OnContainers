using CrowdAnalyzer.Abstractions;
using CrowdAnalyzer.Models;
using CoreLib.Repos;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdAnalyzer.Repos
{
    public class VisitorsRepository : CosmosDbRepository<Visitor>, IVisitorsRepository
    {
        public VisitorsRepository(ICosmosDbClientFactory cosmosDbClientFactory) : base(cosmosDbClientFactory) { }

        public override string CollectionName { get; } = AppConstants.DbColVisitors;

        public override string GenerateId(Visitor entity) => $"{entity.Id ?? Guid.NewGuid().ToString()}";

        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey($"{entityId.Substring(entityId.LastIndexOf('-') + 1)}");
    }
}
